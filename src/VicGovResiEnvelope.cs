using Elements;
using Elements.Geometry;
using Elements.Geometry.Solids;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Dynamic;

namespace VicGovResiEnvelope
{
      public static class VicGovResiEnvelope
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <param name="input">The arguments to the execution.</param>
        /// <returns>A VicGovResiEnvelopeOutputs instance containing computed results and the model with any new elements.</returns>
        public static VicGovResiEnvelopeOutputs Execute(Dictionary<string, Model> inputModels, VicGovResiEnvelopeInputs input)
        {

            // Get site model dependency
            var siteModel = inputModels["Site"];
            var siteElement = getSite(siteModel);

            // Get Setback
            double sideSetback = GetSideSetbackFromBldgHeight(input.ProposedBuildingHeights);
            string allotmentType = input.AllotmentDesignation.ToString();
            string facingCondition = input.BuildingFacing.ToString();
            bool isCorner = false;

            double frontSetback = GetFrontSetback(allotmentType, facingCondition, isCorner, sideSetback);
            double maxHeight = GetMaxHeightAllowance(input.ProposedBuildingHeights);
            var output = new VicGovResiEnvelopeOutputs(sideSetback);

            // Boundary & sort
            var perimeter = siteElement.Perimeter.Offset(sideSetback * -1);
            var siteArea = siteElement.Area;
            var siteBoundaryProfile = new Profile(perimeter);
            List<Line> lotBoundarySegments = siteBoundaryProfile.Segments();

            // Create lotBoundary as Curve object
            var lotBoundaryCurveLoop = CreatePolylineFromLineSegments(lotBoundarySegments);

            // Get front & back lot edge and render to Hypar
            Vector3 frontLotClosestPt = input.FrontBoundary;
            var frontBoundary = CurveClosestPt(lotBoundarySegments, frontLotClosestPt);
            var rearBoundary = CurveFurthestPt(lotBoundarySegments, frontLotClosestPt);
            var sideBoundary = lotBoundarySegments.ElementAt(1);
            var frontBoundaryLength = frontBoundary.Length();
            var frontBoundaryLengthHalved = frontBoundaryLength/2;

            // Create front offset
            var translationVec =  rearBoundary.PointAt(0.5) - frontBoundary.PointAt(0.5);
            var unitTranslationVec = translationVec.Unitized();
            var frontSetbackTranslationVector = unitTranslationVec * frontSetback;
            var frontBoundaryXformed = frontBoundary.TransformedLine(new Transform(frontSetbackTranslationVector));

            // Draw lot centreline      
            var lotCentreLine = new Line(frontBoundaryXformed.PointAt(0.5), rearBoundary.PointAt(0.5));

            // Create planning Envelope Polygon
            var planningEnvelopePolgyon = CreatePlanningEnvelopePolygon(input.ProposedBuildingHeights, frontBoundaryLengthHalved).First();
            
            // Orient envelope to lot centreline
            planningEnvelopePolgyon.Transform(new Transform(lotCentreLine.TransformAt(0)));

            // Create envelope geom representation
            var envelopeProfile = new Profile(planningEnvelopePolgyon);
            var sweep = new Elements.Geometry.Solids.Sweep(envelopeProfile, lotBoundaryCurveLoop, 0, 0, 0, false);
            var extrude = new Elements.Geometry.Solids.Extrude(envelopeProfile, lotCentreLine.Length(), lotCentreLine.Direction(), false);
            var geometryRepresentation = new Representation(new List<Elements.Geometry.Solids.SolidOperation>() { extrude });
            var envelopeMaterial = new Material("envelope", new Color(0.3, 0.7, 0.7, 0.6), 0.0f, 0.0f);
            var envelopes = new List<Envelope>()
            {
              new Envelope(envelopeProfile, 0, maxHeight, lotCentreLine.Direction(), 0.0, new Transform(0,0,0), envelopeMaterial, geometryRepresentation, false, Guid.NewGuid(),"")
            };
            output.Model.AddElements(envelopes);
            return output;
        }

        private static double GetFrontSetback(string allotmentType, string facingCondition, bool isCorner, double sideSetback)
        {
            if (allotmentType == "Type_A")
            {
              if (facingCondition == "Road")
              {
                return 4.0 - sideSetback;
              }

              else if (facingCondition == "Reserve")
              {
                return 1.5 - sideSetback;
              }

              else if (facingCondition == "Other")
              {
                return 3.0 - sideSetback;
              } 
            }

            else if (allotmentType == "Type _B")
            {
              if (facingCondition == "Road")
              {
                return 4.0 - sideSetback;
              }

              else
              {
                return 1.5 - sideSetback;
              }
            }

            // Default output
            return 4.0 - sideSetback;
        }
        public static List<Polygon> CreatePlanningEnvelopePolygon(double proposedBuildingHeight, double frontBoundaryLengthHalved)
        {
            double maxHeight = GetMaxHeightAllowance(proposedBuildingHeight);
            double thirdStoreyXcoordinate = GetThirdStoreyXcoordinate(maxHeight);
            List<Vector3> envelopePtList = new List<Vector3>();
            envelopePtList.Add(Vector3.Origin);
            envelopePtList.Add(new Vector3(0, 3.6));
            envelopePtList.Add(new Vector3(1, 3.6));
            envelopePtList.Add(new Vector3(2, 6.9));
            envelopePtList.Add(new Vector3((thirdStoreyXcoordinate + 2), maxHeight));
            envelopePtList.Add(new Vector3(frontBoundaryLengthHalved, maxHeight));
            envelopePtList.Add(new Vector3(frontBoundaryLengthHalved, 0.0));
            var planningEnvelopePolgyon = new Polygon(envelopePtList);
            
            planningEnvelopePolgyon.Transform(new Transform(new Vector3(frontBoundaryLengthHalved*-1, 0, 0), 0));
            var mirrorYaxisMatrix = new Matrix(Vector3.XAxis*-1, Vector3.YAxis, Vector3.ZAxis, Vector3.Origin);
            var mirroredPolygon = planningEnvelopePolgyon.TransformedPolygon(new Transform(mirrorYaxisMatrix));

            List<Polygon> polyList = new List<Polygon>();
            polyList.Add(planningEnvelopePolgyon);
            polyList.Add(mirroredPolygon);
            var poly = Polygon.UnionAll(polyList, Vector3.EPSILON) as List<Polygon>;
            return poly;
        }
        public static double GetSideSetbackFromBldgHeight(double proposedBuildingHeight)

        {
          if (proposedBuildingHeight < 6.9)
          { 
            return ((proposedBuildingHeight - 3.6) * 0.3) + 1.0;
          }

          else if (proposedBuildingHeight >= 6.9)
          {
            return ((proposedBuildingHeight - 6.9) * 1.0) + 2.0;
          }
          // Default output
          return 1;
        }
        public static double GetMaxHeightAllowance(double proposedBuildingHeight)
        {
          if (proposedBuildingHeight > 10.0)
          {
            return (11.0);
          }
          // Default output
          return 10.0;
        }
        public static double GetThirdStoreyXcoordinate(double maxHeightAllowance)
        {
          if (maxHeightAllowance == 10.0)
          {
            return 3.1;
          }
          else if (maxHeightAllowance == 11.0)
          {
            return 4.1;
          }
          // Default output
          return 3.1;
        }
        public static Polyline CreatePolylineFromLineSegments(IList<Line> lineSegments)
        {
          List<Vector3> points = new List<Vector3>();
          foreach (Line seg in lineSegments)
          {
            points.Add(seg.PointAt(0.0));
          }
          return new Polyline(points);
        }
        public static Line CurveClosestPt(List<Line> lineSegments, Vector3 closestPt)
        {
            var sortedLineSegments = lineSegments.OrderBy(s => s.PointAt(0.5).DistanceTo(closestPt));
            var closestCrv = sortedLineSegments.First();
            return closestCrv;
        }

        public static Line CurveFurthestPt(List<Line> lineSegments, Vector3 closestPt)
        {
            var sortedLineSegments = lineSegments.OrderBy(s => s.PointAt(0.5).DistanceTo(closestPt));
            var reversed = sortedLineSegments.Reverse();
            var furthestCrv = reversed.First();
            return furthestCrv;
        }
        private static Site getSite(Model model)
        {
            var sites = getElementsOfType<Site>(model);
            if (sites == null)
            {
                return null;
            }
            sites = sites.OrderByDescending(e => e.Perimeter.Area()).ToList();
            var site = sites[0];
            return site;
        }
        private static List<T> getElementsOfType<T>(Model model)
        {
            if (model == null)
            {
                return null;
            }
            var items = new List<T>();
            items.AddRange(model.AllElementsOfType<T>());
            return items;
        }
      }
}
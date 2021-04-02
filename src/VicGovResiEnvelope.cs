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
            var siteElement = siteModel.AllElementsOfType<Site>().First();

            // Get Setback
            double setback = GetSetBackFromBldgHeight(input.ProposedBuildingHeights);
            var output = new VicGovResiEnvelopeOutputs(setback);

            // Boundary & sort
            var perimeter = siteElement.Perimeter.Offset(setback * -1);
            var siteArea = siteElement.Area;
            var siteBoundaryProfile = new Profile(perimeter);
            List<Line> lotBoundarySegments = siteBoundaryProfile.Segments();

            // Create lotBoundary as Curve object
            var lotBoundaryCurveLoop = GetPolylineFromSegments(lotBoundarySegments);

            // Get front lot edge and render to Hypar
            Vector3 frontLotClosestPt = input.FrontBoundary; // Override with UI
            var sortedLotBoundarySegments = lotBoundarySegments.OrderBy(s => s.PointAt(0.5).DistanceTo(frontLotClosestPt));
            List<Vector3> midPoints = sortedLotBoundarySegments.Select(i => i.PointAt(0.5)).ToList();
            var frontBoundary = sortedLotBoundarySegments.First();
            var sideBoundary1 = sortedLotBoundarySegments.ElementAt(1);
            var sideBoundary2 = sortedLotBoundarySegments.ElementAt(2);
            var backBoundary = sortedLotBoundarySegments.Last();
            var frontBoundaryLength = frontBoundary.Length();
            var frontBoundaryLengthHalved = frontBoundaryLength / 2;
            var sideBoundary = sortedLotBoundarySegments.ElementAt(2);
            var frontBoundaryModelCrv = new ModelCurve(frontBoundary);
            output.Model.AddElement(frontBoundaryModelCrv);

            // Draw lot centreline      
            var lotCentreLine = new Line(frontBoundary.PointAt(0.5), backBoundary.PointAt(0.5));
            var centreModelLine = new ModelCurve(lotCentreLine);
            output.Model.AddElement(centreModelLine);

            // Draw in XY plane envelope at origin
            double maxHeight = GetMaxHeightAllowance(input.ProposedBuildingHeights);
            double thirdStoreyXcoordinate = GetThirdStoreyXcoordinate(maxHeight);
            List<Vector3> envelopePtList = new List<Vector3>();
            envelopePtList.Add(Vector3.Origin);
            envelopePtList.Add(new Vector3(0*-1, 3.6));
            envelopePtList.Add(new Vector3(1*-1, 3.6));
            envelopePtList.Add(new Vector3(2*-1, 6.9));
            envelopePtList.Add(new Vector3((thirdStoreyXcoordinate + 2)*-1, maxHeight));
            envelopePtList.Add(new Vector3((frontBoundaryLengthHalved + thirdStoreyXcoordinate)*-1, maxHeight));
            envelopePtList.Add(new Vector3((frontBoundaryLengthHalved + thirdStoreyXcoordinate)*-1, 0.0));
            var planningEnvelopePolgyon = new Polygon(envelopePtList);
            planningEnvelopePolgyon.Transform(new Transform(sideBoundary1.TransformAt(0.0)));
            var planningEnvelopeModelCurves = planningEnvelopePolgyon.Segments().Select(i => new ModelCurve(i));
            output.Model.AddElements(planningEnvelopeModelCurves);

            // Create envelope geom representation
            var envelopeProfile = new Profile(planningEnvelopePolgyon);
            var sweep = new Elements.Geometry.Solids.Sweep(envelopeProfile, lotBoundaryCurveLoop, 0, 0, 0, false);
            var extrude = new Elements.Geometry.Solids.Extrude(envelopeProfile, lotCentreLine.Length(), lotCentreLine.Direction(), false);
            var geomRep2 = new Representation(new List<Elements.Geometry.Solids.SolidOperation>() { extrude });
            var envMatl = new Material("envelope", new Color(0.3, 0.7, 0.7, 0.6), 0.0f, 0.0f);
            var envelopes = new List<Envelope>()
            {
              new Envelope(envelopeProfile, 0, 50, Vector3.ZAxis, 0.0, new Transform(0,0,0), envMatl, geomRep2, false, Guid.NewGuid(),"")
            };
            output.Model.AddElements(envelopes);
            return output;
        }


        // Table 1 - Side and rear boundary setbacks table
        public static double GetSetBackFromBldgHeight(double proposedBuildingHeight)

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

        public static Polyline GetPolylineFromSegments(IList<Line> lineSegments)
        {
          List<Vector3> points = new List<Vector3>();
          foreach (Line seg in lineSegments)
          {
            points.Add(seg.PointAt(0.0));
          }
          return new Polyline(points);
        }
      }
}
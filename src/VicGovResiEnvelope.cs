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
            var sortedLotBoundarySegments = lotBoundarySegments.OrderBy(s => s.Length());
            List<Vector3> midPoints = sortedLotBoundarySegments.Select(i => i.PointAt(0.5)).ToList();

            // Create lotBoundary as Curve object
            var lotBoundaryCurveLoop = GetPolylineFromSegments(lotBoundarySegments);

            
            // Calculate lot centreline      
            var lotCentreLine = new Line(midPoints[0], midPoints[1]);
            var centreModelLine = new ModelCurve(lotCentreLine);

            // Get lot data 
            Vector3 frontLotClosestPt = Vector3.Origin; // Override with UI
            var frontBoundary = sortedLotBoundarySegments.First();
            var frontBoundaryLength = frontBoundary.Length();
            var frontBoundaryLengthHalved = frontBoundaryLength / 2;
            var sideBoundary = sortedLotBoundarySegments.ElementAt(2);

            // Draw side and rear setback envelope at origin
            double maxHeight = GetMaxHeightAllowance(input.ProposedBuildingHeights);
            double thirdStoreyXcoordinate = GetThirdStoreyXcoordinate(maxHeight);
            List<Vector3> envelopePtList = new List<Vector3>();
            envelopePtList.Add(Vector3.Origin);
            envelopePtList.Add(new Vector3(0, 3.6));
            envelopePtList.Add(new Vector3(1, 3.6));
            envelopePtList.Add(new Vector3(2, 6.9));
            envelopePtList.Add(new Vector3(thirdStoreyXcoordinate + 2, maxHeight));
            envelopePtList.Add(new Vector3(frontBoundaryLengthHalved + thirdStoreyXcoordinate, maxHeight));
            envelopePtList.Add(new Vector3(frontBoundaryLengthHalved + thirdStoreyXcoordinate, 0.0));
            var planningEnvelopePolgyon = new Polygon(envelopePtList);
            //Transform transform = new Transform(frontBoundary.Start, frontBoundary.Direction(), sideBoundary.Direction().Negate(), 0);
            //planningEnvelopePolgyon.Transform(transform);  

            // Create envelope 
            var envelopeProfile = new Profile(planningEnvelopePolgyon);
            var extrude = new Elements.Geometry.Solids.Extrude(envelopeProfile, setback, Vector3.ZAxis, false);
            var sweep = new Elements.Geometry.Solids.Sweep(envelopeProfile, lotBoundaryCurveLoop, 5, 5, 0, false);
            var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>() { extrude });
            var geomRep2 = new Representation(new List<Elements.Geometry.Solids.SolidOperation>() { sweep });
            var envMatl = new Material("envelope", new Color(0.3, 0.7, 0.7, 0.6), 0.0f, 0.0f);
            var envelopes = new List<Envelope>()
            {
              new Envelope(envelopeProfile, 0, 50, Vector3.ZAxis, 0.0, new Transform(0,0,0), envMatl, geomRep2, false, Guid.NewGuid(),"")
            };
            output.Model.AddElements(envelopes);
            var sideRearSetbackModelCurves = planningEnvelopePolgyon.Segments().Select(i => new ModelCurve(i));
            output.Model.AddElements(sideRearSetbackModelCurves);
            var modelCurves = perimeter.Select(i => new ModelCurve(i));
            output.Model.AddElements(modelCurves);

            // Outputs
            output.Model.AddElement(centreModelLine);

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
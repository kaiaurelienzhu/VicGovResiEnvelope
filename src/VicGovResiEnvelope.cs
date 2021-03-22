using Elements;
using Elements.Geometry;
using Elements.Geometry.Solids;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Dynamic;
using System.Collections;

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
          
            // Calculate lot centreline      
            var lotCentreLine = new Line(midPoints[0], midPoints[1]);
            var centreModelLine = new ModelCurve(lotCentreLine);

            var distToCentre = 0;

            // Get lot data 
            Vector3 frontLotClosestPt = Vector3.Origin; // Override with UI
            var frontBoundary = sortedLotBoundarySegments.First();
            var frontBoundaryLength = frontBoundary.Length();
            var frontBoundaryLengthHalved = frontBoundaryLength / 2;
            var sideBoundary = sortedLotBoundarySegments.ElementAt(2);

            // Draw side and rear setback envelope at origin
            List<Vector3> envelopePtList = new List<Vector3>();
            envelopePtList.Add(Vector3.Origin);
            envelopePtList.Add(new Vector3(0, 3.6, 0));
            envelopePtList.Add(new Vector3(1, 3.6, 0));
            envelopePtList.Add(new Vector3(2, 6.9, 0));
            envelopePtList.Add(new Vector3(frontBoundaryLengthHalved, 10));
            envelopePtList.Add(new Vector3(frontBoundaryLengthHalved + distToCentre, 10));
            envelopePtList.Add(new Vector3(frontBoundaryLengthHalved + distToCentre, 0));

            // Mirror envelope 


            // Transform to lot
            var planningEnvelopePolgyon = new Polygon(envelopePtList);
            Transform transform = new Transform(frontBoundary.Start, frontBoundary.Direction(), sideBoundary.Direction().Negate(), 0);
            //planningEnvelopePolgyon.Transform(transform);  

            // Create envelope 
            var envelopeProfile = new Profile(planningEnvelopePolgyon);
            var mat = new Material("Red", Colors.Red);
            var geomRep = new Representation(new List<Elements.Geometry.Solids.SolidOperation>() { extrude });
            var fndMatl = new Material("foundation", new Color(0.6, 0.60000002384185791, 0.6, 1), 0.0f, 0.0f);
            var envMatl = new Material("envelope", new Color(0.3, 0.7, 0.7, 0.6), 0.0f, 0.0f);
            var envelopes = new List<Envelope>()
            {
              new Envelope(envelopeProfile, 0, 20, Vector3.ZAxis, 0.0, new Transform(0,0,0), fndMatl, geomRep, false, Guid.NewGuid(),"")
            };
            output.Model.AddElements(envelopes);
            var sideRearSetbackModelCurves = planningEnvelopePolgyon.Segments().Select(i => new ModelCurve(i));
            output.Model.AddElements(sideRearSetbackModelCurves);
            var modelCurves = perimeter.Select(i => new ModelCurve(i));
            output.Model.AddElements(modelCurves);


            // Create planning envelope
            var mass =  new Mass(siteBoundaryProfile, input.ProposedBuildingHeights);
            output.Model.AddElements(mass);

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
      }
}
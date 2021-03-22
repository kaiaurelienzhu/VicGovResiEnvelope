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
            var output = new VicGovResiEnvelopeOutputs();
            // Default Values
            // var maxBuildingHeight = 11.0;



            // Get site model dependency
            var siteModel = inputModels["Site"];
            var siteElement = siteModel.AllElementsOfType<Site>().First();


            // Get boundary & sort
            var perimeter = siteElement.Perimeter.Offset(-input.Setback);
            var siteArea = siteElement.Area;
            var siteBoundaryProfile = new Profile(perimeter);
            List<Line> lotBoundarySegments = siteBoundaryProfile.Segments();
            var sortedLotBoundarySegments = lotBoundarySegments.OrderBy(s => s.Length());
            List<Vector3> midPoints = sortedLotBoundarySegments.Select(i => i.PointAt(0.5)).ToList();
            
            
            Vector3 frontLotClosestPt = Vector3.Origin; // Override with UI

            // Calculate centreline      
            var centreLine = new Line(midPoints[0], midPoints[1]);
            var centreModelLine = new ModelCurve(centreLine);

            var distToCentre = 0;


            // Orient to lot
            var frontBoundary = sortedLotBoundarySegments.First();
            var frontBoundaryLength = frontBoundary.Length();
            var frontBoundaryLengthHalved = frontBoundaryLength / 2;
            var sideBoundary = sortedLotBoundarySegments.ElementAt(2);

            // Side and rear setback envelope at origin
            List<Vector3> envelopePtList = new List<Vector3>();
            envelopePtList.Add(Vector3.Origin);
            envelopePtList.Add(new Vector3(0, 3.6, 0));
            envelopePtList.Add(new Vector3(1, 3.6, 0));
            envelopePtList.Add(new Vector3(2, 6.9, 0));
            envelopePtList.Add(new Vector3(frontBoundaryLengthHalved, 10));
            envelopePtList.Add(new Vector3(frontBoundaryLengthHalved + distToCentre, 10));
            envelopePtList.Add(new Vector3(frontBoundaryLengthHalved + distToCentre, 0));

            var poly = new Polygon(envelopePtList);
            Transform transform = new Transform(frontBoundary.Start, frontBoundary.Direction(), sideBoundary.Direction().Negate(), 0);
            poly.Transform(transform);  

            // Create envelope  
            var envelopeProfile = new Profile(poly);
            var mat = new Material("Red", Colors.Red);
            var geomRep = new Representation(new List<SolidOperation>() );
            var envelope = new Envelope(envelopeProfile, 0, 11, Vector3.ZAxis, 0.0, new Transform(0, 0, 0), mat, geomRep, false, Guid.NewGuid(), "");
            output.Model.AddElement(envelope);



            var sideRearSetbackModelCurves = poly.Segments().Select(i => new ModelCurve(i));
            output.Model.AddElements(sideRearSetbackModelCurves);
            var modelCurves = perimeter.Select(i => new ModelCurve(i));
            output.Model.AddElements(modelCurves);


            // Create planning envelope
            var mass =  new Mass(siteBoundaryProfile, 11);
            // output.Model.AddElements(mass);

            // Outputs
            output.Model.AddElement(centreModelLine);

            return output;
        }
      }
}
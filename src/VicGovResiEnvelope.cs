using Elements;
using Elements.Geometry;
using System.Collections.Generic;
using System;
using System.Linq;

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
            var perimeter = siteElement.Perimeter.Offset(-input.Setback);
            var profile = new Profile(perimeter);
            var mass =  new Mass(profile, 10);
            var modelCurves = perimeter.Select(p => new ModelCurve(p));

            // Generate outputs
            var output = new VicGovResiEnvelopeOutputs();
            output.Model.AddElements(modelCurves);
            output.Model.AddElements(mass);
            return output;
        }
      }
}
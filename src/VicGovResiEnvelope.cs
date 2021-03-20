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
            // Retrieve site information from incoming models
            var sites = new List<Site>();
            inputModels.TryGetValue("Site", out var model);
            if (model == null)
            {
              throw new ArgumentException("No Site found.");
            }
            sites.AddRange(model.AllElementsOfType<Site>());
            sites = sites.OrderByDescending(e => e.Perimeter.Area()).ToList();
            var output = new VicGovResiEnvelopeOutputs();

            var siteModel = inputModels["Site"];
            var siteElement = siteModel.AllElementsOfType<Site>().First();
            var perimeter = siteElement.Perimeter.Offset(-input.Setback);
            var modelCurves = perimeter.Select(p => new ModelCurve(p));
            output.Model.AddElements(modelCurves);
            return output;
        }
      }
}
// This code was generated by Hypar.
// Edits to this code will be overwritten the next time you run 'hypar init'.
// DO NOT EDIT THIS FILE.

using Elements;
using Elements.GeoJSON;
using Elements.Geometry;
using Elements.Geometry.Solids;
using Elements.Validators;
using Elements.Serialization.JSON;
using Hypar.Functions;
using Hypar.Functions.Execution;
using Hypar.Functions.Execution.AWS;
using System;
using System.Collections.Generic;
using System.Linq;
using Line = Elements.Geometry.Line;
using Polygon = Elements.Geometry.Polygon;

namespace VicGovResiEnvelope
{
    #pragma warning disable // Disable all warnings

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.1.21.0 (Newtonsoft.Json v12.0.0.0)")]
    
    public  class VicGovResiEnvelopeInputs : S3Args
    
    {
        [Newtonsoft.Json.JsonConstructor]
        
        public VicGovResiEnvelopeInputs(VicGovResiEnvelopeInputsBuildingFacing @buildingFacing, Vector3 @frontBoundary, VicGovResiEnvelopeInputsAllotmentDesignation @allotmentDesignation, Vector3 @rearBoundary, double @proposedBuildingHeights, string bucketName, string uploadsBucket, Dictionary<string, string> modelInputKeys, string gltfKey, string elementsKey, string ifcKey):
        base(bucketName, uploadsBucket, modelInputKeys, gltfKey, elementsKey, ifcKey)
        {
            var validator = Validator.Instance.GetFirstValidatorForType<VicGovResiEnvelopeInputs>();
            if(validator != null)
            {
                validator.PreConstruct(new object[]{ @buildingFacing, @frontBoundary, @allotmentDesignation, @rearBoundary, @proposedBuildingHeights});
            }
        
            this.BuildingFacing = @buildingFacing;
            this.FrontBoundary = @frontBoundary;
            this.AllotmentDesignation = @allotmentDesignation;
            this.RearBoundary = @rearBoundary;
            this.ProposedBuildingHeights = @proposedBuildingHeights;
        
            if(validator != null)
            {
                validator.PostConstruct(this);
            }
        }
    
        /// <summary>Building facing condition (declared road or recreation reserve on the other side of the street and opposite the allotment)</summary>
        [Newtonsoft.Json.JsonProperty("Building facing", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public VicGovResiEnvelopeInputsBuildingFacing BuildingFacing { get; set; } = VicGovResiEnvelopeInputsBuildingFacing.Other;
    
        /// <summary>A point which will be used to select the nearest boundary edge</summary>
        [Newtonsoft.Json.JsonProperty("Front Boundary", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Vector3 FrontBoundary { get; set; }
    
        /// <summary>Designation of the allotment in the subdivision permit</summary>
        [Newtonsoft.Json.JsonProperty("Allotment designation", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public VicGovResiEnvelopeInputsAllotmentDesignation AllotmentDesignation { get; set; } = VicGovResiEnvelopeInputsAllotmentDesignation.Type_A;
    
        /// <summary>A point which will be used to select the nearest boundary edge</summary>
        [Newtonsoft.Json.JsonProperty("Rear Boundary", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Vector3 RearBoundary { get; set; }
    
        [Newtonsoft.Json.JsonProperty("Proposed Building Heights", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.Range(3.6D, 11D)]
        public double ProposedBuildingHeights { get; set; } = 3.6D;
    
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.1.21.0 (Newtonsoft.Json v12.0.0.0)")]
    public enum VicGovResiEnvelopeInputsBuildingFacing
    {
        [System.Runtime.Serialization.EnumMember(Value = @"Road")]
        Road = 0,
    
        [System.Runtime.Serialization.EnumMember(Value = @"Reserve")]
        Reserve = 1,
    
        [System.Runtime.Serialization.EnumMember(Value = @"Other")]
        Other = 2,
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.1.21.0 (Newtonsoft.Json v12.0.0.0)")]
    public enum VicGovResiEnvelopeInputsAllotmentDesignation
    {
        [System.Runtime.Serialization.EnumMember(Value = @"Type A")]
        Type_A = 0,
    
        [System.Runtime.Serialization.EnumMember(Value = @"Type B")]
        Type_B = 1,
    
    }
}
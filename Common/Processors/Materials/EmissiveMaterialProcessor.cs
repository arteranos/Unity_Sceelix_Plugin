using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Materials
{
    [Processor("EmissiveMaterial")]
    public class EmissiveMaterialProcessor : MaterialProcessor
    {
        public override Material Process(IGenerationContext context, JToken jtoken)
        {
            Material emissiveMaterial = GetStandardMaterial();

            emissiveMaterial.EnableKeyword("_EMISSION");
            JToken props = jtoken["Properties"];
            Color baseColor = props["DefaultColor"].ToColor();

            // Upscale (or downscale) with the HDR intensity... if it is there.
            JToken intensityProp = props["Intensity"];
            float intensity = intensityProp != null ? Mathf.Pow(2, (float)intensityProp) : 1.0f;
            Color color = new(baseColor.r * intensity, baseColor.g * intensity, baseColor.b * intensity, baseColor.a);

            emissiveMaterial.color = baseColor;
            emissiveMaterial.SetColor("_EmissionColor", color);
            emissiveMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;

            return emissiveMaterial;
        }
    }
}
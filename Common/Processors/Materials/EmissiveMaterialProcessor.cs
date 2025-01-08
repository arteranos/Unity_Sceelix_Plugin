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
            emissiveMaterial.SetColor("_EmissionColor", jtoken["Properties"]["DefaultColor"].ToColor());
            emissiveMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;

            return emissiveMaterial;
        }
    }
}
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Materials
{
    [Processor("TextureAndBumpMaterial")]
    public class TextureAndBumpMaterialMaterialProcessor : MaterialProcessor
    {
        public override Material Process(IGenerationContext context, JToken jtoken)
        {
            Material textureAndBump = GetStandardMaterial();

            textureAndBump.SetTexture("_MainTex", CreateOrGetTexture(context, jtoken["Properties"]["DiffuseTexture"]));
            textureAndBump.SetTexture("_MetallicGlossMap", CreateOrGetTexture(context, jtoken["Properties"]["SpecularTexture"]));
            textureAndBump.SetTexture("_BumpMap", CreateOrGetTexture(context, jtoken["Properties"]["NormalTexture"], true));
            textureAndBump.EnableKeyword("_NORMALMAP");

            return textureAndBump;
        }
    }
}
using System;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Materials
{
    [Processor("CustomMaterial")]
    public class CustomMaterialProcessor : MaterialProcessor
    {
        public override Material Process(IGenerationContext context, JToken jtoken)
        {
            var shaderName = jtoken["Shader"].ToObject<String>();

            Material customMaterial = new Material(Shader.Find(shaderName));


            foreach (JToken propertyToken in jtoken["Properties"].Children())
            {
                var propertyName = propertyToken["Name"].ToObject<String>();
                var propertyType = propertyToken["Type"].ToObject<String>();
                JToken jToken = propertyToken["Value"];
                switch (propertyType)
                {
                    case "TextureSlot":
                        var textureType = jToken["Type"].ToObject<String>();
                        string textureName = (string) jToken["Name"];
                        bool isNormal = textureType == "Normal";

                        // If there's an empty texture slot, skip it and leave it on the shader's defaults.
                        if(!string.IsNullOrEmpty(textureName))
                            customMaterial.SetTexture(propertyName, CreateOrGetTexture(context, jToken, isNormal));
                        break;
                    case "Boolean":
                        var status = jToken.ToObject<bool>();
                        if (status)
                            customMaterial.EnableKeyword(propertyName);
                        else
                            customMaterial.DisableKeyword(propertyName);
                        break;
                    case "Color":
                        customMaterial.SetColor(propertyName, jToken.ToColor());
                        break;
                    case "Int32":
                        customMaterial.SetInt(propertyName, jToken.ToObject<int>());
                        break;
                    case "Single":
                        customMaterial.SetFloat(propertyName, jToken.ToObject<float>());
                        break;
                    case "Vector4":
                        customMaterial.SetVector(propertyName, jToken.ToVector4());
                        break;
                    case "String":
                        customMaterial.SetOverrideTag(propertyName, jToken.ToObject<String>());
                        break;
                }
            }

            return customMaterial;
        }
    }
}
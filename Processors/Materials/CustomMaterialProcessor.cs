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
            string shaderName = jtoken["Shader"].ToObject<String>();

            Material customMaterial = new(Shader.Find(shaderName));


            foreach (JToken propertyToken in jtoken["Properties"].Children())
            {
                string propertyName = propertyToken["Name"].ToObject<String>();
                string propertyType = propertyToken["Type"].ToObject<String>();
                JToken valueToken = propertyToken["Value"];
                switch (propertyType)
                {
                    case "TextureSlot":
                        string textureType = valueToken["Type"].ToObject<String>();
                        string textureName = (string) valueToken["Name"];

                        // If there's an empty texture slot, skip it and leave it on the shader's defaults.
                        if(!string.IsNullOrEmpty(textureName))
                            customMaterial.SetTexture(propertyName, CreateOrGetTexture(context, valueToken, textureType == "Normal"));
                        break;
                    case "Boolean":
                        bool status = valueToken.ToObject<bool>();
                        if (status)
                            customMaterial.EnableKeyword(propertyName);
                        else
                            customMaterial.DisableKeyword(propertyName);
                        break;
                    case "Color":
                        customMaterial.SetColor(propertyName, valueToken.ToColor());
                        break;
                    case "Int32":
                        customMaterial.SetInt(propertyName, valueToken.ToObject<int>());
                        break;
                    case "Single":
                        customMaterial.SetFloat(propertyName, valueToken.ToObject<float>());
                        break;
                    case "Vector4":
                        customMaterial.SetVector(propertyName, valueToken.ToVector4());
                        break;
                    case "String":
                        customMaterial.SetOverrideTag(propertyName, valueToken.ToObject<String>());
                        break;
                }
            }

            return customMaterial;
        }
    }
}
using System;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Materials
{
    public abstract class MaterialProcessor
    {
        public abstract Material Process(IGenerationContext context, JToken jtoken);


        public static Texture CreateOrGetTexture(IGenerationContext context, JToken textureToken, bool setAsNormal = false)
        {
            if (textureToken == null)
                return null;

            var name = textureToken["Name"].ToObject<String>();

            return context.CreateOrGetAssetOrResource(name + ".asset", () => textureToken["Content"].ToTexture(setAsNormal));
        }
    }
}

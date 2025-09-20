using System.Linq;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Components
{
    [Processor("Light")]
    public class LightProcessor : ComponentProcessor
    {
        public override void Process(IGenerationContext context, GameObject gameObject, JToken jtoken)
        {
            Light light = gameObject.AddComponent<Light>();

            JToken properties = jtoken["Properties"];
            light.type = properties["LightType"].ToEnum<LightType>();
            light.range = properties["Range"].ToObject<float>();
            light.color = properties["Color"].ToColor();
            light.intensity = properties["Intensity"].ToObject<float>();
            light.bounceIntensity = properties["Bounce Intensity"].ToObject<float>();
            light.renderMode = properties["Render Mode"].ToEnum<LightRenderMode>();
            light.shadows = properties["Shadow Type"].ToEnum<LightShadows>();

            // Supported since Sceelix 1.1.0
            if (properties["Bake Type"] != null)
                light.lightmapBakeType = properties["Bake Type"].ToEnum<LightmapBakeType>();
        }
    }
}
using System;
using System.IO;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Materials
{
    [Processor("RemoteMaterial")]
    public class RemoteMaterialProcessor : MaterialProcessor
    {
        public override Material Process(IGenerationContext context, JToken jtoken)
        {
            var path = jtoken["Properties"]["Path"].ToObject<String>();

            if (string.IsNullOrEmpty(Path.GetExtension(path)))
                path = path + ".mat";

            var remoteMaterial = context.GetExistingResource<Material>(path);
            if(remoteMaterial == null)
            {
                Debug.LogWarning(String.Format("Could not find material with the path {0}.", path));
                return GetStandardMaterial();
            }

            // Copy the material to detach the asset store entry
            return new Material(remoteMaterial);
        }
    }
}

using System.Linq;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Components
{
    [Processor("Mesh Collider")]
    public class MeshColliderProcessor : ComponentProcessor
    {
        public override void Process(IGenerationContext context, GameObject gameObject, JToken jtoken)
        {
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();

            //if a meshCollider already exists, this will be null, so don't overwrite it
            if (meshCollider == null)
                return;

            var properties = jtoken["Properties"];

            JToken meshToken = properties["Mesh"];

            if (meshToken != null)
                meshCollider.sharedMesh = Utils.GetMesh(context, meshToken["MeshFilter"]["Mesh"]);
            else
                meshCollider.sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;

            meshCollider.convex = properties["IsConvex"].ToObject<bool>();
            meshCollider.isTrigger = properties["IsTrigger"].ToObject<bool>();
        }
    }
}
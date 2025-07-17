using System;
using System.Linq;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Sceelix.Processors.Components
{
    public static class Utils
    {
        public static Mesh GetMesh(IGenerationContext context, JToken meshToken)
        {
            var meshName = meshToken["Name"].ToObject<String>();
            var newMesh = context.CreateOrGetAssetOrResource<Mesh>(meshName + ".asset", () =>
            {
                var mesh = new Mesh();

                mesh.vertices = meshToken["Positions"].Children().Select(x => JTokenExtensions.ToVector3(x)).ToArray();
                mesh.normals = meshToken["Normals"].Children().Select(x => x.ToVector3()).ToArray();
                mesh.colors = meshToken["Colors"].Children().Select(x => x.ToColor()).ToArray();
                mesh.uv = meshToken["UVs"].Children().Select(x => x.ToVector2()).ToArray();
                //mesh.uv2 = meshToken["UVs"].Children().Select(x => x.ToVector2()).ToArray();
                mesh.tangents = meshToken["Tangents"].Children().Select(x => x.ToVector4()).ToArray();

                var triangleSetList = meshToken["Triangles"].Children().ToList();

                mesh.subMeshCount = triangleSetList.Count;

                for (int index = 0; index < triangleSetList.Count; index++)
                {
                    int[] subList = triangleSetList[index].Children().Select(x => x.ToObject<int>()).ToArray();

                    int highest = subList.Max();
                    if (highest > UInt16.MaxValue)
                    {
                        if (SystemInfo.supports32bitsIndexBuffer)
                            Debug.LogError($"{meshName}: More than 65535 vertices, which is not supported in this platform. Split up your meshes into smaller parts.");
                        else
                            Debug.LogWarning($"{meshName}: More than 65535 vertices, which may be unsupported on other platform. Suggest to split up your meshes.");
                    }
                    mesh.SetTriangles(subList, index);
                }

                return mesh;
            });
            return newMesh;
        }
    }

    [Processor("MeshFilter")]
    public class MeshFilterProcessor : ComponentProcessor
    {
        public override void Process(IGenerationContext context, GameObject gameObject, JToken jtoken)
        {
            GameObject realGameObject = gameObject;

            //if a meshfilter already exists, don't overwrite it
            if (gameObject.GetComponent<MeshFilter>() != null)
                gameObject = new GameObject();

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();

            if (meshFilter == null)
                return;

            var meshToken = jtoken["Mesh"];

            meshFilter.sharedMesh = Utils.GetMesh(context, meshToken);

            if (realGameObject != gameObject)
                Object.DestroyImmediate(gameObject);
        }
    }
}
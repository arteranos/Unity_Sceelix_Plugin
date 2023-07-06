using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Sceelix.Processors.Components
{
    [Processor("Terrain")]
    public class TerrainProcessor : ComponentProcessor
    {
        public override void Process(IGenerationContext context, GameObject gameObject, JToken jtoken)
        {
            //if a Terrain already exists, don't overwrite it
            if (gameObject.GetComponent<Terrain>() != null)
                return;

            var heightsResolution = jtoken["HeightsResolution"].ToVector2();
            var heightsBytes = jtoken["Heights"].ToObject<byte[]>();
            var heights = heightsBytes.ToTArray<float>((int) heightsResolution.x, (int) heightsResolution.y);
            var resolution = jtoken["Resolution"].ToObject<int>();
            var sizes = jtoken["Size"].ToVector3();

            // AltspaceVR specific: Copy the default material to avoid Altspace using a cut down version
            Material terrainMat = context.CreateOrGetAssetOrResource("Sceelix Terrain Default.mat", () =>
            {
                Shader shad = null;

                shad = Shader.Find("Universal Render Pipeline/Terrain/Lit");
                if(shad == null)
                    shad = Shader.Find("Nature/Terrain/Standard");

                Material mat = new Material(shad);
                return mat;
            });

            TerrainData newTerrain = context.CreateOrGetAssetOrResource<TerrainData>("Sceelix Terrain.asset", () =>
            {
                //initialize the terrain data instance and set height data
                //unfortunately unity terrain maps have to be square and the sizes must be powers of 2
                TerrainData terrainData = new TerrainData();

                terrainData.heightmapResolution = resolution;
                terrainData.alphamapResolution = resolution;
                terrainData.size = sizes;
                terrainData.SetHeights(0, 0, heights);

                var materialToken = jtoken["Material"];
                if (materialToken != null)
                {
                    var defaultTexture = Texture2D.whiteTexture.ToMipmappedTexture();
                    List<TerrainLayer> terrainLayers = new List<TerrainLayer>();

                    int layerNum = 0;
                    foreach(JToken layerToken in materialToken["Layers"].Children())
                    {
                        var tileSize = layerToken["TileSize"].ToVector2();

                        var textureToken = layerToken["Texture"];

                        // var name = textureToken["Name"].ToObject<String>();
                        TerrainLayer layer = context.CreateOrGetAssetOrResource($"Layer_{layerNum}.asset",
                            () => new TerrainLayer()
                            {
                                diffuseTexture = Texture2DExtensions.CreateOrGetTexture(context, layerToken["DiffuseMap"]) ?? defaultTexture,
                                normalMapTexture = Texture2DExtensions.CreateOrGetTexture(context, layerToken["NormalMap"], true),
                                tileSize = tileSize
                            });

                        terrainLayers.Add(layer);
                        layerNum++;

                        //terrainLayers.Add(new TerrainLayer()
                        //{
                        //    diffuseTexture = Texture2DExtensions.CreateOrGetTexture(context, layerToken["DiffuseMap"]) ?? defaultTexture,
                        //    normalMapTexture = Texture2DExtensions.CreateOrGetTexture(context, layerToken["NormalMap"], true),
                        //    tileSize = new Vector2(terrainData.size.x / tileSize.x, terrainData.size.z / tileSize.y)
                        //});
                    }

                    terrainData.terrainLayers = terrainLayers.ToArray();
                    terrainData.RefreshPrototypes();
                }

                return terrainData;
            });

            var materialToken = jtoken["Material"];

            var splatMapSize = materialToken["SplatmapSize"].ToVector3();
            var splatMapBytes = materialToken["Splatmap"].ToObject<byte[]>();
            float[,,] splatMap = splatMapBytes.ToTArray<float>((int) splatMapSize.x, (int) splatMapSize.y, (int) splatMapSize.z);
            newTerrain.SetAlphamaps(0, 0, splatMap);
            context.RefreshAssets();


            //finally, create the terrain components
            Terrain terrain = gameObject.AddComponent<Terrain>();
            TerrainCollider collider = gameObject.AddComponent<TerrainCollider>();

            terrain.terrainData = newTerrain;
            terrain.materialTemplate = terrainMat;
            collider.terrainData = newTerrain;
        }
    }
}
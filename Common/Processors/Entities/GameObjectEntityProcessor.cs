using System;
using System.Collections.Generic;
using System.IO;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Processors.Components;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Entities
{
    [Processor("UnityEntity")]
    public class GameObjectEntityProcessor : EntityProcessor
    {
        //in order to avoid infinite loops, we have to define these fields statically
        private static readonly Dictionary<String, EntityProcessor> _entityProcessors = ProcessorAttribute.GetClassesOfType<EntityProcessor>();

        Dictionary<string, ComponentProcessor> _componentProcessors = ProcessorAttribute.GetClassesOfType<ComponentProcessor>();

        public override IEnumerable<GameObject> Process(IGenerationContext context, JToken entityToken)
        {
            //first of all, let's see if we are loading a prefab
            var prefabPath = entityToken["Prefab"].ToTypeOrDefault<String>();
            var scaleMode = entityToken["ScaleMode"].ToTypeOrDefault<String>();

            GameObject gameObject;

            //if a prefab instruction is passed, load it
            if (!String.IsNullOrEmpty(prefabPath))
            {
                if (!prefabPath.StartsWith("Assets/"))
                    prefabPath = "Assets/" + prefabPath;

                //make sure the extension is set
                prefabPath = Path.ChangeExtension(prefabPath, ".prefab");

                gameObject = context.InstantiatePrefab(prefabPath);

                if (gameObject == null)
                {
                    gameObject = new GameObject();
                    Debug.LogWarning(String.Format("Could not create instance of prefab {0}. Please verify that it exists in the requested location.", prefabPath));
                    prefabPath = String.Empty;
                }
            }
            else
            {
                gameObject = new GameObject();
            }

            gameObject.name = entityToken["Name"].ToObject<String>();
            gameObject.isStatic = entityToken["Static"].ToTypeOrDefault<bool>();
            gameObject.SetActive(entityToken["Enabled"].ToTypeOrDefault<bool>());


            var tag = entityToken["Tag"].ToTypeOrDefault<String>();
            if (!String.IsNullOrEmpty(tag))
                context.AddTag(gameObject, tag);

            var layer = entityToken["Layer"].ToTypeOrDefault<String>();
            if (!String.IsNullOrEmpty(layer))
            {
                var layerValue = LayerMask.NameToLayer(layer);

                //unfortunately we can't create the layer programmatically, so
                if (layerValue < 0)
                    throw new ArgumentException("Layer '" + layer + "' is not defined. It must be created manually in Unity first.");

                gameObject.layer = layerValue;
            }


            gameObject.transform.position = entityToken["Position"].ToVector3();
            gameObject.transform.rotation *= Quaternion.LookRotation(entityToken["ForwardVector"].ToVector3(), entityToken["UpVector"].ToVector3());

            //if this is a prefab, we need to make its size and position match the same size
            if (!String.IsNullOrEmpty(prefabPath))
            {
                //try to get the bounds of the object
                var objectBounds = GetObjectBounds(gameObject);
                if (objectBounds.HasValue)
                {
                    var objectSize = objectBounds.Value.size;
                    var intendedSize = entityToken["Size"].ToVector3();

                    if (scaleMode == "Stretch To Fill")
                    {
                        var scale = new Vector3(1 / objectSize.x, 1 / objectSize.y, 1 / objectSize.z);

                        gameObject.transform.localScale = Vector3.Scale(intendedSize, scale);
                    }
                    else if (scaleMode == "Scale To Fit")
                    {
                        var scale = new Vector3(intendedSize.x / objectSize.x, intendedSize.y / objectSize.y, intendedSize.z / objectSize.z);
                        var minCoordinate = Math.Min(Math.Min(scale.x, scale.y), scale.z);
                        var newScale = new Vector3(minCoordinate, minCoordinate, minCoordinate);

                        gameObject.transform.localScale = newScale;
                    }

                    gameObject.transform.Translate(Vector3.Scale(-objectBounds.Value.min, gameObject.transform.localScale));

                }
            }
            else
            {
                var intendedSize = entityToken["Scale"].ToVector3();
                gameObject.transform.localScale = intendedSize;
            }

            // Since Sceelix 1.2.0: Support of child actors. Build up _before_ setting up the components.
            JToken subEntities = entityToken["SubEntities"];
            if (subEntities != null)
            {
                foreach (JToken subEntityToken in entityToken["SubEntities"].Children())
                {
                    EntityProcessor entityProcessor;

                    //if there is a processor for this entity Type, call it
                    if (_entityProcessors.TryGetValue(subEntityToken["EntityType"].ToObject<String>(), out entityProcessor))
                    {
                        var childGameObjects = entityProcessor.Process(context, subEntityToken);
                        foreach (GameObject childGameObject in childGameObjects)
                        {
                            childGameObject.transform.parent = gameObject.transform;
                        }
                    }
                    else
                    {
                        Debug.LogWarning(String.Format("There is no defined processor for entity type {0}.", entityToken["EntityType"]));
                    }
                }
            }

            //now, iterate over the components
            //and look for the matching component processor
            foreach (JToken jToken in entityToken["Components"].Children())
            {
                ComponentProcessor componentProcessorAttribute;

                if (_componentProcessors.TryGetValue(jToken["ComponentType"].ToObject<String>(), out componentProcessorAttribute))
                    componentProcessorAttribute.Process(context, gameObject, jToken);
                else
                {
                    Debug.LogWarning(String.Format("There is no defined processor for component type {0}.", jToken["ComponentType"]));
                }
            }

            yield return gameObject;
        }



        private static Bounds? GetObjectBounds(GameObject gameObject)
        {
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                return meshFilter.sharedMesh.bounds;
            }
            var terrain = gameObject.GetComponent<TerrainCollider>();
            if (terrain != null)
            {
                return terrain.bounds;
            }
            var collider = gameObject.GetComponent<Collider>();
            if (collider != null)
            {
                return collider.bounds;
            }

            return null;
        }
    }
}
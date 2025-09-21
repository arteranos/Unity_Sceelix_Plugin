using System;
using System.Reflection;
using Assets.Sceelix.Contexts;
using Assets.Sceelix.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Processors.Components
{
    [Processor("Custom")]
    public class CustomProcessor : ComponentProcessor
    {
        public override void Process(IGenerationContext context, GameObject gameObject, JToken jtoken)
        {
            // Look for the 'default' assembly holding the Unity scripts
            Assembly ass = null;
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                // Debug.Log($"Assembly: {a.GetName().Name}");
                if (a.GetName().Name == "Sceelix.User")
                {
                    ass = a;
                    break;
                }
            }

            if (ass == null)
            {
                Debug.LogWarning("There is no 'Sceelix.User' assembly. Create an assembly definition file named 'Sceelix.User' and place the scripts in its directory.");
                return;
            }

            var componentName = jtoken["ComponentName"].ToObject<String>();

            //if the optimized processor does not exist, simply go for the slower, but very generic Reflection approach
            var componentType = ass.GetType(componentName);

            if (componentType == null)
                Debug.LogWarning($"Component '{componentName}' is not defined in the 'Sceelix.User' assembly.");
            else
            {
                Component customComponent = gameObject.AddComponent(componentType);

                var properties = jtoken["Properties"];

                foreach (var genericProperty in properties.Children())
                {
                    var propertyFieldName = genericProperty["Name"].ToObject<String>();
                    var propertyFieldType = Type.GetType(genericProperty["Type"].ToObject<String>());

                    //the indicated value can be field or property - try field first
                    FieldInfo fieldInfo = componentType.GetField(propertyFieldName);
                    if (fieldInfo != null)
                    {
                        fieldInfo.SetValue(customComponent, genericProperty["Value"].ToObject(propertyFieldType));
                    }
                    else
                    {
                        //otherwise, try property and let the user know if it failed
                        PropertyInfo propertyInfo = componentType.GetProperty(propertyFieldName);
                        if (propertyInfo != null)
                            propertyInfo.SetValue(customComponent, genericProperty["Value"], null);
                        else
                            Debug.LogWarning(String.Format("Property/Field '{0}' for component '{1}' is not defined.", propertyFieldName, componentName));
                    }
                }
            }
        }
    }
}
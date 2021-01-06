using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using SceneSerialization.Storage;

namespace SceneSerialization.Utility 
{
    public class DataConverter
    {
        private static readonly Type[] _acceptableMethodParameters = new[] { typeof(object), typeof(SerializableFieldData), typeof(Action).MakeByRefType() };
        private static bool _initialized = false;

        public static SurrogateSelector SurrogateSelector { get; set; } = new SurrogateSelector();



        public static ConverterMethodInfoPair PopulateMethodData()
        {
            ConverterMethodInfoPair methods = MethodInfoStorage.Instance.methods;
            if (_initialized)
                return methods;
            if (methods.Count == 0)
            {
                foreach (var method in DataConverterUtility.GetExtensionMethods())
                {
                    DataConverterUtility.OtherMethodIsInvocable(_acceptableMethodParameters, method, out InvocabilityStatus status);
                    if (status == InvocabilityStatus.InvokeAsExtension)
                        methods.Add(new TypeKeyWrapper(method.Name, method.GetParameters()[1].ParameterType), new MethodInfoValueWrapper(method, true));
                }
                foreach (var method in DataConverterUtility.GetBaseMethods())
                {
                    DataConverterUtility.OtherMethodIsInvocable(_acceptableMethodParameters, method, out InvocabilityStatus status);
                    if (status == InvocabilityStatus.Invoke)
                        methods.Add(new TypeKeyWrapper(method.Name, method.GetParameters()[0].ParameterType), new MethodInfoValueWrapper(method, false));
                }
            }
            return methods;
        }



        #region Generic-like Serializer Transport
        public static void SerializeIntoData(object instance, SerializableFieldData data, ref Action afterSerialization)
        {
            const string methodName = "SerializeIntoData";
            object[] methodParameters = new[] { instance, data, afterSerialization };
            object[] extensionMethodParameters = new[] { null, instance, data, afterSerialization };

            ConverterMethodInfoPair methods = PopulateMethodData();
            TypeKeyWrapper key = new TypeKeyWrapper(methodName, instance.GetType());
            if (methods.TryGetValue(key, out MethodInfoValueWrapper value))
            {
                if (value.isExtensionMethod)
                {
                    value.methodInfo.Invoke(new DataConverter(), extensionMethodParameters);
                    afterSerialization = extensionMethodParameters[3] as Action;
                }
                else
                {
                    value.methodInfo.Invoke(new DataConverter(), methodParameters);
                    afterSerialization = methodParameters[2] as Action;
                }
            }
            else if (instance is MonoBehaviour)
            {
                key = new TypeKeyWrapper(methodName, typeof(MonoBehaviour));
                if (methods.TryGetValue(key, out value))
                {
                    value.methodInfo.Invoke(new DataConverter(), methodParameters);
                    afterSerialization = methodParameters[2] as Action;
                }
            }
        }
        public static void DeserializeIntoInstance(object instance, SerializableFieldData data, ref Action afterDeserialization)
        {
            const string methodName = "DeserializeIntoInstance";
            object[] methodParameters = new[] { instance, data, afterDeserialization };
            object[] extensionMethodParameters = new[] { null, instance, data, afterDeserialization };

            ConverterMethodInfoPair methods = PopulateMethodData();
            TypeKeyWrapper key = new TypeKeyWrapper(methodName, instance.GetType());
            if (methods.TryGetValue(key, out MethodInfoValueWrapper value))
            {
                if (value.isExtensionMethod)
                {
                    value.methodInfo.Invoke(new DataConverter(), extensionMethodParameters);
                    afterDeserialization = extensionMethodParameters[3] as Action;
                }
                else
                {
                    value.methodInfo.Invoke(new DataConverter(), methodParameters);
                    afterDeserialization = methodParameters[2] as Action;
                }
            }
            else if (instance is MonoBehaviour)
            {
                key = new TypeKeyWrapper(methodName, typeof(MonoBehaviour));
                if (methods.TryGetValue(key, out value))
                {
                    value.methodInfo.Invoke(new DataConverter(), methodParameters);
                    afterDeserialization = methodParameters[2] as Action;
                }
            }
        }
        #endregion



        #region MonoBehaviour Serializer
        private void SerializeIntoData(MonoBehaviour instance, SerializableFieldData data, ref Action afterSerialization)
        {
            FieldInfo[] fields = instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                if (field == null || field.FieldType.GetCustomAttributes(typeof(DontSaveField), true).Length > 0)
                    continue;

                object value = field.GetValue(instance);
                if (value is UnityObject)
                {
                    if ((value as GameObject) && PrefabStorage.Instance.IsPrefab(value as GameObject, out string key))
                        data[field.Name] = key;
                    else if ((value as UnityObject) && AssetStorage.Instance.SupportsType(value.GetType()))
                    {
                        key = AssetStorage.Instance.StoreAsset(value as UnityObject);
                        data[field.Name] = key;
                    }
                    else
                    {
                        string name = field.Name;
                        afterSerialization += delegate
                        {
                            if ((value as GameObject) && SerializableReference.ReferencesByUnityObject.TryGetValue(value as UnityObject, out string id))
                                data[name] = id;
                        };
                    }
                }
                else if (DataConverterUtility.FieldIsValid(field, SurrogateSelector))
                    data[field.Name] = value;
            }
        }
        private void DeserializeIntoInstance(MonoBehaviour instance, SerializableFieldData data, ref Action afterDeserialization)
        {
            foreach (var pair in data)
            {
                FieldInfo field = instance.GetType().GetField(pair.Key, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.SetField);
                if (field == null)
                    continue;

                object value = pair.Value;
                if (value is string && PrefabStorage.Instance.Prefabs.ContainsKey(value as string))
                {
                    GameObject prefab = PrefabStorage.Instance.RetrieveGameObject(value as string);
                    field.SetValue(instance, prefab);
                }
                else if (value is string && AssetStorage.Instance.SupportsType(field.FieldType))
                {
                    UnityObject asset = AssetStorage.Instance.RetrieveAsset(value as string);
                    field.SetValue(instance, asset);
                }
                else if (value is string)
                {
                    afterDeserialization += delegate
                    {
                        if (SerializableReference.ReferencesByID.TryGetValue(value as string, out UnityObject unityObject))
                        {
                            field.SetValue(instance, unityObject);
                        }
                        else if (field.GetValue(instance).GetType().IsAssignableFrom(typeof(string)))
                        {
                            field.SetValue(instance, value);
                        }
                    };
                }
                else field.SetValue(instance, value);
            }
        }
        #endregion
    }
}
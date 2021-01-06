using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using UnityEngine;
using SceneSerialization.Storage;
using UnityObject = UnityEngine.Object;

namespace SceneSerialization.Utility
{
    public enum InvocabilityStatus { DontInvoke, Invoke, InvokeAsExtension }

    public static class DataConverterUtility
    {
        private static readonly string[] _incompatibleAssemblies = new[] { "UnityEngine" };



        #region UnityObject Field Serializer
        public static bool SerializeFieldAsset(string fieldName, object asset, SerializableFieldData data, ref Action afterSerialization)
        {
            if (asset is UnityObject)
            {
                string key;
                if ((asset as GameObject) && PrefabStorage.Instance.IsPrefab(asset as GameObject, out key))
                {
                    data[fieldName] = key;
                    return true;
                }
                if ((asset as UnityObject) && AssetStorage.Instance.SupportsType(asset.GetType()))
                {
                    key = AssetStorage.Instance.StoreAsset(asset as UnityObject);
                    data[fieldName] = key;
                    return true;
                }
            }
            return false;
        }
        public static bool DeserializeFieldAsset(string fieldName, ref object asset, SerializableFieldData data, ref Action afterDeserialization)
        {
            if (data.ContainsKey(fieldName))
            {
                if (data[fieldName] is string && asset is GameObject)
                {
                    asset = PrefabStorage.Instance.RetrieveGameObject(data[fieldName] as string);
                    return true;
                }
                if (data[fieldName] is string && AssetStorage.Instance.SupportsType(asset as System.Type))
                {
                    asset = AssetStorage.Instance.RetrieveAsset(data[fieldName] as string);
                    return true;
                }
            }
            asset = null;
            return false;
        }
        #endregion



        public static MethodInfo[] GetExtensionMethods()
        {
            List<Type> assemblyTypes = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                assemblyTypes.AddRange(assembly.GetTypes());

            var query = from type in assemblyTypes
                        where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where method.GetParameters()[0].ParameterType == typeof(DataConverter)
                        select method;
            return query.ToArray();
        }
        public static MethodInfo[] GetBaseMethods()
        {
            MethodInfo[] methods = typeof(DataConverter).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            List<MethodInfo> validMethods = new List<MethodInfo>();

            for (int i = 0; i < methods.Length; i++)
                if (true)
                    validMethods.Add(methods[i]);
            return validMethods.ToArray();
        }



        public static void OtherMethodIsInvocable(object[] orignalMethodParameters, MethodInfo otherMethod, out InvocabilityStatus invocabilityStatus)
        {
            invocabilityStatus = InvocabilityStatus.DontInvoke;
            if (otherMethod == null)
                return;

            ParameterInfo[] otherParameters = otherMethod.GetParameters();
            for (int i = 0, j = 0; i < orignalMethodParameters.Length; i++, j++)
            {
                if (i == 0 && i < otherParameters.Length && otherParameters[i].ParameterType == typeof(DataConverter))
                {
                    invocabilityStatus = InvocabilityStatus.InvokeAsExtension;
                    j++;
                }
                if (j >= otherParameters.Length || otherParameters[j].ParameterType == orignalMethodParameters[i].GetType())
                {
                    invocabilityStatus = InvocabilityStatus.DontInvoke;
                    return;
                }
            }
            if (invocabilityStatus != InvocabilityStatus.InvokeAsExtension)
                invocabilityStatus = InvocabilityStatus.Invoke;
        }
        public static bool FieldIsValid(FieldInfo fieldInfo, SurrogateSelector surrogateSelector)
        {
            if (surrogateSelector != null && surrogateSelector.GetSurrogate(fieldInfo.FieldType, new StreamingContext(StreamingContextStates.All), out _) != null)
                return true;
            if (fieldInfo.FieldType.IsSerializable)
            {
                for (int j = 0; j < _incompatibleAssemblies.Length; j++)
                    if (fieldInfo.FieldType.FullName.Contains(_incompatibleAssemblies[j]))
                        return false;
                return true;
            }
            return false;
        }
    }
}
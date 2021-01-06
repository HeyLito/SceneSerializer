using System;
using System.Reflection;

namespace SceneSerialization.Storage 
{
    [Serializable]
    public struct TypeKeyWrapper
    {
        public string methodName;
        public Type type;

        public TypeKeyWrapper(string methodName, Type type)
        {
            this.methodName = methodName;
            this.type = type;
        }
    }
    [Serializable]
    public struct MethodInfoValueWrapper
    {
        public MethodInfo methodInfo;
        public bool isExtensionMethod;

        public MethodInfoValueWrapper(MethodInfo methodInfo, bool isExtensionMethod)
        {
            this.methodInfo = methodInfo;
            this.isExtensionMethod = isExtensionMethod;
        }
    }
    public class ConverterMethodInfoPair : SerializableDictionary<TypeKeyWrapper, MethodInfoValueWrapper> { }
    public class MethodInfoStorage : Storage<MethodInfoStorage>
    {
        public ConverterMethodInfoPair methods = new ConverterMethodInfoPair();
    }
}
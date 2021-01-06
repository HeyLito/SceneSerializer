using System;
using System.Runtime.Serialization;

namespace SceneSerialization 
{
    [Serializable]
    public class SerializableFieldData : SerializableDictionary<string, object>
    {
        public SerializableFieldData(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public SerializableFieldData() { }
    }
}
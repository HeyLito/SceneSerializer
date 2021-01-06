using System;
using UnityEngine;

namespace SceneSerialization.Utility 
{
    public static class DataConverter_3DPhysics
    {
        #region Rigidbody
        private static void SerializeIntoData(this DataConverter _, Rigidbody instance, SerializableFieldData data, ref Action afterSerialization)
        {
            data["velocity"] = instance.velocity;
        }
        private static void DeserializeIntoInstance(this DataConverter _, Rigidbody instance, SerializableFieldData data, ref Action afterDeserialization)
        {
            if (data.ContainsKey("velocity"))
                instance.velocity = (Vector3)data["velocity"];
        }
        #endregion
    }
}
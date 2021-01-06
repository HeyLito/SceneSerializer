using System;
using UnityEngine;

namespace SceneSerialization.Utility 
{
    public static class DataConverter_2DPhysics
    {
        #region Rigidbody2D
        private static void SerializeIntoData(this DataConverter _, Rigidbody2D instance, SerializableFieldData data, ref Action afterSerialization)
        {
            data["velocity"] = instance.velocity;
        }
        private static void DeserializeIntoInstance(this DataConverter _, Rigidbody2D instance, SerializableFieldData data, ref Action afterDeserialization)
        {
            if (data.ContainsKey("velocity"))
                instance.velocity = (Vector2)data["velocity"];
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SceneSerialization.Utility
{
    public static class DataConverter_BaseUnityObjects
    {
        #region GameObject Serializer
        private static void SerializeIntoData(this DataConverter _, GameObject instance, SerializableFieldData data, ref Action afterSerialization)
        {
            data["name"] = instance.name;
            data["tag"] = instance.tag;
            data["layer"] = instance.layer;
            data["isStatic"] = instance.isStatic;
            data["isActive"] = instance.activeSelf;
        }
        private static void DeserializeIntoInstance(this DataConverter _, GameObject instance, SerializableFieldData data, ref Action afterDeserialization)
        {
            if (data.ContainsKey("name"))
                instance.name = (string)data["name"];
            if (data.ContainsKey("tag"))
                instance.tag = (string)data["tag"];
            if (data.ContainsKey("layer"))
                instance.layer = (int)data["layer"];
            if (data.ContainsKey("isStatic"))
                instance.isStatic = (bool)data["isStatic"];
            if (data.ContainsKey("isActive"))
                instance.SetActive((bool)data["isActive"]);
        }
        #endregion



        #region Transform Serializer
        private static void SerializeIntoData(this DataConverter _, Transform instance, SerializableFieldData data, ref Action afterSerialization)
        {
            data["position"] = instance.position;
            data["rotation"] = instance.rotation;
            data["scale"] = instance.localScale;
        }
        private static void DeserializeIntoInstance(this DataConverter _, Transform instance, SerializableFieldData data, ref Action afterDeserialization)
        {
            if (data.ContainsKey("position"))
                instance.position = (Vector3)data["position"];
            if (data.ContainsKey("rotation"))
                instance.rotation = (Quaternion)data["rotation"];
            if (data.ContainsKey("scale"))
                instance.localScale = (Vector3)data["scale"];
        }
        #endregion



        #region MeshFilter Serializer
        private static void SerializeIntoData(this DataConverter _, MeshFilter instance, SerializableFieldData data, ref Action afterSerialization)
        {
            DataConverterUtility.SerializeFieldAsset("mesh", instance.sharedMesh, data, ref afterSerialization);
        }
        private static void DeserializeIntoInstance(this DataConverter _, MeshFilter instance, SerializableFieldData data, ref Action afterDeserialization)
        {
            object asset = typeof(Mesh);
            if (DataConverterUtility.DeserializeFieldAsset("mesh", ref asset, data, ref afterDeserialization))
                instance.mesh = asset as Mesh;
        }
        #endregion



        #region MeshRenderer Serializer
        private static void SerializeIntoData(this DataConverter _, MeshRenderer instance, SerializableFieldData data, ref Action afterSerialization)
        {
            for (int i = 0; i < instance.sharedMaterials.Length; i++)
                DataConverterUtility.SerializeFieldAsset($"material_{i}", instance.sharedMaterials[i], data, ref afterSerialization);
        }
        private static void DeserializeIntoInstance(this DataConverter _, MeshRenderer instance, SerializableFieldData data, ref Action afterDeserialization)
        {
            List<Material> materials = new List<Material>();
            int i = 0;
            foreach (var pair in data)
            {
                object asset = typeof(Material);
                if (DataConverterUtility.DeserializeFieldAsset($"material_{i}", ref asset, data, ref afterDeserialization))
                {
                    materials.Add(asset as Material);
                    i++;
                }
                else return;
            }
            instance.materials = materials.ToArray();
        }
        #endregion



        #region Animator Serializer
        private static void SerializeIntoData(this DataConverter _, Animator instance, SerializableFieldData data, ref Action afterSerialization)
        {
            DataConverterUtility.SerializeFieldAsset("runtimeController", instance.runtimeAnimatorController, data, ref afterSerialization);

            data["currentStateHash"] = instance.GetCurrentAnimatorStateInfo(0).shortNameHash;
            data["currentStateNormalizedTime"] = instance.GetCurrentAnimatorStateInfo(0).normalizedTime;
            data["nextStateHash"] = instance.GetNextAnimatorStateInfo(0).shortNameHash;
            data["transitionDuration"] = instance.GetAnimatorTransitionInfo(0).duration;
            data["transitionNormalized"] = instance.GetAnimatorTransitionInfo(0).normalizedTime;
        }
        private static void DeserializeIntoInstance(this DataConverter _, Animator instance, SerializableFieldData data, ref Action afterDeserialization)
        {
            object asset = typeof(RuntimeAnimatorController);
            if (DataConverterUtility.DeserializeFieldAsset("runtimeController", ref asset, data, ref afterDeserialization))
                instance.runtimeAnimatorController = asset as RuntimeAnimatorController;

            if (data.ContainsKey("currentStateHash") && data.ContainsKey("currentStateNormalizedTime"))
                instance.Play((int)data["currentStateHash"], 0, (float)data["currentStateNormalizedTime"]);
            instance.Update(0f);
            if (data.ContainsKey("nextStateHash") && data.ContainsKey("transitionDuration") && data.ContainsKey("transitionNormalized"))
                instance.CrossFadeInFixedTime((int)data["nextStateHash"], (float)data["transitionDuration"], 0, 0f, (float)data["transitionNormalized"]);
        }
        #endregion
    }
}
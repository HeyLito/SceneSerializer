using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace SceneSerialization.Storage 
{
    [Serializable]
    public class AssetPair : SerializableDictionary<string, UnityObject> { }
    public class AssetStorage : Storage<AssetStorage>
    {
        public AssetPair assets = new AssetPair();

        private readonly List<Type> _supportedAssets = new List<Type>()
        {
        typeof(Mesh),
        typeof(AudioClip),
        typeof(Material),
        typeof(PhysicMaterial),
        typeof(PhysicsMaterial2D),
        typeof(Flare),
        typeof(GUIStyle),
        typeof(Texture),
        typeof(RuntimeAnimatorController),
        typeof(AnimatorController),
        typeof(AnimationClip),
        typeof(ScriptableObject)
        };

        protected override bool CanSaveAsFile => true;

        public bool SupportsType(Type type)
        {
            return _supportedAssets.IndexOf(type) != -1;
        }

        public string StoreAsset(UnityObject assetObject)
        {
            string name = FormatObjectToKey(assetObject);
            if (string.IsNullOrEmpty(name)) return name;
            assets[name] = assetObject;
            return name;
        }
        public UnityObject RetrieveAsset(string key)
        {
            assets.TryGetValue(key, out UnityObject result);
            return result;
        }

        public static string FormatObjectToKey(UnityObject assetObject)
        {
            return assetObject ? $"{assetObject.GetType().Name}.{assetObject.name}" : "";
        }
        public static string FormatObjectToKey(UnityObject assetObject, string name)
        {
            return assetObject ? $"{assetObject.GetType().Name}.{name}" : "";
        }
    }
}
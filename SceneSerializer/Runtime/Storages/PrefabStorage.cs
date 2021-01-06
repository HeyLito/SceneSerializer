using System;
using UnityEngine;

namespace SceneSerialization.Storage 
{
    [Serializable]
    public class PrefabPair : SerializableDictionary<string, GameObject> { }

    public class PrefabStorage : Storage<PrefabStorage>
    {
        [SerializeField] private PrefabPair prefabs = new PrefabPair();
        protected override bool CanSaveAsFile => false;
        public PrefabPair Prefabs => prefabs;

        public bool IsPrefab(GameObject gameObject, out string key)
        {
            key = GetKey(gameObject);
            return key != null;
        }

        public string GetKey(GameObject gameObject)
        {
            foreach (var pair in prefabs)
                if (pair.Value == gameObject)
                    return pair.Key;
            return null;
        }

        public GameObject RetrieveGameObject(string key)
        {
            prefabs.TryGetValue(key, out GameObject result);
            return result;
        }

        public static string FormatGameObjectToKey(GameObject gameObject, int index)
        {
            return gameObject ? $"{gameObject.GetType().Name}.{gameObject.name}.{(index <= -1 ? 0 : index)}" : "";
        }
    }
}
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SceneSerialization.Storage.Editors 
{
    [CustomEditor(typeof(PrefabStorage))]
    public class PrefabStorageEditor : StorageEditor<PrefabStorage>
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Repopulate"))
                Repopulate(_target.Prefabs);
            if (GUILayout.Button("Filter Nulls"))
                FilterNulls(_target.Prefabs);
            if (_target.Prefabs.Count > 0)
                DisplayUnityObjects(_target.Prefabs.keys, _target.Prefabs.values);
            serializedObject.ApplyModifiedProperties();
        }

        private void Repopulate(PrefabPair prefabs)
        {
            prefabs.Clear();
            string[] prefabPaths = Directory.GetFiles("Assets", "*.prefab", SearchOption.AllDirectories);

            for (int i = 0; i < prefabPaths.Length; i++)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPaths[i]);
                if (prefab)
                    prefabs.Add(PrefabStorage.FormatGameObjectToKey(prefab, i), prefab);
            }
            EditorUtility.SetDirty(target);
        }
        private void FilterNulls(PrefabPair prefabs)
        {
            List<string> keys = prefabs.keys;
            List<GameObject> values = prefabs.values;

            for (int i = 0; i < keys.Count && i < values.Count; i++)
                if (values[i] == null)
                    prefabs.Remove(keys[i]);
            EditorUtility.SetDirty(target);
        }
    }
}
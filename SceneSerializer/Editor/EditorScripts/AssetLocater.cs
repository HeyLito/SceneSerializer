using UnityEditor;
using UnityEngine;
using System.IO;
using UnityObject = UnityEngine.Object;
using SceneSerialization.Storage;

namespace SceneSerialization.Editors 
{
    public class AssetLocater : AssetPostprocessor
    {
        enum FileType { Null, Asset, Prefab }
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            HandleImportedAssets(importedAssets);
            HandleMovedAssets(movedFromAssetPaths, movedAssets);
        }

        private static void HandleImportedAssets(string[] importedAssets)
        {
            for (int i = 0; i < importedAssets.Length; i++)
                if (PathIsUsableAsset(importedAssets[i], out _, out FileType fileType, out UnityObject asset) && fileType == FileType.Prefab)
                    if (!PrefabStorage.Instance.IsPrefab(asset as GameObject, out _))
                    {
                        PrefabStorage.Instance.Prefabs.Add(PrefabStorage.FormatGameObjectToKey(asset as GameObject, PrefabStorage.Instance.Prefabs.Count - 1), asset as GameObject);
                        EditorUtility.SetDirty(PrefabStorage.Instance);
                        //foreach (var identifier in UnityObject.FindObjectsOfType<ObjectStateIdentifier>())
                        //    ObjectStateIdentifierEditor.SetIndentifierValues(identifier, out _);
                    }
        }
        private static void HandleMovedAssets(string[] movedFromAssetPaths, string[] movedAssets)
        {
            for (int i = 0; i < movedAssets.Length; i++)
            {
                string directoryOfMovedFromAssetPaths = movedFromAssetPaths[i].Substring(0, movedFromAssetPaths[i].LastIndexOf(Path.AltDirectorySeparatorChar));
                string directoryOfMovedAsset = movedAssets[i].Substring(0, movedAssets[i].LastIndexOf(Path.AltDirectorySeparatorChar));
                if (directoryOfMovedFromAssetPaths != directoryOfMovedAsset)
                    continue;

                if (PathIsAsset(movedFromAssetPaths[i], out string oldAssetName) &&
                    PathIsUsableAsset(movedAssets[i], out string newAssetName, out FileType fileType, out UnityObject asset))
                {
                    string oldKey = null;
                    switch (fileType)
                    {
                        case FileType.Asset:
                            oldKey = AssetStorage.FormatObjectToKey(asset, oldAssetName);
                            if (AssetStorage.Instance.assets.ContainsKey(oldKey))
                            {
                                string newKey = AssetStorage.FormatObjectToKey(asset, newAssetName);
                                AssetStorage.Instance.assets.Remove(oldKey);
                                if (AssetStorage.Instance.assets.ContainsKey(newKey))
                                    AssetStorage.Instance.assets[newKey] = asset;
                                else AssetStorage.Instance.assets.Add(newKey, asset);
                                AssetStorage.Instance.AttemptToSave();
                                Debug.Log($"Changed from: <color=orange>{oldAssetName}</color> To: <color=lime>{newAssetName}</color>");
                            }
                            break;

                        case FileType.Prefab:
                            int index = 0;
                            foreach (var pair in PrefabStorage.Instance.Prefabs)
                                if (pair.Value == asset)
                                {
                                    oldKey = pair.Key;
                                    break;
                                }
                                else index++;
                            if (!string.IsNullOrEmpty(oldKey))
                            {
                                string newKey = PrefabStorage.FormatGameObjectToKey(asset as GameObject, index);
                                PrefabStorage.Instance.Prefabs.Remove(oldKey);
                                PrefabStorage.Instance.Prefabs.Add(newKey, asset as GameObject);
                                Debug.Log($"Changed from: <color=orange>{oldAssetName}</color> To: <color=lime>{newAssetName}</color>");
                            }
                            break;
                    }
                }
            }
        }

        private static bool PathIsAsset(string path, out string fileName)
        {
            string[] pathSplit = path.Split('/', '.');
            fileName = pathSplit.Length > 2 ? pathSplit[pathSplit.Length - 2] : "";

            if (string.IsNullOrEmpty(fileName))
                return false;

            return true;
        }
        private static bool PathIsUsableAsset(string path, out string fileName, out FileType fileType, out UnityObject asset)
        {
            fileType = FileType.Null;
            asset = null;

            string[] pathSplit = path.Split('/', '.');
            fileName = pathSplit.Length > 2 ? pathSplit[pathSplit.Length - 2] : "";

            if (string.IsNullOrEmpty(fileName))
                return false;

            asset = AssetDatabase.LoadAssetAtPath(path, typeof(UnityObject));
            if (asset)
            {
                if (asset is GameObject)
                    fileType = FileType.Prefab;
                else
                    fileType = FileType.Asset;
                return true;
            }
            else return false;
        }
    }
}
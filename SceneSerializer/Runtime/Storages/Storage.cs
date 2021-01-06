using UnityEngine;
using DataSerialization;

namespace SceneSerialization.Storage 
{
    public class Storage<T> : ScriptableObject where T : Storage<T>
    {
        protected virtual bool CanSaveAsFile { get; } = false;

        private static T _instance = null;
        public static T Instance => _instance == null ? _instance = GetStorage() : _instance;

        private static T GetStorage()
        {
            T storage = Resources.Load<T>(typeof(T).Name);
#if UNITY_EDITOR
            if (!storage)
            {
                string[] resourcePaths = System.IO.Directory.GetDirectories("Assets", "Resources", System.IO.SearchOption.AllDirectories);
                string resourcePath;
                if (resourcePaths.Length > 0)
                    resourcePath = resourcePaths[0];
                else resourcePath = System.IO.Directory.CreateDirectory($"Assets{System.IO.Path.AltDirectorySeparatorChar}Resources").Name;

                string path = $"{System.IO.Path.Combine(resourcePath, typeof(T).Name)}.asset";
                storage = CreateInstance<T>();
                UnityEditor.AssetDatabase.CreateAsset(storage, path);
                UnityEditor.AssetDatabase.ImportAsset(path, UnityEditor.ImportAssetOptions.ForceUpdate);
                UnityEditor.AssetDatabase.Refresh();
            }
#endif
            if (storage && storage.CanSaveAsFile)
            {
                if (!DataManager.LoadFileInDatabase(FileType.JSON, storage.GetType().Name, storage))
                    DataManager.CreateNewFileInDatabase(storage.GetType().Name, storage, FileType.JSON);

                Application.quitting += storage.AttemptToSave;
            }
            return storage;
        }

        public void AttemptToSave()
        {
            if (CanSaveAsFile)
                DataManager.SaveFileInDatabase(FileType.JSON, GetType().Name, this);
        }
    }

}
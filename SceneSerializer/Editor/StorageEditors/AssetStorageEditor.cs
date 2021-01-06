using UnityEditor;

namespace SceneSerialization.Storage.Editors 
{
    [CustomEditor(typeof(AssetStorage))]
    public class AssetStorageEditor : StorageEditor<AssetStorage>
    {
        public override void OnInspectorGUI()
        {
            if (_target.assets.Count > 0)
                DisplayUnityObjects(_target.assets.keys, _target.assets.values);
        }
    }
}
using UnityEditor;
using SceneSerialization.Storage;

[InitializeOnLoad]
class EditorStartup
{
    private const string editorStatus = "";
    static EditorStartup() 
    {
        if (!SessionState.GetBool(editorStatus, false))
        {
            SessionState.SetBool(editorStatus, true);
            _ = PrefabStorage.Instance;
            _ = AssetStorage.Instance;
        }
    }
}

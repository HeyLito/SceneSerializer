using System;
using UnityEngine;
using UnityEditor;
using SceneSerialization.Storage;

namespace SceneSerialization.Editors
{
    [CustomEditor(typeof(ObjectStateIdentifier))]
    public class ObjectStateIdentifierEditor : Editor
    {
        private GUIStyle _box;
        private GUIStyle Box
        {
            get
            {
                if (_box == null)
                {
                    _box = new GUIStyle(GUI.skin.textField);
                    _box.fontSize = 10;
                    _box.padding.bottom += 2;
                    _box.wordWrap = true;
                    _box.normal.textColor = new Color(1, 1f, 0.5f, 1);
                    _box.onNormal.textColor = _box.normal.textColor;
                    _box.focused.textColor = _box.normal.textColor;
                    _box.onFocused.textColor = _box.normal.textColor;
                    _box.hover.textColor = _box.normal.textColor;
                    _box.onHover.textColor = _box.normal.textColor;
                    _box.active.textColor = new Color(_box.normal.textColor.r - 0.2f, _box.normal.textColor.g - 0.2f, _box.normal.textColor.b - 0.2f, _box.normal.textColor.a);
                    _box.onActive.textColor = _box.active.textColor;
                    _box.alignment = TextAnchor.UpperLeft;
                }
                return _box;
            }
        }

        private SerializedProperty _identifierType;
        private SerializedProperty _persistentID;
        private SerializedProperty _destroyOnSceneClear;
        private bool _viewingPrefab = false;

        private void OnEnable()
        {
            _box = null;
            _identifierType = serializedObject.FindProperty("runtimeDataState.identifierType");
            _persistentID = serializedObject.FindProperty("runtimeDataState.persistentID");
            _destroyOnSceneClear = serializedObject.FindProperty("runtimeDataState.destroyOnSceneClear");

            if (Application.isPlaying)
                return;

            SetIndentifierValues(target as ObjectStateIdentifier, out _viewingPrefab);
        }

        public override void OnInspectorGUI()
        {
            if (!_viewingPrefab)
            {
                DisplayID();
                DisplayRuntimeTypeStatus();
            }
            EditorGUILayout.PropertyField(_destroyOnSceneClear);
            serializedObject.ApplyModifiedProperties();
        }

        private void DisplayID()
        {
            string[] splits = _persistentID.stringValue.Split('-');
            string output = "";
            for (int i = 0; i < splits.Length; i++)
            {
                output += splits[i];
                if (i + 1 < splits.Length)
                    output += " - ";
            }
            GUIStyle defaultLabelStyle = new GUIStyle(EditorStyles.label);
            defaultLabelStyle.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Unique ID", defaultLabelStyle, GUILayout.MinWidth(65f));
            if (GUILayout.Button(output, Box, GUILayout.MaxWidth(Screen.width)))
                GUIUtility.systemCopyBuffer = _persistentID.stringValue;
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
        }

        private void DisplayRuntimeTypeStatus()
        {
            string labelText = Enum.GetName(typeof(IdentifierType), _identifierType.enumValueIndex);
            Color labelColor = (IdentifierType)_identifierType.enumValueIndex == IdentifierType.Prefab ? new Color(0.35f, 0.85f, 1f, 1) : (IdentifierType)_identifierType.enumValueIndex == IdentifierType.Instance ? new Color(0.85f, 0.85f, 0.85f, 1f) : new Color();

            GUIStyle defaultLabelStyle = new GUIStyle(EditorStyles.label);
            defaultLabelStyle.richText = true;
            GUIStyle labelStyle = new GUIStyle(defaultLabelStyle);
            labelStyle.normal.textColor = labelColor;
            labelStyle.fontSize += 1;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Object Status", defaultLabelStyle, GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUILayout.LabelField(new GUIContent($"<b>{labelText}</b>", "The tooltip associated with this content. Read GUItooltip to get the tooltip of the gui element the user is currently over."), labelStyle);
            EditorGUILayout.EndHorizontal();
        }

        public static void SetIndentifierValues(ObjectStateIdentifier objectStateIdentifier, out bool inspectingPrefab) 
        {
            SerializedObject serializedObject = new SerializedObject(objectStateIdentifier);
            SerializedProperty identifierType = serializedObject.FindProperty("runtimeDataState.identifierType");
            SerializedProperty persistentID = serializedObject.FindProperty("runtimeDataState.persistentID");
            SerializedProperty prefabKey = serializedObject.FindProperty("runtimeDataState.prefabKey");
            SerializedProperty destroyOnSceneClear = serializedObject.FindProperty("runtimeDataState.destroyOnSceneClear");

            if (PrefabUtility.IsPartOfPrefabAsset(objectStateIdentifier.gameObject))
            {
                inspectingPrefab = true;
                identifierType.enumValueIndex = (int)IdentifierType.Null;
                persistentID.stringValue = "";
                prefabKey.stringValue = "";
                destroyOnSceneClear.boolValue = true;
            }
            else 
            {
                inspectingPrefab = false;
                if (persistentID.stringValue == "")
                    persistentID.stringValue = Guid.NewGuid().ToString();

                string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(objectStateIdentifier.gameObject);
                GameObject prefab = !string.IsNullOrEmpty(prefabPath) ? (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) : null;

                if ((IdentifierType)identifierType.enumValueIndex != IdentifierType.Prefab && 
                    prefab && 
                    prefab.GetComponent<ObjectStateIdentifier>() && 
                    PrefabStorage.Instance.IsPrefab(prefab, out string key))
                {
                    identifierType.enumValueIndex = (int)IdentifierType.Prefab;
                    prefabKey.stringValue = key;
                }
                else if ((IdentifierType)identifierType.enumValueIndex != IdentifierType.Instance)
                {
                    identifierType.enumValueIndex = (int)IdentifierType.Instance;
                    prefabKey.stringValue = "";
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityObject = UnityEngine.Object;

namespace SceneSerialization.Storage.Editors 
{
    public class StorageEditor<T> : Editor where T : Storage<T>
    {
        protected T _target;

        private GUIStyle _box = null;
        protected GUIStyle Box
        {
            get
            {
                if (_box == null)
                {
                    _box = new GUIStyle(GUI.skin.textArea);
                    _box.fontSize = 10;
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

        private GUIStyle _idStyle = null;
        protected GUIStyle IDStyle
        {
            get
            {
                if (_idStyle == null)
                {
                    _idStyle = new GUIStyle();
                    _idStyle.fontSize = 10;
                    _idStyle.normal.textColor = Color.white;
                    _idStyle.richText = true;
                    _idStyle.alignment = TextAnchor.MiddleLeft;
                    _idStyle.wordWrap = true;
                }
                return _idStyle;
            }
        }

        private GUIStyle _unityObjectStyle = null;
        protected GUIStyle UnityObjectStyle
        {
            get
            {
                if (_unityObjectStyle == null)
                {
                    _unityObjectStyle = new GUIStyle();
                    _unityObjectStyle.fontSize = 10;
                    _unityObjectStyle.normal.textColor = new Color(0.25f, 0.65f, 1f, 1);
                    _unityObjectStyle.richText = true;
                    _unityObjectStyle.alignment = TextAnchor.MiddleLeft;
                    _unityObjectStyle.wordWrap = true;
                }
                return _unityObjectStyle;
            }
        }

        private GUIStyle _errorStyle = null;
        protected GUIStyle ErrorStyle
        {
            get
            {
                if (_errorStyle == null)
                {
                    _errorStyle = new GUIStyle();
                    _errorStyle.fontSize = 10;
                    _errorStyle.normal.textColor = new Color(1, 0.65f, 0.25f, 1);
                    _errorStyle.richText = true;
                    _errorStyle.alignment = TextAnchor.MiddleLeft;
                }
                return _errorStyle;
            }
        }

        protected virtual void OnEnable()
        {
            _target = target as T;
            _box = _idStyle = _unityObjectStyle = _errorStyle = null;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        protected void DisplayUnityObjects<K, V>(List<K> keys, List<V> values) where V : UnityObject
        {
            Color color = GUI.backgroundColor;
            GUI.backgroundColor -= new Color(0.35f, 0.35f, 0.35f, 0);
            EditorGUILayout.BeginVertical(Box);
            for (int i = 0; i < keys.Count && i < values.Count; i++)
            {
                EditorGUILayout.Space(2.5f);
                GUI.backgroundColor = color;
                Rect rect = EditorGUILayout.BeginHorizontal("Button");
                if (GUI.Button(rect, GUIContent.none, EditorStyles.textField) && values[i] != null)
                    OnFieldClick(values[i]);

                EditorGUILayout.LabelField($"ID: <i>{keys[i]}</i>", IDStyle, GUILayout.Width(EditorGUIUtility.labelWidth + 30f));
                if (values[i] != null || values[i].GetType() != typeof(V))
                    EditorGUILayout.LabelField($"<b>{values[i].name}</b>", UnityObjectStyle);
                else EditorGUILayout.LabelField("NULL", ErrorStyle);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                if (i + 1 >= keys.Count)
                    EditorGUILayout.Space(2.5f);
            }
            EditorGUILayout.EndVertical();
        }

        protected virtual void OnFieldClick<V>(V value) where V : UnityObject
        {
            Selection.activeObject = value;
        }
    }

}
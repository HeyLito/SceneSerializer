using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using SceneSerialization.Storage;
using SceneSerialization.Utility;
using DataSerialization;

namespace SceneSerialization 
{
    [Serializable]
    public class RuntimeDataStatesByID : SerializableDictionary<string, RuntimeDataState>
    {
        public RuntimeDataStatesByID(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public RuntimeDataStatesByID() { }
    }

    public class SceneStateManager : MonoBehaviour
    {
        private static bool _quitting = false;
        private static SceneStateManager _instance = null;
        public static SceneStateManager Instance
        {
            get
            {
                if (!_instance)
                    if ((_instance = FindObjectOfType<SceneStateManager>()) == null && !_quitting)
                        _instance = new GameObject("StateManager", typeof(SceneStateManager)).GetComponent<SceneStateManager>();
                return _instance;
            }
        }

        public List<NonPersistentObject> nonPersistentObjects = new List<NonPersistentObject>();
        public RuntimeDataStatesByID runtimeDataStates = new RuntimeDataStatesByID();

        private event Action _afterSerialization;
        private event Action _afterDeserialization;

        private void Awake()
        {
            Application.quitting += () => _quitting = true;

            //DataManager.debugMode = true;
            DataManager.LoadAllFileDatabases();

            DataConverter.PopulateMethodData();
            DataConverter.SurrogateSelector = DataManager.SurrogateSelector;
        }

        public void QuickSaveLevelState()
        {
            SceneState savedSceneState = PackSceneData();
            if (!DataManager.SaveFile("SceneState_Test", savedSceneState))
                DataManager.CreateNewFile("SceneState_Test", savedSceneState, FileType.Binary);
        }
        public void QuickLoadLevelState()
        {
            SceneState loadedSceneState = new SceneState();
            if (DataManager.LoadFile("SceneState_Test", loadedSceneState))
                UnpackSceneData(loadedSceneState);
        }



        public SceneState PackSceneData()
        {
            SerializableReference.NullifyAllReferences();
            _afterSerialization = null;


            SceneState sceneState = new SceneState
            {
                storedGameObjectDataStates = new List<GameObjectDataState>(),
                storedRuntimeDataStates = new RuntimeDataStatesByID()
            };
            foreach (var pair in runtimeDataStates)
            {
                GameObjectDataState gameObjectDataState = PackGameObject(pair.Value);
                sceneState.storedGameObjectDataStates.Add(gameObjectDataState);
                sceneState.storedRuntimeDataStates.Add(pair.Key, pair.Value);
            }


            _afterSerialization?.Invoke();
            return sceneState;
        }
        public void UnpackSceneData(SceneState sceneState)
        {
            RemoveNonPersistentStates(sceneState.storedRuntimeDataStates, runtimeDataStates);


            foreach (var storedRuntimeDataState in sceneState.storedRuntimeDataStates)
                if (!runtimeDataStates.ContainsKey(storedRuntimeDataState.Key))
                    runtimeDataStates[storedRuntimeDataState.Key] = storedRuntimeDataState.Value;

            _afterDeserialization = null;


            for (int i = 0; i < sceneState.storedGameObjectDataStates.Count; i++)
                if (runtimeDataStates.TryGetValue(sceneState.storedGameObjectDataStates[i].persistantID, out RuntimeDataState runtimeDataState))
                    UnpackGameObject(sceneState.storedGameObjectDataStates[i], runtimeDataState);


            _afterDeserialization?.Invoke();
            SerializableReference.NullifyAllReferences();
        }
        private void RemoveNonPersistentStates(RuntimeDataStatesByID storedRuntimeDataStates, RuntimeDataStatesByID runtimeDataStates)
        {
            for (int i = nonPersistentObjects.Count - 1; i >= 0; i--) 
            {
                if (!nonPersistentObjects[i])
                    return;
                Destroy(nonPersistentObjects[i].gameObject);
                nonPersistentObjects.RemoveAt(i);
            }

            List<KeyValuePair<string, RuntimeDataState>> nonPersistentStates = new List<KeyValuePair<string, RuntimeDataState>>();
            foreach (var runtimeDataState in runtimeDataStates)
                if (!storedRuntimeDataStates.ContainsKey(runtimeDataState.Key) && runtimeDataState.Value.DestoryOnSceneClear)
                    nonPersistentStates.Add(runtimeDataState);
            for (int i = nonPersistentStates.Count - 1; i >= 0; i--)
            {
                if (nonPersistentStates[i].Value.objectStateIdentifier != null)
                    Destroy(nonPersistentStates[i].Value.objectStateIdentifier.gameObject);
                runtimeDataStates.Remove(nonPersistentStates[i].Key);
            }
        }



        #region GameObject Serialization
        private GameObjectDataState PackGameObject(RuntimeDataState runtimeDataState)
        {
            SerializableGameObject serializedGameObject = null;
            SerializeGameObjectTree(runtimeDataState.objectStateIdentifier.transform, ref serializedGameObject, null);
            return new GameObjectDataState(runtimeDataState.PersistentID, serializedGameObject);
        }
        private SerializableGameObject SerializeGameObject(GameObject gameObject)
        {
            SerializableGameObject serializedGameObject = new SerializableGameObject(gameObject, gameObject.name, SerializeComponents(gameObject.GetComponents(typeof(Component))));
            DataConverter.SerializeIntoData(gameObject, serializedGameObject.data, ref _afterSerialization);
            return serializedGameObject;
        }
        private List<SerializableComponent> SerializeComponents(Component[] components)
        {
            List<SerializableComponent> serializedComponents = new List<SerializableComponent>();
            foreach (var component in components)
            {
                SerializableComponent serializableComponent = new SerializableComponent(component, component.GetType().FullName, component.GetType().AssemblyQualifiedName);
                DataConverter.SerializeIntoData(component, serializableComponent.data, ref _afterSerialization);
                serializedComponents.Add(serializableComponent);
            }
            return serializedComponents;
        }
        private void SerializeGameObjectTree(Transform parent, ref SerializableGameObject startingPoint, List<SerializableGameObject> serializableChildren)
        {
            if (serializableChildren == null)
            {
                startingPoint = SerializeGameObject(parent.gameObject);
                serializableChildren = startingPoint.serializableChildren;
            }
            foreach (Transform child in parent)
            {
                SerializableGameObject serializableChildGameObject = SerializeGameObject(child.gameObject);
                serializableChildren.Add(serializableChildGameObject);
                SerializeGameObjectTree(child, ref startingPoint, serializableChildGameObject.serializableChildren);
            }
        }
        #endregion

        #region GameObject Deserialization
        private void UnpackGameObject(GameObjectDataState gameObjectDataState, RuntimeDataState runtimeDataState)
        {
            if (runtimeDataState.objectStateIdentifier == null)
            {
                if (runtimeDataState.IdentifierType == IdentifierType.Prefab)
                {
                    GameObject prefab = PrefabStorage.Instance.RetrieveGameObject(runtimeDataState.PrefabKey);
                    if (prefab)
                    {
                        bool status = prefab.activeSelf;
                        prefab.SetActive(false);
                        runtimeDataState.objectStateIdentifier = Instantiate(prefab).GetComponent<ObjectStateIdentifier>();
                        runtimeDataState.objectStateIdentifier.OverrideStateData(runtimeDataState);
                        prefab.SetActive(status);
                    }
                    else return;
                }
                else return;
            }

            DeserializeSerializableGameObjectTree(runtimeDataState.objectStateIdentifier.transform, gameObjectDataState.root, null);
        }
        private void DeserializeSerializableGameObject(SerializableGameObject serializableGameObject, GameObject gameObject)
        {
            serializableGameObject.Init(gameObject);
            DeserializeSerializableComponents(serializableGameObject.serializableComponents, gameObject);
            DataConverter.DeserializeIntoInstance(gameObject, serializableGameObject.data, ref _afterDeserialization);
        }
        private void DeserializeSerializableComponents(List<SerializableComponent> serializableComponents, GameObject targetOfComponents)
        {
            Component[] components = targetOfComponents.GetComponents(typeof(Component));
            for (int i = 0, j = 0; i < serializableComponents.Count; i++)
            {
                SerializableComponent serializableComponent = serializableComponents[i];
                Component component;
                if (i - j >= components.Length || serializableComponent.name != components[i - j].GetType().FullName)
                {
                    j++;
                    component = targetOfComponents.AddComponent(Type.GetType(serializableComponent.assemblyQualifiedName));
                }
                else component = components[i - j];

                serializableComponent.Init(component);
                DataConverter.DeserializeIntoInstance(component, serializableComponent.data, ref _afterDeserialization);
            }
        }
        private void DeserializeSerializableGameObjectTree(Transform parent, SerializableGameObject startingPoint, List<SerializableGameObject> serializableChildren)
        {
            if (startingPoint != null && serializableChildren == null)
            {
                DeserializeSerializableGameObject(startingPoint, parent.gameObject);
                serializableChildren = startingPoint.serializableChildren;
            }

            List<Transform> children = new List<Transform>();
            foreach (Transform child in parent)
                children.Add(child);

            for (int i = 0, j = 0; i < serializableChildren.Count; i++)
            {
                SerializableGameObject serializableChild = serializableChildren[i];
                Transform child;
                if (i - j >= children.Count || serializableChild.name != children[i - j].name)
                {
                    j++;
                    GameObject newChild = new GameObject(serializableChild.name);
                    newChild.transform.SetParent(parent);
                    newChild.transform.SetSiblingIndex(i);
                    child = newChild.transform;
                }
                else child = children[i - j];
                DeserializeSerializableGameObject(serializableChild, child.gameObject);
                DeserializeSerializableGameObjectTree(child, null, serializableChild.serializableChildren);
            }
        }
        #endregion
    }
}
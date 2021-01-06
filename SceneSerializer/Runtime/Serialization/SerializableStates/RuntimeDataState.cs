using System;
using UnityEngine;

namespace SceneSerialization 
{
    public enum IdentifierType { Null = default, Instance, Prefab }
    //public enum DataSerializingMode { AllPossibleData = default, ReferencesOnly, ComponentsOnly }

    [Serializable]
    public class RuntimeDataState
    {
        [NonSerialized] public ObjectStateIdentifier objectStateIdentifier = null;
        [SerializeField] private IdentifierType identifierType = default;
        [SerializeField] private string persistentID = "";
        [SerializeField] private string prefabKey = "";
        [SerializeField] private bool destroyOnSceneClear = true;

        public IdentifierType IdentifierType => identifierType;
        public string PersistentID => persistentID;
        public string PrefabKey => prefabKey;
        public bool DestoryOnSceneClear => destroyOnSceneClear;

        public RuntimeDataState() { }
        public RuntimeDataState(RuntimeDataState runtimeDataState)
        {
            objectStateIdentifier = runtimeDataState.objectStateIdentifier;
            identifierType = runtimeDataState.identifierType;
            persistentID = runtimeDataState.persistentID;
            destroyOnSceneClear = runtimeDataState.destroyOnSceneClear;
            prefabKey = runtimeDataState.prefabKey;
        }
    }
}
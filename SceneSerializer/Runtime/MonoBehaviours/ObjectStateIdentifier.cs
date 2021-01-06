using UnityEngine;

namespace SceneSerialization 
{
    [DisallowMultipleComponent]
    public class ObjectStateIdentifier : MonoBehaviour
    {
        [SerializeField] private RuntimeDataState runtimeDataState;
        public RuntimeDataState RuntimeDataState => runtimeDataState;

        private void OnEnable()
        {
            if (runtimeDataState.IdentifierType == IdentifierType.Null)
                return;
            runtimeDataState.objectStateIdentifier = this;
            if (SceneStateManager.Instance && !SceneStateManager.Instance.runtimeDataStates.ContainsKey(runtimeDataState.PersistentID))
                SceneStateManager.Instance?.runtimeDataStates.Add(runtimeDataState.PersistentID, runtimeDataState);
        }
        private void OnDisable()
        {
            SceneStateManager.Instance?.runtimeDataStates.Remove(runtimeDataState.PersistentID);
        }

        public void OverrideStateData(RuntimeDataState otherDataState)
        {
            runtimeDataState = new RuntimeDataState(otherDataState);
        }
    }
}
using System;
using System.Collections.Generic;

namespace SceneSerialization
{
    [Serializable]
    public class SceneState
    {
        public List<GameObjectDataState> storedGameObjectDataStates;
        public RuntimeDataStatesByID storedRuntimeDataStates;
    }
}
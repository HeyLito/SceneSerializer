using System;

namespace SceneSerialization 
{
    [Serializable]
    public class GameObjectDataState
    {
        public readonly string persistantID;
        public readonly SerializableGameObject root;

        public GameObjectDataState(string persistantID, SerializableGameObject root)
        {
            this.persistantID = persistantID;
            this.root = root;
        }
        public GameObjectDataState(GameObjectDataState gameObjectDataState)
        {
            persistantID = gameObjectDataState.persistantID;
            root = gameObjectDataState.root;
        }
    }
}
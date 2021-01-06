using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace SceneSerialization 
{
    [Serializable]
    public class SerializableGameObject
    {
        public readonly string name;
        public readonly SerializableFieldData data = new SerializableFieldData();
        public readonly List<SerializableComponent> serializableComponents = new List<SerializableComponent>();
        public readonly List<SerializableGameObject> serializableChildren = new List<SerializableGameObject>();
        public readonly SerializableReference reference;

        [NonSerialized] private bool _initialized = false;

        public SerializableGameObject(GameObject objectReference, string name)
        {
            this.name = name;
            reference = new SerializableReference(objectReference);

            _initialized = true;
        }
        public SerializableGameObject(GameObject objectReference, string name, List<SerializableComponent> serializableComponents)
        {
            this.name = name;
            this.serializableComponents = serializableComponents;
            reference = new SerializableReference(objectReference);

            _initialized = true;
        }

        public void Init(UnityObject objectReference)
        {
            if (_initialized)
                return;

            reference.Init(objectReference);
            _initialized = true;
        }
    }
}
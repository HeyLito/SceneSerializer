using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace SceneSerialization
{
    [Serializable]
    public class SerializableComponent
    {
        public readonly string name;
        public readonly string assemblyQualifiedName;
        public readonly SerializableReference reference;
        public readonly SerializableFieldData data = new SerializableFieldData();

        private bool _initialized = false;

        public SerializableComponent(Component objectReference, string name, string assemblyQualifiedName)
        {
            this.name = name;
            this.assemblyQualifiedName = assemblyQualifiedName;
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
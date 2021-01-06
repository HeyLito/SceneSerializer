using System;
using UnityObject = UnityEngine.Object;

namespace SceneSerialization 
{
    [Serializable]
    public class ReferencesByID : SerializableDictionary<string, UnityObject> { }
    [Serializable]
    public class ReferencesByUnityObject : SerializableDictionary<UnityObject, string> { }

    [Serializable]
    public class SerializableReference
    {
        private static ReferencesByID _referencesById = new ReferencesByID();
        private static ReferencesByUnityObject _referencesByUnityObject = new ReferencesByUnityObject();

        public static ReferencesByID ReferencesByID => _referencesById;
        public static ReferencesByUnityObject ReferencesByUnityObject => _referencesByUnityObject;

        public readonly string persistentID;

        public SerializableReference(UnityObject referenceObject)
        {
            persistentID = Guid.NewGuid().ToString();

            if (_referencesByUnityObject.TryGetValue(referenceObject, out string value))
            {
                persistentID = value;
                _referencesById[value] = referenceObject;
            }
            else
            {
                _referencesById[persistentID] = referenceObject;
                _referencesByUnityObject[referenceObject] = persistentID;
            }
        }

        public static void NullifyAllReferences()
        {
            _referencesById = new ReferencesByID();
            _referencesByUnityObject = new ReferencesByUnityObject();
        }

        public void Init(UnityObject referenceObject)
        {
            if (string.IsNullOrEmpty(persistentID))
                return;

            _referencesById[persistentID] = referenceObject;
            _referencesByUnityObject[referenceObject] = persistentID;
        }
    }

}
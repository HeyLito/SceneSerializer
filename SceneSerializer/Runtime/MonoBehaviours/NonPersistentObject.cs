using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonPersistentObject : MonoBehaviour
{
    private void OnEnable() 
    {
        SceneSerialization.SceneStateManager.Instance?.nonPersistentObjects.Add(this);
    }
    private void OnDisable() 
    {
        SceneSerialization.SceneStateManager.Instance?.nonPersistentObjects.Remove(this);
    }
}

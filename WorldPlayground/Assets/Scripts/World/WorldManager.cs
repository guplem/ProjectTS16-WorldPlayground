using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class WorldManager : MonoBehaviour
{

    public static WorldManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Multiple WorldManager exist. Destroying the last one registered.", gameObject);
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    
}

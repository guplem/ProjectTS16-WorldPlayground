using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Random = UnityEngine.Random;

public class WorldManager : MonoBehaviour
{

    [SerializeField] public WorldGenerator worldGenerator;
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
            short seed = Convert.ToInt16(Random.Range(0, ushort.MaxValue) - short.MaxValue);
            worldGenerator.Setup(seed);
        }
    }
    
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] private int minSurfaceHeight = 40;
    [SerializeField] private int maxSurfaceHeight = 100;
    [SerializeField] private int seed;
    [SerializeField] private float frequency = 75;
    [Space]
    [SerializeField] private Cube empty;
    [SerializeField] private Cube bottomEdge;
    [Space]
    [SerializeField] private Cube fillingCube;
    [SerializeField] private Cube surfaceCube;
    
    public static WorldGenerator Instance { get; private set; }
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

    private void Start()
    {
        if (minSurfaceHeight > Chunk.size.y)
            Debug.LogWarning("The 'minSurfaceHeight' (" + minSurfaceHeight + ") should be smaller or equal than the chunk vertical size (" + Chunk.size.y + ").", gameObject);
        
        if (maxSurfaceHeight > Chunk.size.y)
            Debug.LogWarning("The 'maxSurfaceHeight' (" + maxSurfaceHeight + ") should be smaller or equal than the chunk vertical size (" + Chunk.size.y + ").", gameObject);
        
        if (minSurfaceHeight > maxSurfaceHeight)
            Debug.LogWarning("The 'maxSurfaceHeight' (" + maxSurfaceHeight + ") should be bigger or equal than 'minSurfaceHeight' (" + minSurfaceHeight + ").", gameObject);
    }

    public Cube GetCube(Vector3 position)
    {
        position = new Vector3((int)position.x, (int)position.y, (int)position.z);
        
        // Outside the world
        if (position.y < 0 || position.y >= Chunk.size.y)
        {
            Debug.LogWarning("Asking for a cube outside the world: " + position);
            return empty;
        }
        // Bottom edge of the world
        if (position.y == 0)
            return bottomEdge;
        
        
        
        
        // World height generation
        int terrainHeight = Mathf.RoundToInt(minSurfaceHeight + Mathf.PerlinNoise(position.x/frequency+seed, position.z/frequency+seed) * (maxSurfaceHeight-minSurfaceHeight));
        if (position.y < terrainHeight)
            return fillingCube;
        if (position.y == terrainHeight)
            return surfaceCube;
        
        
        
        
        
        // Default cube
        return empty;
    }
}

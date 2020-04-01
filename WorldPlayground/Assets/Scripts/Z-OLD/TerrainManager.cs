using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [SerializeField] private int terrainSize = 100;
    [SerializeField] private GameObject cube;
    [SerializeField] private int minSurfaceHeight = 64;
    [SerializeField] private int maxSurfaceHeight = 120;
    [SerializeField] private int seed;
    [SerializeField] private float frequency = 100;

    void Start()
    {
        DestroyWorld();
        GenerateWorld();
    }

    public void DestroyWorld()
    {
        Debug.Log("Destroying world.");
        this.transform.DestroyImmediateAllChildren();
    }
    
    public void GenerateWorld()
    {
        Debug.Log("Generating world.");
        
        for (int x = -terrainSize / 2; x <= terrainSize / 2; x++)
        {
            for (int z = -terrainSize / 2; z <= terrainSize / 2; z++)
            {
                int y = Mathf.RoundToInt(minSurfaceHeight + Mathf.PerlinNoise(x/frequency+seed, z/frequency+seed) * (maxSurfaceHeight-minSurfaceHeight));    
                
                Instantiate(cube, new Vector3(x, y, z), Quaternion.identity, this.transform);
            }
        }
    }
}

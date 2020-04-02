using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubesManager : MonoBehaviour
{
    public static readonly int textureAtlasSizeInBlocks = 4;
    public static readonly float normalizedBlockTextureSize = 1f / textureAtlasSizeInBlocks;
    
    [SerializeField] private Cube[] cubes; //Max qty of cubes = 256. Id range = [0, 255]
    
    
    public static CubesManager Instance { get; private set; }
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
            SetupCubes();
        }
    }
    private void SetupCubes()
    {
        if (cubes.Length > 256)
            Debug.LogError("There are too many cubes. THe maximum qty of cubes is 256. The range of the cube's Id is [0, 255]");
        
        for (int c = 0; c < cubes.Length; c++)
            if (cubes[c] != null)
                cubes[c].id = c;
    }

    public Cube GetCube(byte cubeId)
    {
        return cubes[cubeId];
    }
}

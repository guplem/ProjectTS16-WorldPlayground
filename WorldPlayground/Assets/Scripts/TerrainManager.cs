using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [SerializeField] private int terrainSize = 100;
    [SerializeField] private GameObject cube;
    
    void Start()
    {
        for (int x = -terrainSize/2; x <= terrainSize/2; x++)
        {
            for (int z = -terrainSize/2; z <= terrainSize/2; z++)
            {
                Instantiate(cube, new Vector3(x, 0, z), Quaternion.identity);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField] private Cube bottomEdge;
    [SerializeField] private Cube middleCube;
    [SerializeField] private Cube topEdge;
    
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

    public Cube GetCube(Vector3Int position)
    {
        if (position.y <= 0) return bottomEdge;
        if (position.y >= 5) return topEdge;
        else return middleCube;
    }
}

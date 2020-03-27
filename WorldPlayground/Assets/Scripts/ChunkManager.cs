using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static Vector2Int size = new Vector2Int(16, 256); // XZ, Y

    public Vector2Int position { get; private set; }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        //Gizmos.DrawWireCube(transform.position+Vector3.up*size.y/2, new Vector3(size.x, size.y, size.x));
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0, size.x));
    }

    public void InitializeAt(Vector2Int position)
    {
        // Set the new position
        transform.position = new Vector3(position.x, 0, position.y);
        this.position = position;
        
        // Generate the data
        
    }
    
    
}

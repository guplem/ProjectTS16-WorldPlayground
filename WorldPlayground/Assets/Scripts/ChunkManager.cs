using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static Vector2Int size = new Vector2Int(16, 256); // XZ, Y

    public Vector2Int position
    {
        get { return _position; }
        set { 
            transform.position = new Vector3(value.x, 0, value.y);
            _position = value;
        }
    }

    private Vector2Int _position;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        //Gizmos.DrawWireCube(transform.position+Vector3.up*size.y/2, new Vector3(size.x, size.y, size.x));
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0, size.x));
    }
    
    
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    
    public static readonly Vector2Int size = new Vector2Int(10,100); // XZ, Y     //    16, 256 // 10, 128
    
    public Vector2Int position { get; private set; }
    public Vector2Int arrayPos;

    private byte[,,] chunkData;// { get; private set; } //Max qty of cubes = 256. Id range = [0, 255]

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position+new Vector3(size.x-1, size.y-1, size.x-1)/2, new Vector3(size.x, size.y, size.x));
        //Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0, size.x));
    }

    
    
// ==================== UTILITY METHODS ==================== //

    public Vector3 GetWorldPositionFromRelativePosition(Vector3Int vector3Int)
    {
        return transform.position + new Vector3(vector3Int.x, vector3Int.y, vector3Int.z);
    }

    public Cube GetCubeFromRelativePosition(Vector3Int relativePositionToTheChunk)
    {
        if (relativePositionToTheChunk.y >= size.y) return null;
        if (relativePositionToTheChunk.y < 0) return null;

        if (relativePositionToTheChunk.x >= size.x) {
            if (WorldManager.Instance.chunksArray.GetLength(0) > arrayPos.x+1) {
                if (WorldManager.Instance.chunksArray[arrayPos.x+1, arrayPos.y] != null){
                    relativePositionToTheChunk.x -= size.x;
                    return WorldManager.Instance.chunksArray[arrayPos.x+1, arrayPos.y].GetCubeFromRelativePosition(relativePositionToTheChunk);
                }
            }
            return null;
        }
        if (relativePositionToTheChunk.z >= size.x) {
            if (WorldManager.Instance.chunksArray.GetLength(1) > arrayPos.y+1) {
                if (WorldManager.Instance.chunksArray[arrayPos.x, arrayPos.y+1] != null){
                    relativePositionToTheChunk.z -= size.x;
                    return WorldManager.Instance.chunksArray[arrayPos.x, arrayPos.y+1].GetCubeFromRelativePosition(relativePositionToTheChunk);
                }
            }
            return null;
        }


        if (relativePositionToTheChunk.x < 0) {
            if (arrayPos.x - 1 >= 0) {
                if (WorldManager.Instance.chunksArray[arrayPos.x-1, arrayPos.y] != null){
                    relativePositionToTheChunk.x += size.x;
                    return WorldManager.Instance.chunksArray[arrayPos.x-1, arrayPos.y] .GetCubeFromRelativePosition(relativePositionToTheChunk);
                }
            }
            return null;
        }
        if (relativePositionToTheChunk.z < 0) {
            if (arrayPos.y - 1 >= 0) {
                if (WorldManager.Instance.chunksArray[arrayPos.x, arrayPos.y-1] != null){
                    relativePositionToTheChunk.z += size.x;
                    return WorldManager.Instance.chunksArray[arrayPos.x, arrayPos.y-1].GetCubeFromRelativePosition(relativePositionToTheChunk);
                }
            }
            return null;
        }

        return CubesManager.Instance.GetCube(chunkData[relativePositionToTheChunk.x, relativePositionToTheChunk.y, relativePositionToTheChunk.z]);
    }
    
    
    
// ==================== SETUP AND DEBUG ==================== //

    public void SetPosition(Vector2Int position)
    {
        transform.position = new Vector3(position.x, 0, position.y);
        this.position = position;
    }

    public void GenerateChunkData()
    {
        //Clear previous data
        chunkData = new byte[size.x,size.y,size.x];
        
        //Create new data
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                for (int z = 0; z < size.x; z++)
                    chunkData[x, y, z] = WorldGenerator.Instance.GetCube(GetWorldPositionFromRelativePosition(new Vector3Int(x, y, z))).byteId;
    }
    
    public void UpdateMesh()
    {
        meshFilter.mesh = ChunkMeshGenerator.GetMeshOf(this, chunkData);
    }

    
    
}

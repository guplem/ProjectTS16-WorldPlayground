using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    
    public static readonly Vector2Int size = new Vector2Int(10,100); // XZ, Y     //    16, 256 // 10, 128
    
    public Vector2Int position { get; private set; }
    public Vector2Int arrayPos;

    private byte[,,] chunkData = new byte[size.x,size.y,size.x];// { get; private set; } //Max qty of cubes = 256. Id range = [0, 255]
    private MeshData meshData = new MeshData();

    private Thread thread;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position+new Vector3(size.x-1, size.y-1, size.x-1)/2, new Vector3(size.x, size.y, size.x));
        //Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0, size.x));
    }

    
    
// ==================== UTILITY METHODS ==================== //

    public Vector3 GetWorldPositionFromRelativePosition(Vector3Int vector3Int)
    {
        return new Vector3(vector3Int.x + position.x, vector3Int.y, vector3Int.z + position.y);
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
    
    
    
// ==================== SETUP ==================== //

    public void SetPosition(Vector2Int position)
    {
        if (this.position == position)
            return;
        
        
        transform.position = new Vector3(position.x, 0, position.y);
        this.position = position;

        // Stop the threads
        thread?.Abort();

        // Reset the chunk
        lock (WorldManager.Instance.chunksToDrawSortedByDistance)
            WorldManager.Instance.chunksToDrawSortedByDistance.Remove(this);
        lock (WorldManager.Instance.notDrawnChunksWithDataSortedByDistance)
            WorldManager.Instance.notDrawnChunksWithDataSortedByDistance.Remove(this);
        
        DisableMeshRenderer();
        ClearChunkData();
    }

    
    
    public void GenerateChunkData()
    {
        if (thread != null && thread.IsAlive)
            Debug.LogWarning("The thread is alive and shouldn't to execute GenerateChunkData");
        
        thread = new Thread(new ThreadStart(ThreadedChunkDataGenerator));
        thread.Start();
    }

    private void ThreadedChunkDataGenerator()
    {
        for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                for (int z = 0; z < size.x; z++)
                    chunkData[x, y, z] = WorldGenerator.Instance.GetCube(GetWorldPositionFromRelativePosition(new Vector3Int(x, y, z))).byteId;
        
        lock (WorldManager.Instance.notDrawnChunksWithDataSortedByDistance)
        {
            WorldManager.Instance.notDrawnChunksWithDataSortedByDistance.Add(this);
            WorldManager.Instance.notDrawnChunksWithDataSortedByDistance.Sort((c1, c2) => (int) (Vector2Int.Distance(c1.position, WorldManager.Instance.centralChunkPosition) - Vector2Int.Distance(c2.position, WorldManager.Instance.centralChunkPosition)));
        }
    }

    private void ClearChunkData()
    {
        chunkData = new byte[size.x,size.y,size.x];
    }

    
    
    public void DisableMeshRenderer()
    {
        meshRenderer.enabled = false;
    }
    
    public void UpdateMeshToDrawChunk(bool enableMeshRenderer = true)
    {
        if (thread != null && thread.IsAlive)
            Debug.LogWarning("The thread is alive and shouldn't to execute UpdateMeshToDrawChunk");
        
        thread = new Thread(() => ChunkMeshGenerator.PopulateMeshDataToDrawChunk(this, meshData, chunkData));
        thread.Start();
    }

    public void DrawChunk()
    {
        meshFilter.mesh = ChunkMeshGenerator.GetMeshFrom(meshData);
        meshRenderer.enabled = true;
    }

    
    
}

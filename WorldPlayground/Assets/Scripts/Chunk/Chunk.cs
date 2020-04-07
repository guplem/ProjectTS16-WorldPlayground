using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    public static readonly Vector2Int size = new Vector2Int(10,100); // XZ, Y     //    16, 256 // 10, 128
    
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    internal CustomMesh mesh { get; private set; }

    public Vector2Int position { get; private set; }
    public Vector2Int arrayPos;

    internal byte[,,] chunkData = new byte[size.x,size.y,size.x];// { get; private set; } //Max qty of cubes = 256. Id range = [0, 255]

    //private bool isActive = false;
    private static readonly int mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
    private Thread thread;
    
    public ChunkEvolution.State currentState { get; set; }
    public ChunkEvolution.State targetState { get; set; }

    private void OnDrawGizmos()
    {
        Vector3 bottomCenter = transform.position + new Vector3(size.x - 1, size.y - 1, size.x - 1) / 2;
        
        switch (currentState) {
            case ChunkEvolution.State.Empty: Gizmos.color =  new Color(0.4f, 0.4f, 0.4f, 0.9f); break;
            case ChunkEvolution.State.Terrain: Gizmos.color = Color.red; break;
            case ChunkEvolution.State.Structures: Gizmos.color = new Color(255/255f, 140/255f, 0/255f); break;
            case ChunkEvolution.State.NeighboursStructures: Gizmos.color = Color.yellow; break;
            case ChunkEvolution.State.LoadedModifications: Gizmos.color = Color.green; break;
            case ChunkEvolution.State.MeshData: Gizmos.color = Color.cyan; break;
            case ChunkEvolution.State.MeshBuilt: Gizmos.color = Color.blue; break;
            case ChunkEvolution.State.Colliders: Gizmos.color = Color.magenta; break;
            case ChunkEvolution.State.Active: Gizmos.color = Color.white; break;
        }
        //Gizmos.DrawWireCube(center, new Vector3(size.x, size.y, size.x));
        Gizmos.DrawWireCube(bottomCenter+Vector3.up*size.y, new Vector3(size.x-0.5f, 0, size.x-0.5f));

        if (currentState != targetState)
        {
            if (currentState > targetState)
                Gizmos.color = Color.yellow;
            else if (currentState < targetState)
                Gizmos.color = Color.red;
            Gizmos.DrawSphere(bottomCenter+Vector3.up*size.y, 2);
        }
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
            if (WorldManager.Instance.chunkManager.chunksArray.GetLength(0) > arrayPos.x+1) {
                if (WorldManager.Instance.chunkManager.chunksArray[arrayPos.x+1, arrayPos.y] != null){
                    relativePositionToTheChunk.x -= size.x;
                    return WorldManager.Instance.chunkManager.chunksArray[arrayPos.x+1, arrayPos.y].GetCubeFromRelativePosition(relativePositionToTheChunk);
                }
            }
            return null;
        }
        if (relativePositionToTheChunk.z >= size.x) {
            if (WorldManager.Instance.chunkManager.chunksArray.GetLength(1) > arrayPos.y+1) {
                if (WorldManager.Instance.chunkManager.chunksArray[arrayPos.x, arrayPos.y+1] != null){
                    relativePositionToTheChunk.z -= size.x;
                    return WorldManager.Instance.chunkManager.chunksArray[arrayPos.x, arrayPos.y+1].GetCubeFromRelativePosition(relativePositionToTheChunk);
                }
            }
            return null;
        }
        
        if (relativePositionToTheChunk.x < 0) {
            if (arrayPos.x - 1 >= 0) {
                if (WorldManager.Instance.chunkManager.chunksArray[arrayPos.x-1, arrayPos.y] != null){
                    relativePositionToTheChunk.x += size.x;
                    return WorldManager.Instance.chunkManager.chunksArray[arrayPos.x-1, arrayPos.y] .GetCubeFromRelativePosition(relativePositionToTheChunk);
                }
            }
            return null;
        }
        if (relativePositionToTheChunk.z < 0) {
            if (arrayPos.y - 1 >= 0) {
                if (WorldManager.Instance.chunkManager.chunksArray[arrayPos.x, arrayPos.y-1] != null){
                    relativePositionToTheChunk.z += size.x;
                    return WorldManager.Instance.chunkManager.chunksArray[arrayPos.x, arrayPos.y-1].GetCubeFromRelativePosition(relativePositionToTheChunk);
                }
            }
            return null;
        }

        return CubesManager.Instance.GetCube(chunkData[relativePositionToTheChunk.x, relativePositionToTheChunk.y, relativePositionToTheChunk.z]);
    }

// ==================== WORKING METHODS ==================== //

    public void SetupAt(Vector2Int position)
    {
        if (this.position == position && mesh != null) return;
        if (mesh == null) mesh = new CustomMesh(meshFilter, meshRenderer);

        transform.position = new Vector3(position.x, 0, position.y);
        this.position = position;

        //isActive = false;
        thread?.Abort();

        currentState = 0;
        targetState = 0;
    }

    
    
    /*public void GenerateChunkData()
    {
        if (thread != null && thread.IsAlive)
            Debug.LogWarning("The thread is alive and shouldn't to execute GenerateChunkData");
        
        thread = new Thread(new ThreadStart(ThreadedChunkDataGenerator));
        thread.Start();
    }

    public void UpdateMeshToDrawChunk(bool enableMeshRenderer = true)
    {
        if (thread != null && thread.IsAlive)
            Debug.LogWarning("The thread is alive and shouldn't to execute UpdateMeshToDrawChunk");
        
        thread = new Thread(() => ChunkMeshGenerator.PopulateMeshDataToDrawChunk(this, mesh, chunkData));
        thread.Start();
    }

    public void DrawChunk()
    {
        meshFilter.mesh = ChunkMeshGenerator.GetMeshFrom(mesh);
        meshRenderer.enabled = true;
    }*/

    public void EvolveToState(ChunkEvolution.State desiredState)
    {
        if (targetState == desiredState) return;
        targetState = desiredState;
        ContinueEvolving();
    }

    public void ContinueEvolving()
    {
        if (targetState == currentState) return;
        
        // If it is not in the main thread (so the thread is alive and this is being executed from the chunk thread itself)
        /*if (System.Threading.Thread.CurrentThread.ManagedThreadId != mainThreadId)
            Evolve();
        else
            thread = new Thread(new ThreadStart(Evolve)); // thread = new Thread(() => Chunk.Evolve(this));*/
        Evolve();
    }

    private void Evolve()
    {
        if (targetState == currentState) return;

        if (targetState > currentState)
        {
            // If generating chunk
            switch (currentState) {
                case ChunkEvolution.State.Empty: new EmptyToTerrain().Evolve(this); break;
                case ChunkEvolution.State.Terrain: new TerrainToStructures().Evolve(this); break;
                case ChunkEvolution.State.Structures: new StructuresToNeighboursStructures().Evolve(this); break;
                case ChunkEvolution.State.NeighboursStructures: new NeighboursStructuresToLoadedModifications().Evolve(this); break;
                case ChunkEvolution.State.LoadedModifications: new LoadedModificationsToMeshData().Evolve(this); break;
                case ChunkEvolution.State.MeshData: new MeshDataToMeshBuilt().Evolve(this); break;
                case ChunkEvolution.State.MeshBuilt: new MeshBuiltToColliders().Evolve(this); break;
                case ChunkEvolution.State.Colliders: new CollidersToActive().Evolve(this); break;
                case ChunkEvolution.State.Active: break; // Shouldn't be reached
            }
        }
        else 
        {
            // If deconstructing the chunk
            switch (currentState) {
                case ChunkEvolution.State.Empty: break; // Shouldn't be reached
                case ChunkEvolution.State.Terrain: break; // No further evolution is needed
                case ChunkEvolution.State.Structures: break; // No further evolution is needed
                case ChunkEvolution.State.NeighboursStructures: break; // No further evolution is needed
                case ChunkEvolution.State.LoadedModifications: break; // No further evolution is needed
                case ChunkEvolution.State.MeshData: currentState = ChunkEvolution.State.LoadedModifications; ContinueEvolving(); break;
                case ChunkEvolution.State.MeshBuilt: currentState = ChunkEvolution.State.MeshData; ContinueEvolving(); break;
                case ChunkEvolution.State.Colliders: currentState = ChunkEvolution.State.MeshBuilt; ContinueEvolving(); break;
                case ChunkEvolution.State.Active: currentState = ChunkEvolution.State.Colliders; ContinueEvolving(); break;
            }
        }
        
    }
}
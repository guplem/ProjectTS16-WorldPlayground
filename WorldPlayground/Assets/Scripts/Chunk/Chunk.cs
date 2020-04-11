using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Chunk : MonoBehaviour, IComparable
{
    public static readonly Vector2Int size = new Vector2Int(10,100); // XZ, Y     //    16, 256 // 10, 128
    
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    internal CustomMesh mesh { get; private set; }

    public Vector2Int position { get; private set; }
    public Vector2Int arrayPos;

    internal byte[,,] chunkData = new byte[size.x,size.y,size.x]; // { get; private set; } //Max qty of cubes = 256. Id range = [0, 255]

    private StateManager _state = new StateManager();
    public StateManager.State currentState {
        get { lock (_state) { return _state.currentState;  } }
        set { lock (_state) { _state.currentState = value;  } }
    }
    public StateManager.State targetState {
        get { lock (_state) { return _state.targetState;  } }
        set { lock (_state) { _state.targetState = value;  } }
    }

    private void OnDrawGizmos()
    {
        Vector3 bottomCenter = transform.position + new Vector3(size.x - 1, size.y - 1, size.x - 1) / 2;
        
        switch (currentState) {
            case StateManager.State.Empty: Gizmos.color =  new Color(0.4f, 0.4f, 0.4f, 0.9f); break;
            case StateManager.State.Terrain: Gizmos.color = Color.red; break;
            case StateManager.State.Structures: Gizmos.color = new Color(255/255f, 140/255f, 0/255f); break;
            case StateManager.State.NeighboursStructures: Gizmos.color = Color.yellow; break;
            case StateManager.State.LoadedModifications: Gizmos.color = Color.green; break;
            case StateManager.State.MeshData: Gizmos.color = Color.cyan; break;
            case StateManager.State.MeshBuilt: Gizmos.color = Color.blue; break;
            case StateManager.State.Colliders: Gizmos.color = Color.magenta; break;
            case StateManager.State.Active: Gizmos.color = Color.white; break;
        }
        //Gizmos.DrawWireCube(center, new Vector3(size.x, size.y, size.x));
        //Gizmos.DrawWireCube(bottomCenter+Vector3.up*size.y, new Vector3(size.x-0.5f, 0, size.x-0.5f));

        if (currentState != targetState)
        {
            /*if (currentState > targetState)
                Gizmos.color = Color.yellow;
            else if (currentState < targetState)
                Gizmos.color = Color.red;*/
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
            if (ChunkManager.Instance.chunksArray.GetLength(0) > arrayPos.x+1) {
                if (ChunkManager.Instance.chunksArray[arrayPos.x+1, arrayPos.y] != null){
                    relativePositionToTheChunk.x -= size.x;
                    return ChunkManager.Instance.chunksArray[arrayPos.x+1, arrayPos.y].GetCubeFromRelativePosition(relativePositionToTheChunk);
                }
            }
            return null;
        }
        if (relativePositionToTheChunk.z >= size.x) {
            if (ChunkManager.Instance.chunksArray.GetLength(1) > arrayPos.y+1) {
                if (ChunkManager.Instance.chunksArray[arrayPos.x, arrayPos.y+1] != null){
                    relativePositionToTheChunk.z -= size.x;
                    return ChunkManager.Instance.chunksArray[arrayPos.x, arrayPos.y+1].GetCubeFromRelativePosition(relativePositionToTheChunk);
                }
            }
            return null;
        }
        
        if (relativePositionToTheChunk.x < 0) {
            if (arrayPos.x - 1 >= 0) {
                if (ChunkManager.Instance.chunksArray[arrayPos.x-1, arrayPos.y] != null){
                    relativePositionToTheChunk.x += size.x;
                    return ChunkManager.Instance.chunksArray[arrayPos.x-1, arrayPos.y] .GetCubeFromRelativePosition(relativePositionToTheChunk);
                }
            }
            return null;
        }
        if (relativePositionToTheChunk.z < 0) {
            if (arrayPos.y - 1 >= 0) {
                if (ChunkManager.Instance.chunksArray[arrayPos.x, arrayPos.y-1] != null){
                    relativePositionToTheChunk.z += size.x;
                    return ChunkManager.Instance.chunksArray[arrayPos.x, arrayPos.y-1].GetCubeFromRelativePosition(relativePositionToTheChunk);
                }
            }
            return null;
        }

        return CubesManager.Instance.GetCube(chunkData[relativePositionToTheChunk.x, relativePositionToTheChunk.y, relativePositionToTheChunk.z]);
    }

// ==================== WORKING METHODS ==================== //

    public void SetupAt(Vector2Int position)
    {
        if (this.position == position && HasBeenSetUpBefore()) return;
        
        if (mesh == null) mesh = new CustomMesh(meshFilter, meshRenderer);

        ChunkManager.Instance.chunkEvolver.RemoveChunkToEvolve(this);
        EvolveToState(StateManager.State.Empty, true);
        
        transform.position = new Vector3(position.x, 0, position.y);
        this.position = position;
    }

    private bool HasBeenSetUpBefore()
    {
        return mesh != null;
    }

    internal void EvolveToState(StateManager.State desiredState, bool evolveInCurrentThread = false)
    {
        if (targetState == desiredState && !evolveInCurrentThread) return;
        targetState = desiredState;

        if (!evolveInCurrentThread)
            ChunkManager.Instance.chunkEvolver.AddChunkToEvolve(this);
        else
            Evolve(evolveInCurrentThread);
    }

    internal bool Evolve(bool evolveInCurrentThread = false) // Returns true if the target state is reached. False otherwise
    {
        if (targetState == currentState) return true;

        if (targetState > currentState)
        {
            // If generating chunk
            switch (currentState) {
                case StateManager.State.Empty: new EmptyToTerrain(this).EvolveTo(evolveInCurrentThread); break;
                case StateManager.State.Terrain: new TerrainToStructures(this).EvolveTo(evolveInCurrentThread); break;
                case StateManager.State.Structures: new StructuresToNeighboursStructures(this).EvolveTo(evolveInCurrentThread); break;
                case StateManager.State.NeighboursStructures: new NeighboursStructuresToLoadedModifications(this).EvolveTo(evolveInCurrentThread); break;
                case StateManager.State.LoadedModifications: new LoadedModificationsToMeshData(this).EvolveTo(evolveInCurrentThread); break;
                case StateManager.State.MeshData: new MeshDataToMeshBuilt(this).EvolveTo(evolveInCurrentThread); break;
                case StateManager.State.MeshBuilt: new MeshBuiltToColliders(this).EvolveTo(evolveInCurrentThread); break;
                case StateManager.State.Colliders: new CollidersToActive(this).EvolveTo(evolveInCurrentThread); break;
                case StateManager.State.Active: break; // Shouldn't be reached
            }
        } else {
            // If deconstructing the chunk
            switch (currentState) {
                case StateManager.State.Empty: break; // Shouldn't be reached
                case StateManager.State.Terrain: currentState = targetState; break; // No further evolution is needed
                case StateManager.State.Structures: currentState = targetState; break; // No further evolution is needed
                case StateManager.State.NeighboursStructures: currentState = targetState; break; // No further evolution is needed
                case StateManager.State.LoadedModifications: currentState = targetState; break; // No further evolution is needed
                case StateManager.State.MeshData: currentState = targetState; break; // No further evolution is needed
                case StateManager.State.MeshBuilt: new MeshBuiltToMeshData(this).EvolveTo(evolveInCurrentThread); break;
                case StateManager.State.Colliders: new CollidersToMeshBuilt(this).EvolveTo(evolveInCurrentThread); break;
                case StateManager.State.Active: new ActiveToColliders(this).EvolveTo(evolveInCurrentThread); break;
            }
        }

        return currentState == targetState;
    }


    public int CompareTo(object obj)
    {
        Chunk chunkToCompareTo = (Chunk) obj;
        return (int) (Vector2Int.Distance(this.position, ChunkManager.Instance.centralChunkPosition) - Vector2Int.Distance(chunkToCompareTo.position, ChunkManager.Instance.centralChunkPosition));
    }
}
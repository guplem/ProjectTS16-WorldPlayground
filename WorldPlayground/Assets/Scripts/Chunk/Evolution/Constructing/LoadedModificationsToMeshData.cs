using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadedModificationsToMeshData : ChunkEvolution
{
    public LoadedModificationsToMeshData(Chunk chunk) { this.chunk = chunk; }
    protected override StateManager.State stateToEvolveTo => StateManager.State.MeshData;

    protected override bool CanEvolve()
    {
        return true;
    }

    protected override bool EvolutionWithMultithreading(bool forceEvolveArCurrentThread)
    {
        ChunkMeshGenerator.PopulateMeshDataToDrawChunk(chunk, chunk.mesh, chunk.chunkData);
        return true;
    }

    protected override bool EvolutionAtMainThread()
    {
        return false;
    }

}

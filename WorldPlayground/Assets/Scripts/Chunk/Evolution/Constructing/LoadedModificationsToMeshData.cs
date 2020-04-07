using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadedModificationsToMeshData : ChunkEvolution
{
    protected override State stateToEvolveTo => State.MeshData;
    
    protected override bool EvolutionWithMultithreading(Chunk chunk)
    {
        ChunkMeshGenerator.PopulateMeshDataToDrawChunk(chunk, chunk.mesh, chunk.chunkData);
        return true;
    }

    protected override bool EvolutionAtMainThread(Chunk chunk)
    {
        return false;
    }

}

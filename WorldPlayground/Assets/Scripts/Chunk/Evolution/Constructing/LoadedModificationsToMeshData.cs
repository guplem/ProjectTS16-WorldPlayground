using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadedModificationsToMeshData : ChunkEvolution
{
    protected override State stateToEvolveTo => State.MeshData;
    
    protected override void PerformEvolution(Chunk chunk)
    {
        ChunkMeshGenerator.PopulateMeshDataToDrawChunk(chunk, chunk.mesh, chunk.chunkData);
    }
}

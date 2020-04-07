using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class MeshDataToMeshBuilt : ChunkEvolution
{
    protected override State stateToEvolveTo => State.MeshBuilt;
    
    protected override bool EvolutionWithMultithreading(Chunk chunk)
    {
        WorldManager.Instance.chunkManager.AddAssistedEvolution(chunk, this);
        return false;
    }

    protected override bool EvolutionAtMainThread(Chunk chunk)
    {
        chunk.mesh.EnableMeshRenderer();
        chunk.mesh.UpdateMesh();
        
        return true;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class MeshDataToMeshBuilt : ChunkEvolution
{
    protected override StateManager.State stateToEvolveTo => StateManager.State.MeshBuilt;
    
    protected override bool EvolutionWithMultithreading(Chunk chunk)
    {
        ChunkManager.Instance.AddAssistedEvolution(chunk, this);
        return false;
    }

    protected override bool EvolutionAtMainThread(Chunk chunk)
    {
        chunk.mesh.EnableMeshRenderer();
        chunk.mesh.UpdateMesh();
        
        return true;
    }
}

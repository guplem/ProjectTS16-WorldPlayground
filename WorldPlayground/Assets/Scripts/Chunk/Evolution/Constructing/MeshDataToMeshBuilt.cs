using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class MeshDataToMeshBuilt : ChunkEvolution
{
    public MeshDataToMeshBuilt(Chunk chunk) { this.chunk = chunk; }
    protected override StateManager.State stateToEvolveTo => StateManager.State.MeshBuilt;
    
    protected override bool EvolutionWithMultithreading(bool forceEvolveArCurrentThread)
    {
        if (!forceEvolveArCurrentThread)
            ChunkManager.Instance.chunkEvolver.AddAssistedEvolution(this);
        
        return false;
    }

    protected override bool EvolutionAtMainThread()
    {
        chunk.mesh.EnableMeshRenderer();
        chunk.mesh.UpdateMesh();
        
        return true;
    }
}

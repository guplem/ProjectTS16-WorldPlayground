using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuiltToMeshData : ChunkEvolution
{
    public MeshBuiltToMeshData(Chunk chunk) { this.chunk = chunk; }
    protected override StateManager.State stateToEvolveTo => StateManager.State.MeshData;
    
    protected override bool EvolutionWithMultithreading(bool forceEvolveArCurrentThread)
    {
        if (!forceEvolveArCurrentThread)
            ChunkManager.Instance.chunkEvolver.AddAssistedEvolution(this);
        
        return false;
    }

    protected override bool EvolutionAtMainThread()
    {
        chunk.mesh.DisableMeshRenderer();

        return true;
    }
}

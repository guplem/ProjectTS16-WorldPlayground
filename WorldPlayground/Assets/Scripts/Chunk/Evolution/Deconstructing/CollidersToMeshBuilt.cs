using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollidersToMeshBuilt : ChunkEvolution
{
    public CollidersToMeshBuilt(Chunk chunk) { this.chunk = chunk; }
    protected override StateManager.State stateToEvolveTo => StateManager.State.MeshBuilt;
    
    protected override bool EvolutionWithMultithreading(bool forceEvolveArCurrentThread)
    {
        return true;
    }

    protected override bool EvolutionAtMainThread()
    {
        return false;
    }
}

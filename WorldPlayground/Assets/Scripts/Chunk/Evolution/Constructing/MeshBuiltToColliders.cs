using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuiltToColliders : ChunkEvolution
{
    protected override StateManager.State stateToEvolveTo => StateManager.State.Colliders;
    
    protected override bool EvolutionWithMultithreading(Chunk chunk)
    {
        return true;
    }

    protected override bool EvolutionAtMainThread(Chunk chunk)
    {
        return false;
    }
}

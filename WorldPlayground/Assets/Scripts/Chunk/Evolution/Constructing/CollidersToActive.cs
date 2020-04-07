using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollidersToActive : ChunkEvolution
{
    protected override StateManager.State stateToEvolveTo => StateManager.State.Active;
    
    protected override bool EvolutionWithMultithreading(Chunk chunk)
    {
        return true;
    }

    protected override bool EvolutionAtMainThread(Chunk chunk)
    {
        return false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainToStructures : ChunkEvolution
{
    public TerrainToStructures(Chunk chunk) { this.chunk = chunk; }
    protected override StateManager.State stateToEvolveTo => StateManager.State.Structures;
    
    protected override bool EvolutionWithMultithreading(bool forceEvolveArCurrentThread)
    {
        return true;
    }

    protected override bool EvolutionAtMainThread()
    {
        return false;
    }
}
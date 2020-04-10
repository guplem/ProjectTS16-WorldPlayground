using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeighboursStructuresToLoadedModifications : ChunkEvolution
{
    public NeighboursStructuresToLoadedModifications(Chunk chunk) { this.chunk = chunk; }
    protected override StateManager.State stateToEvolveTo => StateManager.State.LoadedModifications;
    
    protected override bool EvolutionWithMultithreading()
    {
        return true;
    }

    protected override bool EvolutionAtMainThread()
    {
        return false;
    }
}
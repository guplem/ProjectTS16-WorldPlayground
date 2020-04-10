using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructuresToNeighboursStructures : ChunkEvolution
{
    public StructuresToNeighboursStructures(Chunk chunk) { this.chunk = chunk; }
    protected override StateManager.State stateToEvolveTo => StateManager.State.NeighboursStructures;
    
    protected override bool EvolutionWithMultithreading()
    {
        return true;
    }

    protected override bool EvolutionAtMainThread()
    {
        return false;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructuresToNeighboursStructures : ChunkEvolution
{
    protected override State stateToEvolveTo => State.NeighboursStructures;
    
    protected override bool EvolutionWithMultithreading(Chunk chunk)
    {
        return true;
    }

    protected override bool EvolutionAtMainThread(Chunk chunk)
    {
        return false;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructuresToNeighboursStructures : ChunkEvolution
{
    protected override State stateToEvolveTo => State.NeighboursStructures;
    
    protected override void PerformEvolution(Chunk chunk)
    {
        
    }
}
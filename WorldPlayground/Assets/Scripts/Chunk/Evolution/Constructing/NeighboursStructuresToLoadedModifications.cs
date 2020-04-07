using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeighboursStructuresToLoadedModifications : ChunkEvolution
{
    protected override State stateToEvolveTo => State.LoadedModifications;
    
    protected override void PerformEvolution(Chunk chunk)
    {
        
    }
}
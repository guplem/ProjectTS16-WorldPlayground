using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainToStructures : ChunkEvolution
{
    protected override State stateToEvolveTo => State.Structures;
    
    protected override void PerformEvolution(Chunk chunk)
    {
        
    }
}
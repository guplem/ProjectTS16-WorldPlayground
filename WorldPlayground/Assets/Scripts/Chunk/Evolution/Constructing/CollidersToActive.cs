using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollidersToActive : ChunkEvolution
{
    protected override State stateToEvolveTo => State.Active;
    
    protected override void PerformEvolution(Chunk chunk)
    {
        
    }
}

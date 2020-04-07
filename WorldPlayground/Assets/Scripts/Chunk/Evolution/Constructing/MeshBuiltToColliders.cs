using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuiltToColliders : ChunkEvolution
{
    protected override State stateToEvolveTo => State.Colliders;
    
    protected override void PerformEvolution(Chunk chunk)
    {

    }
}

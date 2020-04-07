using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainToStructures : ChunkEvolution
{
    protected override State stateToEvolveTo => State.Structures;
    
    protected override bool EvolutionWithMultithreading(Chunk chunk)
    {
        return true;
    }

    protected override bool EvolutionAtMainThread(Chunk chunk)
    {
        return false;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyEvolution : ChunkEvolution
{
    public DummyEvolution(Chunk chunk)
    {
        this.chunk = chunk;
    }

    protected override StateManager.State stateToEvolveTo { get; }
    protected override bool CanEvolve()
    {
        return false;
    }

    protected override bool EvolutionWithMultithreading(bool forceEvolveArCurrentThread)
    {
        return true;
    }

    protected override bool EvolutionAtMainThread()
    {
        return true;
    }
}

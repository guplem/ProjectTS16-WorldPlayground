using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChunkEvolution : IEqualityComparer<ChunkEvolution>
{

    protected Chunk chunk;
    protected abstract StateManager.State stateToEvolveTo { get; }

    public void EvolveTo()
    {
        if (EvolutionWithMultithreading())
            CompleteEvolution();
    }
    
    public void EvolveAtMainThread()
    {
        if (EvolutionAtMainThread())
            CompleteEvolution();
        else
            Debug.LogWarning("Evolution at main thread not completed at " + chunk.gameObject.name, chunk.gameObject);
    }

    private void CompleteEvolution()
    {
        chunk.currentState = stateToEvolveTo;
        chunk.Evolve();
    }

    // Must return true if the evolution is completed. False if the evolutions needs from assisted evolution
    protected abstract bool EvolutionWithMultithreading();
    
    // Must return true if the evolution is completed. False if the evolution shouldn't be assisted
    protected abstract bool EvolutionAtMainThread();

    public bool Equals(ChunkEvolution x, ChunkEvolution y)
    {
        return x.chunk.Equals(y.chunk);
    }

    public int GetHashCode(ChunkEvolution obj)
    {
        return obj.chunk.GetHashCode();
    }
}

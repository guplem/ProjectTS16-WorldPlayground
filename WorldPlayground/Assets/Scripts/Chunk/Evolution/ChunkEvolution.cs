using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChunkEvolution : IEqualityComparer<ChunkEvolution>
{

    protected Chunk chunk;
    protected abstract StateManager.State stateToEvolveTo { get; }

    public void EvolveTo(bool evolveInCurrentThread = false)
    {
        if (EvolutionWithMultithreading(evolveInCurrentThread))
            CompleteEvolution(evolveInCurrentThread);
        else if (evolveInCurrentThread)
            EvolveAtMainThread(evolveInCurrentThread);
    }
    
    public void EvolveAtMainThread(bool evolveInCurrentThread = false)
    {
        if (EvolutionAtMainThread())
            CompleteEvolution(evolveInCurrentThread);
        else
            Debug.LogWarning("Evolution at main thread not completed at " + chunk.gameObject.name, chunk.gameObject);
    }

    private void CompleteEvolution(bool evolveInCurrentThread = false)
    {
        chunk.currentState = stateToEvolveTo;
        chunk.Evolve(evolveInCurrentThread);
    }

    // Must return true if the evolution is completed. False if the evolutions needs from assisted evolution
    protected abstract bool EvolutionWithMultithreading(bool forceEvolveArCurrentThread);
    
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChunkEvolution
{

    protected abstract StateManager.State stateToEvolveTo { get; }

    public void EvolveTo(Chunk chunk)
    {
        if (EvolutionWithMultithreading(chunk))
            CompleteEvolution(chunk);
    }
    
    public void EvolveAtMainThread(Chunk chunk)
    {
        if (EvolutionAtMainThread(chunk))
            CompleteEvolution(chunk);
        else
            Debug.LogWarning("Evolution at main thread not completed at " + chunk.gameObject.name, chunk.gameObject);
    }

    private void CompleteEvolution(Chunk chunk)
    {
        chunk.currentState = stateToEvolveTo;
        chunk.Evolve();
    }

    // Must return true if the evolution is completed. False if the evolutions needs from assisted evolution
    protected abstract bool EvolutionWithMultithreading(Chunk chunk);
    
    // Must return true if the evolution is completed. False if the evolution shouldn't be assisted
    protected abstract bool EvolutionAtMainThread(Chunk chunk);
    
}

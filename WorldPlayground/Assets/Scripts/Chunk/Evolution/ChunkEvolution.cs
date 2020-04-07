using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChunkEvolution
{
    public enum State
    {
        Empty,
        Terrain,
        Structures,
        NeighboursStructures,
        LoadedModifications,
        MeshData,
        MeshBuilt,
        Colliders,
        Active
    }

    protected abstract State stateToEvolveTo { get; }

    public void Evolve(Chunk chunk)
    {
        PerformEvolution(chunk);
        chunk.currentState = stateToEvolveTo;
        chunk.ContinueEvolving();
    }

    protected abstract void PerformEvolution(Chunk chunk);
}

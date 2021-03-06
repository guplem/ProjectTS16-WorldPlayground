﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveToColliders : ChunkEvolution
{
    public ActiveToColliders(Chunk chunk) { this.chunk = chunk; }
    protected override StateManager.State stateToEvolveTo => StateManager.State.Colliders;

    protected override bool CanEvolve()
    {
        return true;
    }

    protected override bool EvolutionWithMultithreading(bool forceEvolveArCurrentThread)
    {
        return true;
    }

    protected override bool EvolutionAtMainThread()
    {
        return false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager
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

    internal State currentState;
    internal State targetState;
}

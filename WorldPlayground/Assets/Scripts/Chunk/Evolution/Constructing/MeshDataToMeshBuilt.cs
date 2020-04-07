using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDataToMeshBuilt : ChunkEvolution
{
    protected override State stateToEvolveTo => State.MeshBuilt;
    
    protected override void PerformEvolution(Chunk chunk)
    {
        chunk.mesh.EnableMeshRenderer();
        chunk.mesh.SetMesh();
    }
}

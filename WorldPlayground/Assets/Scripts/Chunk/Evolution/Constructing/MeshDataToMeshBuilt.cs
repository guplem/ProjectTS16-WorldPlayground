using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public class MeshDataToMeshBuilt : ChunkEvolution
{
    public MeshDataToMeshBuilt(Chunk chunk) { this.chunk = chunk; }
    protected override StateManager.State stateToEvolveTo => StateManager.State.MeshBuilt;

    protected override bool CanEvolve()
    {
        return 
            ChunkManager.Instance.chunksArray[chunk.arrayPos.x+1, chunk.arrayPos.y].currentState >= StateManager.State.MeshData &&
            ChunkManager.Instance.chunksArray[chunk.arrayPos.x-1, chunk.arrayPos.y].currentState >= StateManager.State.MeshData &&
            ChunkManager.Instance.chunksArray[chunk.arrayPos.x, chunk.arrayPos.y+1].currentState >= StateManager.State.MeshData &&
            ChunkManager.Instance.chunksArray[chunk.arrayPos.x, chunk.arrayPos.y-1].currentState >= StateManager.State.MeshData;
    }

    protected override bool EvolutionWithMultithreading(bool forceEvolveArCurrentThread)
    {
        if (!forceEvolveArCurrentThread)
            ChunkManager.Instance.chunkEvolver.AddAssistedEvolution(this);
        
        return false;
    }

    protected override bool EvolutionAtMainThread()
    {
        chunk.mesh.EnableMeshRenderer();
        chunk.mesh.UpdateMesh();
        
        return true;
    }
}

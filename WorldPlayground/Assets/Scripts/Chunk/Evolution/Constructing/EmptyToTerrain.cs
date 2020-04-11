using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyToTerrain : ChunkEvolution
{
    public EmptyToTerrain(Chunk chunk) { this.chunk = chunk; }
    protected override StateManager.State stateToEvolveTo => StateManager.State.Terrain;

    protected override bool EvolutionWithMultithreading(bool forceEvolveArCurrentThread)
    {
        for (int y = 0; y < Chunk.size.y; y++)
        for (int x = 0; x < Chunk.size.x; x++)
        for (int z = 0; z < Chunk.size.x; z++)
            chunk.chunkData[x, y, z] = WorldGenerator.Instance.GetCube(chunk.GetWorldPositionFromRelativePosition(new Vector3Int(x, y, z))).byteId;
        
        return true;
    }

    protected override bool EvolutionAtMainThread()
    {
        return false;
    }
}

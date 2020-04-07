using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyToTerrain : ChunkEvolution
{
    protected override State stateToEvolveTo => State.Terrain;

    protected override void PerformEvolution(Chunk chunk)
    {
        for (int y = 0; y < Chunk.size.y; y++)
            for (int x = 0; x < Chunk.size.x; x++)
                for (int z = 0; z < Chunk.size.x; z++)
                    chunk.chunkData[x, y, z] = WorldGenerator.Instance.GetCube(chunk.GetWorldPositionFromRelativePosition(new Vector3Int(x, y, z))).byteId;
    }
}

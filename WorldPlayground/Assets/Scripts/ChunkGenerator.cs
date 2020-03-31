﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkGenerator
{
    public void GenerateChunks(HashSet<Chunk> currentChunks, GameObject chunkPrefab, Vector2Int centralChunkPosition, int radiusGenerationChunks)
    {
        // Get the positions that should have chunks at the moment
        HashSet<Vector2Int> positionsWhereAChunkMustExist = GetAllChunkPositionsInRadius(centralChunkPosition, radiusGenerationChunks);
        
        // Get chunks that shouldn't be generated now as they are (need to be moved)
        Chunk[] chunksToRegenerate = GetGeneratedChunksNotIn(positionsWhereAChunkMustExist, currentChunks).ToArray();
        // Remove (from the list) the chunks that shouldn't exist/be generated at the moment
        foreach (Chunk chunkToRegenerate in chunksToRegenerate)
            currentChunks.Remove(chunkToRegenerate);

        // Look where new chunks must generate
        HashSet<Vector2Int> positionsOfCurrentChunks = new HashSet<Vector2Int>();
        foreach (Chunk chunk in currentChunks)
            positionsOfCurrentChunks.Add(chunk.position);
        HashSet<Vector2Int> positionsWhereAChunkMustGenerate = new HashSet<Vector2Int>(positionsWhereAChunkMustExist.Except(positionsOfCurrentChunks));

        // Generate the chunks in the new positions that should have one now
        int chunkToGenerateIndex = 0;
        foreach (Vector2Int chunkToGeneratePosition in positionsWhereAChunkMustGenerate)
        {
            Chunk newChunk;

            if (chunkToGenerateIndex < chunksToRegenerate.Length)
            {
                newChunk = chunksToRegenerate[chunkToGenerateIndex];
            }
            else
            {
                newChunk = MonoBehaviour.Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, WorldManager.Instance.transform).GetComponent<Chunk>();
                if (positionsOfCurrentChunks.Count > 0) // Means that it is not the first run (where is normal that new chunks are instantiated)
                    Debug.LogWarning("Instantiating " + newChunk.gameObject.name + " because the current quantity of instantiated chunks is not enough.");
            }
            
            newChunk.gameObject.name = "Chunk " + chunkToGeneratePosition.x + "_" + chunkToGeneratePosition.y;
            newChunk.InitializeAt(chunkToGeneratePosition);
            currentChunks.Add(newChunk);
            
            chunkToGenerateIndex++;
        }
        
        //Destroy non-necessary chunks
        for (int c = chunkToGenerateIndex; c < chunksToRegenerate.Length; c++)
        {
            Debug.LogWarning("Destroying " + chunksToRegenerate[c].gameObject.name + " because it was instantiated before and it is no longer necessary.");
            MonoBehaviour.Destroy(chunksToRegenerate[c].gameObject);
        }
        
        Debug.Log("Generation cycle completed. New chunks: " + chunkToGenerateIndex + ". Total number of chunks: " + currentChunks.Count);
    }
    
    
    
    private HashSet<Vector2Int> GetAllChunkPositionsInRadius(Vector2Int center, int radiusInChunks)
    {
        HashSet<Vector2Int> chunkCoords = new HashSet<Vector2Int>();
        int radiusSqr = radiusInChunks * radiusInChunks;
        
        for (int x = -radiusInChunks; x <= radiusInChunks; x++)
        for (int z = -radiusInChunks; z <= radiusInChunks; z++)
            if (x*x+z*z<=radiusSqr)
                chunkCoords.Add(new Vector2Int(x*Chunk.size.x + center.x, z*Chunk.size.x + center.y));

        return chunkCoords;
    }
    
    
    
    private HashSet<Chunk> GetGeneratedChunksNotIn(HashSet<Vector2Int> positions, HashSet<Chunk> currentChunks)
    {
        HashSet<Chunk> chunksToDestroy = new HashSet<Chunk>();
        
        foreach (Chunk chunk in currentChunks)
            if (!positions.Contains(chunk.position))
                chunksToDestroy.Add(chunk);

        return chunksToDestroy;
    }
}

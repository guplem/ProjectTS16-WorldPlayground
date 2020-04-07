using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkManager
{
    public HashSet<Chunk> chunks = new HashSet<Chunk>();
    public Chunk[,] chunksArray { get; private set; }
    private GameObject chunkPrefab;

    public ChunkManager(GameObject chunkPrefab)
    {
        this.chunkPrefab = chunkPrefab;
    }

    public void RestructureChunks()
    {
        if (UpdateChunksPositions())
        {
            UpdateChunksArray();
            UpdateObjectiveStatesOfChunks();
        }
    }

    // Returns true if new chunks have been generated
    private bool UpdateChunksPositions()
    {
        // Get the positions that should have chunks at the moment
        HashSet<Vector2Int> positionsWhereAChunkMustExist = GetAllChunkPositionsInRadius(WorldManager.Instance.centralChunkPosition, WorldManager.Instance.radiusOfGeneratedChunks);
        
        // Get current chunks that should  to be moved from position
        Chunk[] chunksToRegenerate = GetChunksWithPositionNotIn(positionsWhereAChunkMustExist, chunks).ToArray();

        // Get the positions with missing chunks - Look where new chunks must generate
            //// (To do so, first Remove (from the list) the chunks that shouldn't exist/be generated at the moment)
        foreach (Chunk chunkToRegenerate in chunksToRegenerate)
            chunks.Remove(chunkToRegenerate);
        HashSet<Vector2Int> positionsOfCurrentChunks = new HashSet<Vector2Int>();
        foreach (Chunk chunk in chunks)
            positionsOfCurrentChunks.Add(chunk.position);
        HashSet<Vector2Int> positionsWithMissingChunks = new HashSet<Vector2Int>(positionsWhereAChunkMustExist.Except(positionsOfCurrentChunks));

        // Generate the chunks in the new positions that should have one now
        int chunkToGenerateIndex = 0;
        foreach (Vector2Int chunkToGeneratePosition in positionsWithMissingChunks)
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
            newChunk.SetupAt(chunkToGeneratePosition);
            
            chunks.Add(newChunk);
            
            chunkToGenerateIndex++;
        }
        
        //Destroy non-necessary chunks
        for (int c = chunkToGenerateIndex; c < chunksToRegenerate.Length; c++)
        {
            Debug.LogWarning("Destroying " + chunksToRegenerate[c].gameObject.name + " because it was instantiated before and it is no longer necessary.");
            MonoBehaviour.Destroy(chunksToRegenerate[c].gameObject);
        }
        
        Debug.Log("Generation cycle completed. New chunks: " + chunkToGenerateIndex + ". Total number of chunks: " + chunks.Count);

        return chunkToGenerateIndex > 0;
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
    
    private HashSet<Chunk> GetChunksWithPositionNotIn(HashSet<Vector2Int> positions, HashSet<Chunk> chunks)
    {
        HashSet<Chunk> chunksWithPositionNotInCollection = new HashSet<Chunk>();
        
        foreach (Chunk chunk in chunks)
            if (!positions.Contains(chunk.position))
                chunksWithPositionNotInCollection.Add(chunk);

        return chunksWithPositionNotInCollection;
    }
    
    private void UpdateChunksArray()
    {
        chunksArray = new Chunk[WorldManager.Instance.radiusOfGeneratedChunks*2+1, WorldManager.Instance.radiusOfGeneratedChunks*2+1];
        foreach (var chunk in chunks)
        {
            int x = ((WorldManager.Instance.radiusOfGeneratedChunks*Chunk.size.x)-(WorldManager.Instance.centralChunkPosition.x-chunk.position.x))/Chunk.size.x;
            int y = ((WorldManager.Instance.radiusOfGeneratedChunks*Chunk.size.x)-(WorldManager.Instance.centralChunkPosition.y-chunk.position.y))/Chunk.size.x;
            chunk.arrayPos = new Vector2Int(x, y);
            chunksArray[x, y] = chunk;
        }
    }
    
    private void UpdateObjectiveStatesOfChunks()
    {
        int totalQuantityOfStates = Enum.GetNames(typeof(ChunkEvolution.State)).Length;
        
        foreach (Chunk chunk in chunks)
        {
            float distance = Vector2Int.Distance(chunk.position, WorldManager.Instance.centralChunkPosition)/Chunk.size.x;
            int desiredState =
                Mathf.Clamp(
                    ((int) (totalQuantityOfStates - totalQuantityOfStates /
                            (float) WorldManager.Instance.radiusOfGeneratedChunks * distance)), 0,
                    totalQuantityOfStates - 1);
            chunk.EvolveToState((ChunkEvolution.State) desiredState);
        }
    }
    
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class ChunkConfigurator
{
    private ChunkManager chunkManager;
    public ChunkConfigurator(ChunkManager chunkManager)
    {
        this.chunkManager = chunkManager;
    }
    
    public void RestructureChunks()
    {
        if (UpdateChunksPositions())
        {
            UpdateChunksArray();
            chunkManager.EvolveAllChunksToTheProperState();
        }
    }
    
    private bool UpdateChunksPositions() // Returns true if new chunks have been generated/chunks have been moved
    {
        ////////Stopwatch stopwatch = Stopwatch.StartNew();
        
        // Get the positions that should have chunks at the moment
        HashSet<Vector2Int> positionsWhereAChunkMustExist = GetAllChunkPositionsInRadius(chunkManager.centralChunkPosition, chunkManager.radiusOfGeneratedChunks);
        ////////stopwatch.Stop(); Debug.Log("positionsWhereAChunkMustExist " + stopwatch.ElapsedMilliseconds); stopwatch.Restart();
        
        // Get current chunks that should to be moved from position
        Chunk[] chunksToRegenerate = GetChunksWithPositionNotIn(positionsWhereAChunkMustExist, chunkManager.chunks).ToArray();
        ////////stopwatch.Stop(); Debug.Log("chunksToRegenerate " + stopwatch.ElapsedMilliseconds); stopwatch.Restart();

        // Get the positions with missing chunks - Look where new chunks must generate
        HashSet<Vector2Int> positionsOfCurrentChunks = new HashSet<Vector2Int>();
        foreach (Chunk chunk in chunkManager.chunks)
            positionsOfCurrentChunks.Add(chunk.position);
        HashSet<Vector2Int> positionsWithMissingChunks = new HashSet<Vector2Int>(positionsWhereAChunkMustExist.Except(positionsOfCurrentChunks));
        ////////stopwatch.Stop(); Debug.Log("positionsWithMissingChunks " + stopwatch.ElapsedMilliseconds); stopwatch.Restart();

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
                newChunk = MonoBehaviour.Instantiate(chunkManager.chunkPrefab, Vector3.zero, Quaternion.identity, chunkManager.transform).GetComponent<Chunk>();
                chunkManager.chunks.Add(newChunk);
                if (positionsOfCurrentChunks.Count > 0) // Means that it is not the first run (where is normal that new chunks are instantiated)
                    Debug.LogWarning("Instantiating " + newChunk.gameObject.name + " because the current quantity of instantiated chunks is not enough.");
            }
            
            newChunk.gameObject.name = "Chunk " + chunkToGeneratePosition.x + "_" + chunkToGeneratePosition.y;
            newChunk.SetupAt(chunkToGeneratePosition);
            
            chunkToGenerateIndex++;
        }
        ////////stopwatch.Stop(); Debug.Log("Generation " + stopwatch.ElapsedMilliseconds); stopwatch.Restart();
        
        //Destroy non-necessary chunks
        for (int c = chunkToGenerateIndex; c < chunksToRegenerate.Length; c++)
        {
            Debug.LogWarning("Destroying " + chunksToRegenerate[c].gameObject.name + " because it was instantiated before and it is no longer necessary.");
            MonoBehaviour.Destroy(chunksToRegenerate[c].gameObject);
        }
        
        Debug.Log("Generation cycle completed. New chunks: " + chunkToGenerateIndex + ". Total number of chunks: " + chunkManager.chunks.Count);

        ////////stopwatch.Stop(); Debug.Log("Destruction " + stopwatch.ElapsedMilliseconds); stopwatch.Restart();
        
        if (chunkToGenerateIndex > 0)
            chunkManager.chunks.Sort((c1, c2) => c1.CompareTo(c2));
        ////////stopwatch.Stop(); Debug.Log("Sorting " + stopwatch.ElapsedMilliseconds); stopwatch.Restart();
        
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
    
    private HashSet<Chunk> GetChunksWithPositionNotIn(HashSet<Vector2Int> positions, List<Chunk> chunks)
    {
        HashSet<Chunk> chunksWithPositionNotInCollection = new HashSet<Chunk>();
        
        foreach (Chunk chunk in chunks)
            if (!positions.Contains(chunk.position))
                chunksWithPositionNotInCollection.Add(chunk);

        return chunksWithPositionNotInCollection;
    }
    
    private void UpdateChunksArray()
    {
        chunkManager.chunksArray = new Chunk[chunkManager.radiusOfGeneratedChunks*2+1, chunkManager.radiusOfGeneratedChunks*2+1];
        foreach (Chunk chunk in chunkManager.chunks)
        {
            int x = ((chunkManager.radiusOfGeneratedChunks*Chunk.size.x)-(chunkManager.centralChunkPosition.x-chunk.position.x))/Chunk.size.x;
            int y = ((chunkManager.radiusOfGeneratedChunks*Chunk.size.x)-(chunkManager.centralChunkPosition.y-chunk.position.y))/Chunk.size.x;
            chunk.arrayPos = new Vector2Int(x, y);
            chunkManager.chunksArray[x, y] = chunk;
        }
    }

    internal void UpdateObjectiveStatesOfChunks()
    {
        try
        {
            int totalQuantityOfStates = Enum.GetNames(typeof(StateManager.State)).Length;
        
            foreach (Chunk chunk in chunkManager.chunks)
            {
                float distance = Vector2Int.Distance(chunk.position, chunkManager.centralChunkPosition)/Chunk.size.x;
                int desiredState =
                    Mathf.Clamp(
                        ((int) (totalQuantityOfStates - totalQuantityOfStates /
                                (float) chunkManager.radiusOfGeneratedChunks * distance)), 0,
                        totalQuantityOfStates - 1);
                chunk.EvolveToState((StateManager.State) desiredState);
            }
        } catch (InvalidOperationException) { chunkManager.revalidateChunks = true; }

    }
}

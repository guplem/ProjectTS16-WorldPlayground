using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class ChunkConfigurator
{
    private HashSet<Vector2Int> positionsWithMissingChunks = new HashSet<Vector2Int>();
    private Chunk[] chunksToRegenerate = new Chunk[0];
    
    private Thread chunkConfiguratorThread;
    internal bool waitingForUnityWorkToEnd { get; private set; }
    internal bool waitingForUnityWorkToStart { get; private set; }

    #region Restructuration process

    internal void RestructureChunks()
    {
        waitingForUnityWorkToStart = false;
        
        if (waitingForUnityWorkToEnd)
            Debug.LogWarning("'waitingForUnityWorkToEnd' should be always 'false' if calling 'RestructureChunks' to properly synchronize threads");
        else
        {
            chunkConfiguratorThread?.Abort();
            ThreadStart starter = SaveChunksRestructurations;
            starter += () =>
            {
                waitingForUnityWorkToEnd = false; 
                waitingForUnityWorkToStart = true;
            };
            chunkConfiguratorThread = new Thread(starter);
            chunkConfiguratorThread.Start();
        }

    }
    
    private void SaveChunksRestructurations() // Returns true if new chunks have been generated/chunks have been moved
    {
        // Get the positions that should have chunks at the moment
        HashSet<Vector2Int> positionsWhereAChunkMustExist = GetAllChunkPositionsInRadius(ChunkManager.Instance.centralChunkPosition, ChunkManager.Instance.radiusOfGeneratedChunks);

        // Get current chunks that should to be moved from position or shouldn't exist (at least where they are)
        lock (chunksToRegenerate)
            lock (ChunkManager.Instance.chunks)
                chunksToRegenerate = GetChunksWithPositionNotIn(positionsWhereAChunkMustExist, ChunkManager.Instance.chunks).ToArray();

        // Get the positions with missing chunks - Look where new chunks must generate
        HashSet<Vector2Int> positionsOfCurrentChunks = new HashSet<Vector2Int>();
        lock (ChunkManager.Instance.chunks)
            foreach (Chunk chunk in ChunkManager.Instance.chunks)
                positionsOfCurrentChunks.Add(chunk.position);
        positionsWithMissingChunks = new HashSet<Vector2Int>(positionsWhereAChunkMustExist.Except(positionsOfCurrentChunks));
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

    #endregion

    #region Unity work

    internal void UpdateWorldChunks()
    {
        if (!waitingForUnityWorkToStart)
            Debug.LogWarning("UpdateWorldChunks shouldn't be called if 'waitingForUnityWorkToStart' is false.");
        if (waitingForUnityWorkToEnd)
            Debug.LogWarning("UpdateWorldChunks shouldn't be called if there is already another instance of it running (waitingForUnityWorkToEnd is true).");
        
        waitingForUnityWorkToStart = false;
        waitingForUnityWorkToEnd = true; 
        
        int chunkToGenerateIndex = 0;
        lock (chunksToRegenerate)
        {
            lock (positionsWithMissingChunks)
            {
                // Generate the chunks in the new positions that should have one now
                foreach (Vector2Int chunkToGeneratePosition in positionsWithMissingChunks)
                {
                    Chunk newChunk;

                    if (chunkToGenerateIndex < chunksToRegenerate.Length)
                    {
                        newChunk = chunksToRegenerate[chunkToGenerateIndex];
                    }
                    else
                    {
                        newChunk = MonoBehaviour.Instantiate(ChunkManager.Instance.chunkPrefab, Vector3.zero, Quaternion.identity, ChunkManager.Instance.transform).GetComponent<Chunk>();
                        
                        lock(ChunkManager.Instance.chunks)
                            ChunkManager.Instance.chunks.Add(newChunk);
                    }

                    newChunk.gameObject.name = "Chunk " + chunkToGeneratePosition.x + "_" + chunkToGeneratePosition.y;
            
                    newChunk.SetupAt(chunkToGeneratePosition);

                    chunkToGenerateIndex++;
                }
                positionsWithMissingChunks.Clear();
            }

            //Destroy non-necessary chunks
            for (int c = chunkToGenerateIndex; c < chunksToRegenerate.Length; c++)
            {
                lock(ChunkManager.Instance.chunks)
                    ChunkManager.Instance.chunks.Remove(chunksToRegenerate[c]);
                MonoBehaviour.Destroy(chunksToRegenerate[c].gameObject);
            }
            
            chunksToRegenerate = new Chunk[0];
        }


        waitingForUnityWorkToEnd = false;
        
        if (chunkToGenerateIndex > 0)
            UpdateChunksConfiguration();
    }

    #endregion

    #region Chunks configuration

    private void UpdateChunksConfiguration()
    {
        chunkConfiguratorThread?.Abort();
        ThreadStart starter = UpdateChunksArray;
        starter += () => { UpdateObjectiveStatesOfChunks(); };
        chunkConfiguratorThread = new Thread(starter);
        chunkConfiguratorThread.Start();
    }

    private void UpdateChunksArray()
    {
        lock (ChunkManager.Instance.chunksArray)
        {
            ChunkManager.Instance.chunksArray = new Chunk[ChunkManager.Instance.radiusOfGeneratedChunks*2+1, ChunkManager.Instance.radiusOfGeneratedChunks*2+1];
            lock (ChunkManager.Instance.chunks)
            {
                foreach (Chunk chunk in ChunkManager.Instance.chunks)
                {
                    int x = ((ChunkManager.Instance.radiusOfGeneratedChunks*Chunk.size.x)-(ChunkManager.Instance.centralChunkPosition.x-chunk.position.x))/Chunk.size.x;
                    int y = ((ChunkManager.Instance.radiusOfGeneratedChunks*Chunk.size.x)-(ChunkManager.Instance.centralChunkPosition.y-chunk.position.y))/Chunk.size.x;
                    chunk.arrayPos = new Vector2Int(x, y);
                    ChunkManager.Instance.chunksArray[x, y] = chunk;
                }
            }

        }
    }

    private void UpdateObjectiveStatesOfChunks()
    {
        try
        {
            int totalQuantityOfStates = Enum.GetNames(typeof(StateManager.State)).Length;

            lock (ChunkManager.Instance.chunks)
            {
                ChunkManager.Instance.chunks.Sort((c1, c2) => c1.CompareTo(c2));
                
                foreach (Chunk chunk in ChunkManager.Instance.chunks)
                {
                    float distance = Vector2Int.Distance(chunk.position, ChunkManager.Instance.centralChunkPosition)/Chunk.size.x;
                    int desiredState =
                        Mathf.Clamp(
                            ((int) (totalQuantityOfStates - totalQuantityOfStates /
                                    (float) ChunkManager.Instance.radiusOfGeneratedChunks * distance)), 0,
                            totalQuantityOfStates - 1);
                    chunk.EvolveToState((StateManager.State) desiredState);
                }
            }
            ChunkManager.Instance.chunkEvolver.SortChunksToEvolve();

        } catch (InvalidOperationException) { ChunkManager.Instance.revalidateChunks = true; }

    }

    #endregion
    
}
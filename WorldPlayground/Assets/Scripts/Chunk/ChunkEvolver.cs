using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class ChunkEvolver
{
    private Dictionary<Chunk, ChunkEvolution> assistedEvolutions = new Dictionary<Chunk, ChunkEvolution>();
    private HashSet<Chunk> threadedEvolutions = new HashSet<Chunk>();
    //private SortedList<Chunk, ChunkEvolution> assistedEvolutions = new SortedList<Chunk, ChunkEvolution>();
    
    private ChunkManager chunkManager;
    public ChunkEvolver(ChunkManager chunkManager)
    {
        this.chunkManager = chunkManager;
    }

    #region Assisted Evolution

    public void AddAssistedEvolution(Chunk chunk, ChunkEvolution evolution)
    {
        lock (assistedEvolutions)
        {
            if (!assistedEvolutions.ContainsKey(chunk))
                assistedEvolutions.Add(chunk, evolution);
        }
    }
    
    public void AssistChunkEvolution()
    {
        lock (assistedEvolutions)
        {
            if (assistedEvolutions.Count <= 0) return;
            
            List<Chunk> chunksToAssist = new List<Chunk>(assistedEvolutions.Keys);
            foreach (Chunk chunkToAssist in chunksToAssist)
            {
                ChunkEvolution evolution = assistedEvolutions[chunkToAssist];
                assistedEvolutions.Remove(chunkToAssist);
                evolution.EvolveAtMainThread(chunkToAssist);
            }
        }
    }
    
    public bool IsAssistingChunk(Chunk chunk)
    {
        lock (assistedEvolutions)
        {
            return assistedEvolutions.ContainsKey(chunk);
        }
    }

    #endregion

    
    #region Threaded Evolutions

    public void AddChunkToEvolve(Chunk chunk)
    {
        lock (threadedEvolutions)
        {
            threadedEvolutions.Add(chunk);
        }

        CreateThreadToEvolve();
    }

    private void CreateThreadToEvolve()
    {
        if (SoftwareManager.Instance.ShouldNewThreadsBeEnqueued())
            ThreadPool.QueueUserWorkItem(EvolveChunks);
    }

    public void EvolveChunks(object stateInfo) 
    {
        int threadedEvolutionsCount;
        lock (threadedEvolutions)
            threadedEvolutionsCount = threadedEvolutions.Count;
        
        while (threadedEvolutionsCount > 0)
        {
            Chunk chunkToEvolve;
            lock (threadedEvolutions)
            {
                chunkToEvolve = threadedEvolutions.RemoveFirst();
                threadedEvolutionsCount = threadedEvolutions.Count;
            }

            if (!chunkToEvolve.Evolve())
            {
                lock (threadedEvolutions)
                {
                    threadedEvolutions.Add(chunkToEvolve);
                    threadedEvolutionsCount = threadedEvolutions.Count;
                }
            }
            
            Thread.Sleep(5);
        }
    }
    
    #endregion
    
}

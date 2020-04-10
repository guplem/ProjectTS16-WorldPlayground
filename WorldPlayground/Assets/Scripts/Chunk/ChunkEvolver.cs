using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class ChunkEvolver
{
    private HashSet<ChunkEvolution> assistedEvolutions = new HashSet<ChunkEvolution>();
    private HashSet<Chunk> threadedEvolutions = new HashSet<Chunk>();

    #region Assisted Evolution

    public void AddAssistedEvolution(ChunkEvolution evolution)
    {
        lock (assistedEvolutions)
        {
            assistedEvolutions.Add(evolution);
        }
    }
    
    public void AssistChunkEvolution()
    {
        lock (assistedEvolutions)
        {
            if (assistedEvolutions.Count <= 0) return;
            
            HashSet<ChunkEvolution> evolutionsToAssist = new HashSet<ChunkEvolution>(assistedEvolutions);
            foreach (ChunkEvolution evolution in evolutionsToAssist)
            {
                assistedEvolutions.Remove(evolution);
                evolution.EvolveAtMainThread();
            }
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

    private void EvolveChunks(object stateInfo) 
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ChunkManager : MonoBehaviour
{
    [Tooltip("The transform that will be the center of the 'game action'")]
    [SerializeField] private Transform loadingCenter;
    public Vector2Int centralChunkPosition { get; private set; }
    [SerializeField] private GameObject chunkPrefab;

    [Space]
    [Tooltip("Time between each check of the chunks that should be generated at that moment.")]
    [SerializeField] private float timeBetweenChunkUpdates = 1f;
    [Space]
    [Tooltip("Max distance of the chunks generated at that moment.")]
    [SerializeField] public int radiusOfGeneratedChunks = 32;

    public List<Chunk> chunks = new List<Chunk>();
    public Chunk[,] chunksArray { get; private set; }
    public bool revalidateChunks = false;
    
    private Dictionary<Chunk, ChunkEvolution> assistedEvolutions = new Dictionary<Chunk, ChunkEvolution>();
    //private SortedList<Chunk, ChunkEvolution> assistedEvolutions = new SortedList<Chunk, ChunkEvolution>();

    #region Setup

    public static ChunkManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Multiple ChunkManager exist. Destroying the last one registered.", gameObject);
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    
    private void Start()
    {
        StartCoroutine(nameof(UpdateChunks));
    }

    #endregion

    #region Utility

    public Vector2Int PositionToChunkPosition(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / Chunk.size.x) * Chunk.size.x,
            Mathf.FloorToInt(position.z / Chunk.size.x) * Chunk.size.x);
    }

    public Chunk GetChunkOfPosition(Vector3 position)
    {
        Vector2Int chunkPos = PositionToChunkPosition(position);
        int x = ((radiusOfGeneratedChunks*Chunk.size.x)-(centralChunkPosition.x-chunkPos.x))/Chunk.size.x;
        int y = ((radiusOfGeneratedChunks*Chunk.size.x)-(centralChunkPosition.y-chunkPos.y))/Chunk.size.x;
        return chunksArray[x, y];
    }

    #endregion

    #region Iterative methods

    private void Update()
    {
        AssistChunkEvolution();
        
        if (revalidateChunks)
        {
            RestructureChunks(true);
            revalidateChunks = false;
        }
    }
    
    private bool UpdateCentralChunkPosition() // Returns true if the position is updated
    {
        Vector2Int newCentralPos = PositionToChunkPosition(loadingCenter.position);
        bool retValue = newCentralPos != centralChunkPosition;
        centralChunkPosition = newCentralPos;
        return retValue;
    }
    
    private IEnumerator UpdateChunks() {
        while (true)
        {
            if (UpdateCentralChunkPosition() || chunks.Count == 0)
                RestructureChunks();

            yield return new WaitForSeconds(timeBetweenChunkUpdates);
        }
    }



    #endregion
    
    #region Chunk Restructuration

    private Thread restructuringThread;
    public void RestructureChunks(bool force = false)
    {
        if (UpdateChunksPositions() || force)
        {
            restructuringThread?.Abort();
            
            UpdateChunksArray();
            
            restructuringThread = new Thread(new ThreadStart(UpdateObjectiveStatesOfChunks));
            restructuringThread.Start();
        }
    }
    
    private bool UpdateChunksPositions() // Returns true if new chunks have been generated/chunks have been moved
    {
        // Get the positions that should have chunks at the moment
        HashSet<Vector2Int> positionsWhereAChunkMustExist = GetAllChunkPositionsInRadius(centralChunkPosition, radiusOfGeneratedChunks);
        
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
                newChunk = MonoBehaviour.Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<Chunk>();
                
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

        if (chunkToGenerateIndex > 0)
            chunks.Sort((c1, c2) => c1.CompareTo(c2));
        
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
        chunksArray = new Chunk[radiusOfGeneratedChunks*2+1, radiusOfGeneratedChunks*2+1];
        foreach (Chunk chunk in chunks)
        {
            int x = ((radiusOfGeneratedChunks*Chunk.size.x)-(centralChunkPosition.x-chunk.position.x))/Chunk.size.x;
            int y = ((radiusOfGeneratedChunks*Chunk.size.x)-(centralChunkPosition.y-chunk.position.y))/Chunk.size.x;
            chunk.arrayPos = new Vector2Int(x, y);
            chunksArray[x, y] = chunk;
        }
    }
    
    private void UpdateObjectiveStatesOfChunks()
    {
        try
        {
            int totalQuantityOfStates = Enum.GetNames(typeof(StateManager.State)).Length;
        
            foreach (Chunk chunk in chunks)
            {
                float distance = Vector2Int.Distance(chunk.position, centralChunkPosition)/Chunk.size.x;
                int desiredState =
                    Mathf.Clamp(
                        ((int) (totalQuantityOfStates - totalQuantityOfStates /
                                (float) radiusOfGeneratedChunks * distance)), 0,
                        totalQuantityOfStates - 1);
                chunk.EvolveToState((StateManager.State) desiredState);
            }
        } catch (InvalidOperationException) { revalidateChunks = true; }

    }
    
    #endregion

    #region Assisted Evolution

    public void AddAssistedEvolution(Chunk chunk, ChunkEvolution evolution)
    {
        lock (assistedEvolutions)
        {
            assistedEvolutions.Add(chunk, evolution);
        }
        
    }
    
    public void AssistChunkEvolution()
    {
        lock (assistedEvolutions)
        {
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



}

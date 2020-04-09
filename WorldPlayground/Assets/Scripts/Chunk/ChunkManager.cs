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
    [SerializeField] internal GameObject chunkPrefab;

    [Space]
    [Tooltip("Time between each check of the chunks that should be generated at that moment.")]
    [SerializeField] private float timeBetweenChunkUpdates = 1f;
    [Space]
    [Tooltip("Max distance of the chunks generated at that moment.")]
    [SerializeField] public int radiusOfGeneratedChunks = 32;

    public List<Chunk> chunks = new List<Chunk>();
    public Chunk[,] chunksArray { get; internal set; }
    public bool revalidateChunks = false;

    private ChunkConfigurator chunkConfigurator;
    internal ChunkEvolver chunkEvolver;

    public Thread chunkThread;
    
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
            chunkConfigurator = new ChunkConfigurator(this);
            chunkEvolver = new ChunkEvolver(this);
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
        chunkEvolver.AssistChunkEvolution();
    }

    private IEnumerator UpdateChunks() {
        while (true)
        {
            if (UpdateCentralChunkPosition() || chunks.Count == 0 || revalidateChunks)
            {
                revalidateChunks = false;
                chunkConfigurator.RestructureChunks();
            }

            yield return new WaitForSeconds(timeBetweenChunkUpdates);
        }
    }
    
    private bool UpdateCentralChunkPosition() // Returns true if the position is updated
    {
        Vector2Int newCentralPos = PositionToChunkPosition(loadingCenter.position);
        bool retValue = newCentralPos != centralChunkPosition;
        centralChunkPosition = newCentralPos;
        return retValue;
    }
    
    public void EvolveAllChunksToTheProperState()
    {
        chunkThread?.Abort();
        chunkThread = new Thread(new ThreadStart(EvolveAllChunksToTheProperStateThreaded));
        chunkThread.Start();
    }
    
    private void EvolveAllChunksToTheProperStateThreaded()
    {
        chunkConfigurator.UpdateObjectiveStatesOfChunks();
        chunkEvolver.EvolveChunks();
    }


    #endregion
    




}

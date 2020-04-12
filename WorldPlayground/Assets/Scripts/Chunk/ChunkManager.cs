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
    internal Vector2Int centralChunkPosition { get; private set; }
    [SerializeField] internal GameObject chunkPrefab;
    [Space]
    [Tooltip("Max distance of the chunks generated at that moment.")]
    [SerializeField] internal int radiusOfGeneratedChunks = 32;

    internal List<Chunk> chunks = new List<Chunk>();
    internal Chunk[,] chunksArray = new Chunk[0,0];

    internal bool revalidateChunks = false;

    private ChunkConfigurator chunkConfigurator;
    internal ChunkEvolver chunkEvolver;

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
            chunkConfigurator = new ChunkConfigurator();
            chunkEvolver = new ChunkEvolver();
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
        if (chunkConfigurator.waitingForUnityWorkToStart)
        {
            if (chunkConfigurator.waitingForUnityWorkToEnd)
                Debug.LogWarning("Shouldn't be waiting to start if it is already executing (waitingForUnityWorkToEnd = true)");
            
            chunkConfigurator.UpdateWorldChunks();
        }
            
    }

    private IEnumerator UpdateChunks() {
        while (true)
        {
            if (!chunkConfigurator.waitingForUnityWorkToEnd) // Avoid interrupting Unity Work
                if (UpdateCentralChunkPosition() || chunks.Count == 0 || revalidateChunks)
                {
                    revalidateChunks = false;
                    chunkConfigurator.RestructureChunks();
                }

            yield return new WaitForSeconds(radiusOfGeneratedChunks/40); // The time between chunks updates is dynamic. It depends on the number of loaded chunks
        }
    }
    
    private bool UpdateCentralChunkPosition() // Returns true if the position is updated
    {
        Vector2Int newCentralPos = PositionToChunkPosition(loadingCenter.position);
        bool retValue = newCentralPos != centralChunkPosition;
        centralChunkPosition = newCentralPos;
        return retValue;
    }
    
    #endregion

}
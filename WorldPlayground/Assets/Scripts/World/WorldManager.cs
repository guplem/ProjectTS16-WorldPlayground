using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [Tooltip("The transform that will be the center of the 'game action'")]
    [SerializeField] private Transform loadingCenter;
    [SerializeField] private GameObject chunkPrefab;

    [Space]
    [Tooltip("Time between each check of the chunks that should be generated at that moment.")]
    [SerializeField] private float timeBetweenGenerationOfChunks = 1f;
    [Space]
    [Tooltip("Max distance of the chunks generated at that moment.")]
    [SerializeField] private int radiusOfGeneratedChunks = 10;
    [SerializeField] private int radiusOfDrawnChunks = 5;

    public Vector2Int centralChunkPosition { get; private set; }

    private ChunkGenerator chunkGenerator;
    
    private HashSet<Chunk> chunks = new HashSet<Chunk>();
    public Chunk[,] chunksArray { get; private set; }
    public List<Chunk> chunksToDrawSortedByDistance = new List<Chunk>();
    public List<Chunk> notDrawnChunksWithDataSortedByDistance = new List<Chunk>();

    public static WorldManager Instance { get; private set; }
    
// ==================== SETUP AND DEBUG ==================== //

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Multiple WorldManager exist. Destroying the last one registered.", gameObject);
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        chunkGenerator = new ChunkGenerator();
        StartCoroutine(nameof(UpdateChunks));
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        var center = PositionToChunkPosition(loadingCenter.position);
        Gizmos.DrawSphere(new Vector3(center.x, 0f, center.y), 0.5f);
    }
    
    
// ==================== UTILITY METHODS ==================== //
    
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


// ==================== WORLD BUILDING ==================== //
    
    private bool UpdateCentralChunkPosition() // Returns true if the position is updated
    {
        Vector2Int newCentralPos = PositionToChunkPosition(loadingCenter.position);
        bool retValue = newCentralPos != centralChunkPosition;
        centralChunkPosition = newCentralPos;
        return retValue;
    }

    private void Update()
    {
        // Start the calculus of a mesh each frame
        CalculateMeshOfTheClosestChunkWithData();
        // Draw one each frame
        DrawTheClosesAvailableChunk();
    }

    private void DrawTheClosesAvailableChunk()
    {
        lock (chunksToDrawSortedByDistance)
        {
            if (chunksToDrawSortedByDistance.Count <= 0) return;

            Chunk chunkToDraw = chunksToDrawSortedByDistance[0];
        
            chunkToDraw.DrawChunk();
            chunksToDrawSortedByDistance.Remove(chunkToDraw);
        }

    }
    
    private void CalculateMeshOfTheClosestChunkWithData()
    {
        lock (notDrawnChunksWithDataSortedByDistance)
        {
            if (notDrawnChunksWithDataSortedByDistance.Count <= 0) return;

            //if (distance > radiusOfDrawnChunks) return;

            Chunk chunkToDraw = notDrawnChunksWithDataSortedByDistance[0];

            chunkToDraw.UpdateMeshToDrawChunk();
            notDrawnChunksWithDataSortedByDistance.Remove(chunkToDraw);
        }
    }

    private IEnumerator UpdateChunks() {
        while (true)
        {
            if (UpdateCentralChunkPosition() || chunks.Count == 0)
                if (chunkGenerator.GenerateChunks(chunks, chunkPrefab, centralChunkPosition, radiusOfGeneratedChunks))
                    UpdateChunksArray();

            yield return new WaitForSeconds(timeBetweenGenerationOfChunks);
        }
    }

    private void UpdateChunksArray()
    {
        chunksArray = new Chunk[radiusOfGeneratedChunks*2+1, radiusOfGeneratedChunks*2+1];
        foreach (var chunk in chunks)
        {
            int x = ((radiusOfGeneratedChunks*Chunk.size.x)-(centralChunkPosition.x-chunk.position.x))/Chunk.size.x;
            int y = ((radiusOfGeneratedChunks*Chunk.size.x)-(centralChunkPosition.y-chunk.position.y))/Chunk.size.x;
            chunk.arrayPos = new Vector2Int(x, y);
            chunksArray[x, y] = chunk;
        }
    }




}

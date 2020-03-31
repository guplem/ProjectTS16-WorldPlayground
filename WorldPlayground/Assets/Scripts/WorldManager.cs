using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Boo.Lang;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [Tooltip("The transform that will be the center of the 'game action'")]
    [SerializeField] private Transform loadingCenter;
    [SerializeField] private GameObject chunkPrefab;

    [Space]
    [Tooltip("Time between each check of the chunks that should be generated at that moment.")]
    [SerializeField] private float deltaTimeBetweenChunkGeneration;

    [Space]
    [Tooltip("Max distance of the chunks generated at that moment.")]
    [SerializeField] private int radiusGenerationChunks;

    
    private ChunkGenerator chunkGenerator;
    private HashSet<Chunk> chunks = new HashSet<Chunk>();
    private Chunk[,] chunksArray;
    
    private Vector2Int centralChunkPosition;

    
    public static WorldManager Instance { get; private set; }
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
        StartCoroutine(nameof(GenerateChunks));
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        var center = WorldPositionToChunkPosition(loadingCenter.position);
        Gizmos.DrawSphere(new Vector3(center.x, 0f, center.y), 0.5f);
    }
    
    public Vector2Int WorldPositionToChunkPosition(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / Chunk.size.x) * Chunk.size.x,
            Mathf.FloorToInt(position.z / Chunk.size.x) * Chunk.size.x);
    }
    
    private void UpdateChunksArray()
    {
        chunksArray = new Chunk[radiusGenerationChunks*2+1, radiusGenerationChunks*2+1];
        foreach (var chunk in chunks)
        {
            int x = ((radiusGenerationChunks*Chunk.size.x)-(centralChunkPosition.x-chunk.position.x))/Chunk.size.x;
            int y = ((radiusGenerationChunks*Chunk.size.x)-(centralChunkPosition.y-chunk.position.y))/Chunk.size.x;
            chunk.arrayPos = new Vector2Int(x, y);
            chunksArray[x, y] = chunk;
        }
    }

    public Chunk GetChunkOfPosition(Vector3 position)
    {
        Vector2Int chunkPos = WorldPositionToChunkPosition(position);
        int x = ((radiusGenerationChunks*Chunk.size.x)-(centralChunkPosition.x-chunkPos.x))/Chunk.size.x;
        int y = ((radiusGenerationChunks*Chunk.size.x)-(centralChunkPosition.y-chunkPos.y))/Chunk.size.x;
        return chunksArray[x, y];
    }
    
    private IEnumerator GenerateChunks() {
        while (true)
        {
            if (UpdateCentralChunkPosition())
            {
                chunkGenerator.GenerateChunks(chunks, chunkPrefab, centralChunkPosition, radiusGenerationChunks);
                UpdateChunksArray();
            }
            
            Debug.Log("DONE");
            yield return new WaitForSeconds(deltaTimeBetweenChunkGeneration);
        }
    }

    private bool UpdateCentralChunkPosition() // Returns true if the position is updated
    {
        Vector2Int newCentralPos = WorldPositionToChunkPosition(loadingCenter.position);
        bool retValue = newCentralPos != centralChunkPosition;
        centralChunkPosition = newCentralPos;
        return retValue;
    }




}

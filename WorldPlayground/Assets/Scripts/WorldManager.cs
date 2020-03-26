using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldManager : MonoBehaviour
{

    [Tooltip("The transform that will be the center of the 'game action'")]
    [SerializeField] private Transform loadingCenter;
    [SerializeField] private GameObject chunkPrefab;
    [Space]
    [Tooltip("Time between each check of the chunks that should be generated at that moment.")]
    [SerializeField] private float deltaTimeBetweenGenerationChunksChecks;
    [Tooltip("Time between each check of the chunks that should be active at that moment.")]
    [SerializeField] private float deltaTimeBetweenActiveChunksChecks;
    [Tooltip("Time between each check of the chunks that should be visible at that moment.")]
    [SerializeField] private float deltaTimeBetweenVisualChunksChecks;
    [Space]
    [Tooltip("Max distance of the chunks generated at that moment.")]
    [SerializeField] private int radiusGenerationChunksChecks;
    [Tooltip("Max distance of the chunks active at that moment.")]
    [SerializeField] private int radiusActiveChunksChecks;
    [Tooltip("Max distance of the chunks visible at that moment.")]
    [SerializeField] private int radiusVisualChunksChecks;

    private HashSet<ChunkManager> chunks = new HashSet<ChunkManager>();
    
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
        CheckValuesValidity();
        
        StartCoroutine(nameof(GenerateChunks));
        StartCoroutine(nameof(ProcessActiveChunks));
        StartCoroutine(nameof(VisualizeChunks));
    }

    private void CheckValuesValidity()
    {
        if (radiusGenerationChunksChecks <= radiusActiveChunksChecks)
            Debug.LogError("The radiusGenerationChunksChecks must greater than radiusActiveChunksChecks", gameObject);
        if (radiusActiveChunksChecks <= radiusVisualChunksChecks)
            Debug.LogError("The radiusActiveChunksChecks must greater than radiusVisualChunksChecks", gameObject);

        if (CheckIfDeltaTimesAreMultiples())
            Debug.LogWarning(
                "The values of deltaTimeBetweenGenerationChunksChecks, deltaTimeBetweenActiveChunksChecks and deltaTimeBetweenVisualChunksChecks shouldn't be multiples to avoid lag spikes.",
                gameObject);
    }

    private bool CheckIfDeltaTimesAreMultiples()
    {
        return
            deltaTimeBetweenGenerationChunksChecks % deltaTimeBetweenActiveChunksChecks == 0 ||
            deltaTimeBetweenGenerationChunksChecks % deltaTimeBetweenVisualChunksChecks == 0 ||

            deltaTimeBetweenActiveChunksChecks % deltaTimeBetweenGenerationChunksChecks == 0 ||
            deltaTimeBetweenActiveChunksChecks % deltaTimeBetweenVisualChunksChecks == 0 ||

            deltaTimeBetweenVisualChunksChecks % deltaTimeBetweenGenerationChunksChecks == 0 ||
            deltaTimeBetweenVisualChunksChecks % deltaTimeBetweenActiveChunksChecks == 0;
    }

    public Vector2Int WorldPositionToChunkPosition(Vector3 position)
    {
        return new Vector2Int(((int)(position.x/ChunkManager.size.x))*ChunkManager.size.x, ((int)(position.z/ChunkManager.size.x))*ChunkManager.size.x);
    }
    
    private Vector2Int[] GetAllChunkPositionsInRadius(int radiusInChunks)
    {
        List<Vector2Int> chunkCoords = new List<Vector2Int>();
        int radiusSqr = radiusInChunks * radiusInChunks;

        Vector2Int loadingCenterPos = WorldPositionToChunkPosition(loadingCenter.transform.position);
        for (int x = -radiusInChunks; x <= radiusInChunks; x++)
            for (int z = -radiusInChunks; z <= radiusInChunks; z++)
                if (x*x+z*z<=radiusSqr)
                    chunkCoords.Add(new Vector2Int(x*ChunkManager.size.x + loadingCenterPos.x, z*ChunkManager.size.x + loadingCenterPos.y));

        return chunkCoords.ToArray();
    }

    private HashSet<ChunkManager> GetChunksOutsideRadius(int radius)
    {
        HashSet<ChunkManager> chunksToRegenerate = new HashSet<ChunkManager>();
        foreach (ChunkManager generatedChunk in chunks)
        {
            if (Vector2.Distance(
                    generatedChunk.transform.position.ToVector2WithoutY(),
                    loadingCenter.position.ToVector2WithoutY())
                > radius * ChunkManager.size.x)
            {
                chunksToRegenerate.Add(generatedChunk);
            }
        }

        return chunksToRegenerate;
    }

    IEnumerator GenerateChunks() {
        while (true)
        {
            // Get chunks that shouldn't be generated now
            ChunkManager[] chunksToRegenerate = GetChunksOutsideRadius(radiusGenerationChunksChecks).ToArray();
            
            // Remove the chunks that should be generated
            foreach (ChunkManager chunkToRegenerate in chunksToRegenerate)
                chunks.Remove(chunkToRegenerate);

            // Look for what chunks are missing to generate
            Vector2Int[] chunkPositionsInArea = GetAllChunkPositionsInRadius(radiusGenerationChunksChecks);
            HashSet<Vector2Int> remainingChunksToGeneratePositions = new HashSet<Vector2Int>();
            for (int c = 0; c < chunkPositionsInArea.Length; c++)
            {
                bool found = false;
                foreach (var chunk in chunks)
                {
                    if (chunk.position.Equals(chunkPositionsInArea[c]))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    remainingChunksToGeneratePositions.Add(chunkPositionsInArea[c]);
            }
            Debug.Log("Chunks to rebuild: " + remainingChunksToGeneratePositions.Count);

            // Generate the missing chunks
            int chunkToGenerateIndex = 0;
            foreach (Vector2Int chunkToGeneratePosition in remainingChunksToGeneratePositions)
            {
                ChunkManager newChunk;
                
                if (chunkToGenerateIndex < chunksToRegenerate.Length)
                    newChunk = chunksToRegenerate[chunkToGenerateIndex];
                else
                    newChunk = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, this.transform).GetComponent<ChunkManager>();

                newChunk.position = chunkToGeneratePosition; //TODO: change to move and generate data in position.

                newChunk.gameObject.name = "Chunk " + chunkToGeneratePosition.x + "_" + chunkToGeneratePosition.y;
                chunks.Add(newChunk);
                
                chunkToGenerateIndex++;
            }
            
            //Destroy non-necessary chunks
            for (int c = chunkToGenerateIndex; c < chunksToRegenerate.Length; c++)
            {
                Debug.Log("Destroying " + chunksToRegenerate[c].gameObject.name);
                Destroy(chunksToRegenerate[c].gameObject);
            }
            
            Debug.Log("Generation cycle completed");
            yield return new WaitForSeconds(deltaTimeBetweenGenerationChunksChecks);
        }
    }

    IEnumerator ProcessActiveChunks() {
        while (true) {

            yield return new WaitForSeconds(deltaTimeBetweenActiveChunksChecks);
        }
    }
    
    IEnumerator VisualizeChunks() {
        while (true) {

            yield return new WaitForSeconds(deltaTimeBetweenVisualChunksChecks);
        }
    }
    
}

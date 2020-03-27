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
    
    private HashSet<Vector2Int> GetAllChunkPositionsInRadius(int radiusInChunks)
    {
        HashSet<Vector2Int> chunkCoords = new HashSet<Vector2Int>();
        int radiusSqr = radiusInChunks * radiusInChunks;

        Vector2Int loadingCenterPos = WorldPositionToChunkPosition(loadingCenter.transform.position);
        for (int x = -radiusInChunks; x <= radiusInChunks; x++)
            for (int z = -radiusInChunks; z <= radiusInChunks; z++)
                if (x*x+z*z<=radiusSqr)
                    chunkCoords.Add(new Vector2Int(x*ChunkManager.size.x + loadingCenterPos.x, z*ChunkManager.size.x + loadingCenterPos.y));

        return chunkCoords;
    }

    private HashSet<ChunkManager> GetGeneratedChunksNotIn(HashSet<Vector2Int> positions)
    {
        HashSet<ChunkManager> chunksToDestroy = new HashSet<ChunkManager>();
        
        foreach (ChunkManager chunk in chunks)
            if (!positions.Contains(chunk.position))
                chunksToDestroy.Add(chunk);

        return chunksToDestroy;
    }

    IEnumerator GenerateChunks() {
        while (true)
        {
            // Get the positions that should have chunks at the moment
            HashSet<Vector2Int> positionsWhereAChunkMustExist = GetAllChunkPositionsInRadius(radiusGenerationChunksChecks);
            
            // Get chunks that shouldn't be generated now as they are (need to be moved)
            ChunkManager[] chunksToRegenerate = GetGeneratedChunksNotIn(positionsWhereAChunkMustExist).ToArray();
            // Remove (from the list) the chunks that shouldn't exist/be generated at the moment
            foreach (ChunkManager chunkToRegenerate in chunksToRegenerate)
                chunks.Remove(chunkToRegenerate);

            // Look where new chunks must generate
            HashSet<Vector2Int> positionsOfCurrentChunks = new HashSet<Vector2Int>();
            foreach (ChunkManager chunk in chunks)
                positionsOfCurrentChunks.Add(chunk.position);
            HashSet<Vector2Int> positionsWhereAChunkMustGenerate = new HashSet<Vector2Int>(positionsWhereAChunkMustExist.Except(positionsOfCurrentChunks));

            // Generate the chunks in the new positions that should have one now
            int chunkToGenerateIndex = 0;
            foreach (Vector2Int chunkToGeneratePosition in positionsWhereAChunkMustGenerate)
            {
                ChunkManager newChunk;

                if (chunkToGenerateIndex < chunksToRegenerate.Length)
                {
                    newChunk = chunksToRegenerate[chunkToGenerateIndex];
                }
                else
                {
                    newChunk = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity, this.transform).GetComponent<ChunkManager>();
                    if (positionsOfCurrentChunks.Count > 0) // Means that it is not the first run (where is normal that new chunks are instantiated)
                        Debug.LogWarning("Instantiating " + newChunk.gameObject.name + " because the current quantity of instantiated chunks is not enough.");
                }

                newChunk.InitializeAt(chunkToGeneratePosition);

                newChunk.gameObject.name = "Chunk " + chunkToGeneratePosition.x + "_" + chunkToGeneratePosition.y;
                chunks.Add(newChunk);
                
                chunkToGenerateIndex++;
            }
            
            //Destroy non-necessary chunks
            for (int c = chunkToGenerateIndex; c < chunksToRegenerate.Length; c++)
            {
                Debug.LogWarning("Destroying " + chunksToRegenerate[c].gameObject.name + " because it was instantiated before and it is no longer necessary.");
                Destroy(chunksToRegenerate[c].gameObject);
            }
            
            Debug.Log("Generation cycle completed. New chunks: " + chunkToGenerateIndex + ". Total number of chunks: " + chunks.Count);
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

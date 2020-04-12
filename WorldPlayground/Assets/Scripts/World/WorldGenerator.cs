using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New World Generator", menuName = "World Generator")]
public class WorldGenerator : ScriptableObject
{

    private short seed;
    [Space]
    [SerializeField] public Cube empty;
    [SerializeField] private Cube bottomEdge;
    [SerializeField] private float biomesBlending = 20f;
    [SerializeField] private List<Biome> biomes;
    [SerializeField] public int minElevation = 20;
    
    public void Setup(short seed)
    {
        EasyRandom rnd = new EasyRandom(seed);
        foreach (Biome biome in biomes)
        {
            biome.seed = Convert.ToInt16(rnd.GetRandomInt(ushort.MaxValue)-short.MaxValue);
        }
    }
    
    public Cube GetCube(Vector3 position)
    {
        position = new Vector3((int)position.x, (int)position.y, (int)position.z);
        
        // Outside the world
        if (position.y < 0 || position.y >= Chunk.size.y)
            return empty;
        // Bottom edge of the world
        if (position.y == 0)
            return bottomEdge;
        
        Tuple<Biome, float> biome = GetBiomeAtPosition(position);
        Tuple<Biome, float> edgeBiome = GetEdgedBiomeAtPosition(position, biome.Item1);
        
        // World height generation
        //float blending = Mathf.Sqrt(1 - (biome.Item2-edgeBiome.Item2)*(biome.Item2-edgeBiome.Item2) ); //f(x)=(sqrt(1-x^2)) //x = separation (from 1 to 0)
        float heightFromCurrentBiome = Mathf.Clamp((-(biome.Item2-edgeBiome.Item2) * biomesBlending / ((biome.Item2-edgeBiome.Item2) - 1)), 0.5f, 1) ; //f(x) = -x*0.01/(x-1)
        //int terrainHeight = Mathf.RoundToInt(heightFromCurrentBiome*10);
        
        int terrainHeight = minElevation + Mathf.RoundToInt(  (biome.Item1.GetHeightAtPosition(position) * heightFromCurrentBiome) + (edgeBiome.Item1.GetHeightAtPosition(position) * (1-heightFromCurrentBiome))  );                      

        

        return biome.Item1.GetCube(position, terrainHeight);
    }

    private Tuple<Biome, float> GetBiomeAtPosition(Vector3 position)
    {
        Biome topBiome = null;
        float topValue = 0f;
        foreach (Biome biome in biomes)
        {
            float currentValue = biome.GetProbabilityAtPosition(position);
            if (currentValue >= topValue)
            {
                topValue = currentValue;
                topBiome = biome;
            }
        }
        return new Tuple<Biome, float>(topBiome, topValue);
    }
    
    private Tuple<Biome, float> GetEdgedBiomeAtPosition(Vector3 position, Biome biomeAtPosition)
    {
        Biome topBiome = null;
        float topValue = 0f;
        foreach (Biome biome in biomes)
        {
            float currentValue = biome.GetProbabilityAtPosition(position);
            if (currentValue >= topValue && biome != biomeAtPosition)
            {
                topValue = currentValue;
                topBiome = biome;
            }
        }
        return new Tuple<Biome, float>(topBiome, topValue);
    }
}

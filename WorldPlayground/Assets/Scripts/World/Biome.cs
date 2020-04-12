using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Biome", menuName = "Biome")]
public class Biome : ScriptableObject
{
    [SerializeField] private float size = 125;
    [SerializeField] public float smoothness = 70;
    [SerializeField] public int maxAdditionalElevation = 65;
    [Space]
    [SerializeField] private Cube fillingCube;
    [SerializeField] private Cube surfaceCube;
    public short seed { get; set; }
    
    public Cube GetCube(Vector3 position, int terrainHeight)
    {
        if (position.y < terrainHeight)
            return fillingCube;
        if (position.y == terrainHeight)
            return surfaceCube;
        else
            return WorldManager.Instance.worldGenerator.empty;
    }

    public float GetProbabilityAtPosition(Vector3 position)
    {
        return Mathf.PerlinNoise(position.x / size + seed, position.z / size + seed);
    }

    public float GetHeightAtPosition(Vector3 position)
    {
        return Mathf.PerlinNoise(position.x/smoothness+seed, position.z/smoothness+seed) * maxAdditionalElevation;
    }
}

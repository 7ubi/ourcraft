using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Biome", menuName = "Ourcraft/Biome")]
public class Biome : ScriptableObject
{
    public string name;
    
    public float scale;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public int maxGeneratingHeight;
    public int amplitude;
    public Vector2 offset;
    public int topBlock;
    public int secondaryBlock;
    public float vegetationThreshold;
    public int minVegetationHeight;
    public int maxVegetationHeight;
    public bool hasTree = true;
    public bool hasCactus = false;

    public float GetHeight(float x, float z, int seed)
    {
        return Noise.GetNoise(x, z, seed, amplitude, scale, 1, octaves, persistance, lacunarity) * maxGeneratingHeight;
    }
}

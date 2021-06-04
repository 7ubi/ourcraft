using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float GetNoise(float x, float z, int seed, float amplitude, float scale, float frequency, int octaves, float persistance, float lacunarity)
    {
        var height = 0f;
        
        for (var i = 0; i < octaves; i++)
        {
            var sampleX = (x  + seed) / scale * frequency;
            var sampleZ = (z + seed) / scale * frequency;

            var perlinVal = Mathf.PerlinNoise(sampleX, sampleZ);
            height += perlinVal * amplitude;
            
            frequency *= lacunarity;
            amplitude *= persistance;
        }
        
        return Mathf.InverseLerp(0, amplitude, height);
    }
    
    public static float Perlin3D(float x, float y, float z)
    {
        var ab = Mathf.PerlinNoise(x, y);
        var bc = Mathf.PerlinNoise(y, z);
        var ac = Mathf.PerlinNoise(x, z);
        
        var ba = Mathf.PerlinNoise(y, x);
        var cb = Mathf.PerlinNoise(z, y);
        var ca = Mathf.PerlinNoise(z, x);

        var abc = ab + bc + ac + ba + cb + ca;
        
        return abc / 6f;
    }
}

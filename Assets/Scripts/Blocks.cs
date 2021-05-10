using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

public class Blocks: MonoBehaviour
{
    public List<Vector2> GetBlockUV(int BlockID)
    {
        return BlockID switch
        {
            1 => Dirt(),
            3 => Stone(),
            _ => null
        };
    }

    private static List<Vector2> Dirt()
    {
        var uvs = new List<Vector2>();

        const float xMin = 0.1f;
        const float xMax = 0.4f;
        const float yMin = 0.1f;
        const float yMax = 0.4f;
        
        uvs.Add(new Vector2(xMin, yMax));
        uvs.Add(new Vector2(xMax, yMax));
        uvs.Add(new Vector2(xMax, yMin));
        uvs.Add(new Vector2(xMin, yMin));

        return uvs;
    }
    private static List<Vector2> Stone()
    {
        var uvs = new List<Vector2>();

        const float xMin = 0.1f;
        const float xMax = 0.4f;
        const float yMin = 0.6f;
        const float yMax = 0.9f;
        
        uvs.Add(new Vector2(xMin, yMax));
        uvs.Add(new Vector2(xMax, yMax));
        uvs.Add(new Vector2(xMax, yMin));
        uvs.Add(new Vector2(xMin, yMin));

        return uvs;
    }

    public List<Vector2> GrassTop()
    {
        var uvs = new List<Vector2>();

        const float xMin = 0.6f;
        const float xMax = 0.9f;
        const float yMin = 0.6f;
        const float yMax = 0.9f;
            
        uvs.Add(new Vector2(xMin, yMax));
        uvs.Add(new Vector2(xMax, yMax));
        uvs.Add(new Vector2(xMax, yMin));
        uvs.Add(new Vector2(xMin, yMin));

        return uvs;
    }
    
    public List<Vector2> GrassSide()
    {
        var uvs = new List<Vector2>();

        const float xMin = 0.6f;
        const float xMax = 0.9f;
        const float yMin = 0.1f;
        const float yMax = 0.4f;
            
        uvs.Add(new Vector2(xMin, yMax));
        uvs.Add(new Vector2(xMax, yMax));
        uvs.Add(new Vector2(xMax, yMin));
        uvs.Add(new Vector2(xMin, yMin));

        return uvs;
    }
    
    public List<Vector2> GrassBot()
    {
        var uvs = new List<Vector2>();

        const float xMin = 0.1f;
        const float xMax = 0.4f;
        const float yMin = 0.1f;
        const float yMax = 0.4f;
            
        uvs.Add(new Vector2(xMin, yMax));
        uvs.Add(new Vector2(xMax, yMax));
        uvs.Add(new Vector2(xMax, yMin));
        uvs.Add(new Vector2(xMin, yMin));

        return uvs;
    }
}

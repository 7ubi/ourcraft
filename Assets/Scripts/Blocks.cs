using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

public class Blocks: MonoBehaviour
{
    // texture atlas from: https://github.com/mikolalysenko/tile-mip-map

    private float _size = 0.0625f;
    
    public List<Vector2> GetBlockUV(int BlockID)
    {
        return BlockID switch
        {
            1 => Dirt(),
            3 => Stone(),
            5 => Leaves(),
            _ => null
        };
    }

    private List<Vector2> Dirt()
    {
        var uvs = new List<Vector2>();

        var xMin = 2 * _size;
        var xMax = 3 * _size;
        var yMin = 1 - _size;
        var yMax = 1;
        
        uvs.Add(new Vector2(xMin, yMax));
        uvs.Add(new Vector2(xMax, yMax));
        uvs.Add(new Vector2(xMax, yMin));
        uvs.Add(new Vector2(xMin, yMin));

        return uvs;
    }
    private List<Vector2> Stone()
    {
        var uvs = new List<Vector2>();

        var xMin = 1 * _size;
        var xMax = 2 * _size;
        var yMin = 1 - _size;
        var yMax = 1;
        
        uvs.Add(new Vector2(xMin, yMax));
        uvs.Add(new Vector2(xMax, yMax));
        uvs.Add(new Vector2(xMax, yMin));
        uvs.Add(new Vector2(xMin, yMin));

        return uvs;
    }

    public List<Vector2> GrassTop()
    {
        var uvs = new List<Vector2>();

        var xMin = 0 * _size;
        var xMax = 1 * _size;
        var yMin = 1 - _size;
        var yMax = 1;
            
        uvs.Add(new Vector2(xMin, yMax));
        uvs.Add(new Vector2(xMax, yMax));
        uvs.Add(new Vector2(xMax, yMin));
        uvs.Add(new Vector2(xMin, yMin));

        return uvs;
    }
    
    public List<Vector2> GrassSide()
    {
        var uvs = new List<Vector2>();

        var xMin = 3 * _size;
        var xMax = 4 * _size;
        var yMin = 1 - _size;
        var yMax = 1;
            
        uvs.Add(new Vector2(xMin, yMax));
        uvs.Add(new Vector2(xMax, yMax));
        uvs.Add(new Vector2(xMax, yMin));
        uvs.Add(new Vector2(xMin, yMin));

        return uvs;
    }
    
    public List<Vector2> GrassBot()
    {
        var uvs = new List<Vector2>();

        var xMin = 2 * _size;
        var xMax = 3 * _size;
        var yMin = 1 - _size;
        var yMax = 1;
            
        uvs.Add(new Vector2(xMin, yMax));
        uvs.Add(new Vector2(xMax, yMax));
        uvs.Add(new Vector2(xMax, yMin));
        uvs.Add(new Vector2(xMin, yMin));

        return uvs;
    }

    public List<Vector2> LogSide()
    {
        var uvs = new List<Vector2>();

        var xMin = 4 * _size;
        var xMax = 5 * _size;
        var yMin = 1 - 2 * _size;
        var yMax = 1 - _size;
            
        uvs.Add(new Vector2(xMin, yMax));
        uvs.Add(new Vector2(xMax, yMax));
        uvs.Add(new Vector2(xMax, yMin));
        uvs.Add(new Vector2(xMin, yMin));

        return uvs;
    }
    
    public List<Vector2> LogTop()
    {
        var uvs = new List<Vector2>();

        var xMin = 5 * _size;
        var xMax = 6 * _size;
        var yMin = 1 - 2 * _size;
        var yMax = 1 - _size;
            
        uvs.Add(new Vector2(xMin, yMax));
        uvs.Add(new Vector2(xMax, yMax));
        uvs.Add(new Vector2(xMax, yMin));
        uvs.Add(new Vector2(xMin, yMin));

        return uvs;
    }

    public List<Vector2> Leaves()
    {
        var uvs = new List<Vector2>();

        var xMin = 5 * _size;
        var xMax = 6 * _size;
        var yMin = 1 - 4 * _size;
        var yMax = 1 - 3 * _size;
            
        uvs.Add(new Vector2(xMin, yMax));
        uvs.Add(new Vector2(xMax, yMax));
        uvs.Add(new Vector2(xMax, yMin));
        uvs.Add(new Vector2(xMin, yMin));

        return uvs;
    }
    
    public List<Vector2> Water()
    {
        var uvs = new List<Vector2>();

        var xMin = 0;
        var xMax = 1;
        var yMin = 0;
        var yMax = 1;
            
        uvs.Add(new Vector2(xMin, yMax));
        uvs.Add(new Vector2(xMax, yMax));
        uvs.Add(new Vector2(xMax, yMin));
        uvs.Add(new Vector2(xMin, yMin));

        return uvs;
    }
}

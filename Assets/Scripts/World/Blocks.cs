using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Blocks
{
    // texture atlas from: https://github.com/mikolalysenko/tile-mip-map

    [SerializeField] public string name;
    [SerializeField] public int id;
    [SerializeField] public int topIndex;
    [SerializeField] public int frontIndex;
    [SerializeField] public int backIndex;
    [SerializeField] public int rightIndex;
    [SerializeField] public int leftIndex;
    [SerializeField] public int botIndex;
    [SerializeField] public Sprite img;
    [SerializeField] public bool isTransparent = false;
    [SerializeField] public float destroyTime = 1f;
    [SerializeField] public int stackSize = 64;
    private static int _size = 16;
    private float _normalized = 1 / (float) _size;
    
    public List<Vector2> TopUVs()
    {
        var rect = GETRect(topIndex);
        
        var uvs = new List<Vector2>
        {
            new Vector2(rect.x, rect.y + _normalized),
            new Vector2(rect.x + _normalized, rect.y + _normalized),
            new Vector2(rect.x + _normalized, rect.y),
            new Vector2(rect.x, rect.y)
        };


        return uvs;
    }
    
    public List<Vector2> RightUVs()
    {
        var rect = GETRect(rightIndex);
        
        var uvs = new List<Vector2>
        {
            new Vector2(rect.x, rect.y + _normalized),
            new Vector2(rect.x + _normalized, rect.y + _normalized),
            new Vector2(rect.x + _normalized, rect.y),
            new Vector2(rect.x, rect.y)
        };


        return uvs;
    }
    
    public List<Vector2> LeftUVs()
    {
        var rect = GETRect(leftIndex);
        
        var uvs = new List<Vector2>
        {
            new Vector2(rect.x, rect.y + _normalized),
            new Vector2(rect.x + _normalized, rect.y + _normalized),
            new Vector2(rect.x + _normalized, rect.y),
            new Vector2(rect.x, rect.y)
        };


        return uvs;
    }
    
    public List<Vector2> FrontUVs()
    {
        var rect = GETRect(frontIndex);
        
        var uvs = new List<Vector2>
        {
            new Vector2(rect.x, rect.y + _normalized),
            new Vector2(rect.x + _normalized, rect.y + _normalized),
            new Vector2(rect.x + _normalized, rect.y),
            new Vector2(rect.x, rect.y)
        };


        return uvs;
    }
    
    public List<Vector2> BackUVs()
    {
        var rect = GETRect(backIndex);
        
        var uvs = new List<Vector2>
        {
            new Vector2(rect.x, rect.y + _normalized),
            new Vector2(rect.x + _normalized, rect.y + _normalized),
            new Vector2(rect.x + _normalized, rect.y),
            new Vector2(rect.x, rect.y)
        };


        return uvs;
    }
    
    public List<Vector2> BotUVs()
    {
        var rect = GETRect(botIndex);
        
        var uvs = new List<Vector2>
        {
            new Vector2(rect.x, rect.y + _normalized),
            new Vector2(rect.x + _normalized, rect.y + _normalized),
            new Vector2(rect.x + _normalized, rect.y),
            new Vector2(rect.x, rect.y)
        };


        return uvs;
    }

    private Vector2 GETRect(int index)
    {
        float y = index / _size;
        var x = index - (y * _size);

        y *= _normalized;
        x *= _normalized;

        y = 1f - _normalized - y;
        
        return new Vector2(x, y);
    }
}

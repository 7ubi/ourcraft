using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

[Serializable]
public class Blocks
{
    // texture atlas from: https://github.com/mikolalysenko/tile-mip-map

    [SerializeField] public int id;
    [SerializeField] public Rect top;
    [SerializeField] public Rect side;
    [SerializeField] public Rect bot;
    [SerializeField] public Sprite img;
    [SerializeField] public bool isTransparent = false;
    
    public List<Vector2> TopUVs()
    {
        var uvs = new List<Vector2>
        {
            new Vector2(top.xMin, top.yMax),
            new Vector2(top.xMax, top.yMax),
            new Vector2(top.xMax, top.yMin),
            new Vector2(top.xMin, top.yMin)
        };


        return uvs;
    }
    
    public List<Vector2> SideUVs()
    {
        var uvs = new List<Vector2>
        {
            new Vector2(side.xMin, side.yMax),
            new Vector2(side.xMax, side.yMax),
            new Vector2(side.xMax, side.yMin),
            new Vector2(side.xMin, side.yMin)
        };


        return uvs;
    }
    
    public List<Vector2> BotUVs()
    {
        var uvs = new List<Vector2>
        {
            new Vector2(bot.xMin, bot.yMax),
            new Vector2(bot.xMax, bot.yMax),
            new Vector2(bot.xMax, bot.yMin),
            new Vector2(bot.xMin, bot.yMin)
        };


        return uvs;
    }
}

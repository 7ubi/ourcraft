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
    [SerializeField] public BlockShape blockShape;
    [SerializeField] public Sprite img;
    [SerializeField] public bool isTransparent;
    [SerializeField] public bool isRotatable;
    [SerializeField] public float destroyTime = 1f;
    [SerializeField] public int stackSize = 64;
    [SerializeField] private int dropID;
    [SerializeField] public bool isInteractable;
    [SerializeField] public bool meltable;
    [SerializeField] public bool melter;
    [SerializeField] public int meltedId;
    private static int _size = 16;
    private float _normalized = 1 / (float) _size;

    public Vector2 GETRect(int index)
    {
        float y = index / _size;
        var x = index - (y * _size);

        y *= _normalized;
        x *= _normalized;

        y = 1f - _normalized - y;
        
        return new Vector2(x, y);
    }

    public int DropID => dropID == 0 ? id : dropID;
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Biome
{
    [SerializeField] public float noiseMult1;
    [SerializeField] public float noiseMult2;
    [SerializeField] public int maxGeneratingHeight;
    [SerializeField] public Vector2 offset;
    [SerializeField] public int topBlock;
    [SerializeField] public int secondaryBlock;
    [SerializeField] public bool hasTree = true;
}

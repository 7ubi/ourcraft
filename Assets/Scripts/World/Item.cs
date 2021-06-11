using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Item
{
    [SerializeField] public string name;
    [SerializeField] public int id;
    [SerializeField] public int stackSize;
    [SerializeField] public Sprite img;
    [SerializeField] public bool meltable;
    [SerializeField] public bool melter;
    [SerializeField] public int meltedId;
}
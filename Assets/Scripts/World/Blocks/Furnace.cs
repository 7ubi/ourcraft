using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Furnace
{
    public Vector3 position;
    
    public int meltingID;
    public int meltingCount;
    public int melterID;
    public int melterCount;
    public int resultID;
    public int resultCount;

    public float time;

    public Furnace(Vector3 position)
    {
        this.position = position;
    }

    public Furnace(FurnaceInfo furnaceInfo)
    {
        position = furnaceInfo.position;
        meltingID = furnaceInfo.meltingID;
        meltingCount = furnaceInfo.meltingCount;
        melterID = furnaceInfo.melterID;
        melterCount = furnaceInfo.melterCount;
        resultID = furnaceInfo.resultID;
        resultCount = furnaceInfo.resultCount;
        time = furnaceInfo.time;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FurnaceInfo
{
    public Vector3 position;
    
    public byte meltingID;
    public byte meltingCount;
    public byte melterID;
    public byte melterCount;
    public byte resultID;
    public byte resultCount;

    public float time;

    public FurnaceInfo(Furnace furnace)
    {
        position = furnace.position;
        meltingID = (byte)furnace.meltingID;
        meltingCount = (byte)furnace.meltingCount;
        melterID = (byte)furnace.melterID;
        melterCount = (byte)furnace.melterCount;
        resultID = (byte)furnace.resultID;
        resultCount = (byte)furnace.resultCount;
        time = furnace.time;
    }
}

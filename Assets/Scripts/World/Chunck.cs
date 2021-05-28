using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunck : MonoBehaviour
{
    private int[,,] _blockIDs;
    private int[,,] _waterIDs;

    public int[,,] WaterIDs
    {
        get => _waterIDs;
        set => _waterIDs = value;
    }
    
    public int[,,] BlockIDs
    {
        get => _blockIDs;
        set => _blockIDs = value;
    }
}

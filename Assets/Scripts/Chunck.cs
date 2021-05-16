using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunck : MonoBehaviour
{
    private int[,,] _blockIDs;

    public int[,,] BlockIDs
    {
        get => _blockIDs;
        set => _blockIDs = value;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunck : MonoBehaviour
{
    private short[,,] _blockIDs;

    public short[,,] BlockIDs
    {
        get => _blockIDs;
        set => _blockIDs = value;
    }
}

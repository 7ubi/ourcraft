using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunck : MonoBehaviour
{
    private short[,,] blockIDs;

    public short[,,] BlockIDs
    {
        get => blockIDs;
        set => blockIDs = value;
    }
}

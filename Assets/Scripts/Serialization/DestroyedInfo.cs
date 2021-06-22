using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class DestroyedInfo
{
    public DestroyedBlockInfo[] destroyedBlockInfos;

    public DestroyedInfo(List<DestroyedBlock> blocks)
    {
        destroyedBlockInfos = new DestroyedBlockInfo[blocks.Count];

        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            destroyedBlockInfos[i] = new DestroyedBlockInfo(block.transform.position, (byte) block.ID);
        }
    }
}

[System.Serializable]
public class DestroyedBlockInfo
{
    public Vector3 pos;
    public byte id;

    public DestroyedBlockInfo(Vector3 pos, byte id)
    {
        this.pos = pos;
        this.id = id;
    }
}

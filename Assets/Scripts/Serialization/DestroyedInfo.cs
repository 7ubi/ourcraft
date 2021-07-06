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
            destroyedBlockInfos[i] = new DestroyedBlockInfo(block.transform.position, (byte) block.ID, block.CurrentTime);
        }
    }
}

[System.Serializable]
public class DestroyedBlockInfo
{
    public Vector3 pos;
    public byte id;
    public float time;

    public DestroyedBlockInfo(Vector3 pos, byte id, float time)
    {
        this.pos = pos;
        this.id = id;
        this.time = time;
    }
}

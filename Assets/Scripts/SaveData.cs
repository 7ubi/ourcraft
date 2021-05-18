using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class SaveData
{

    public PlayerInfo playerInfo = new PlayerInfo();
    public List<ChunckInfo> chuncksInfos;
    public int seed;

    public SaveData(Vector3 pos, Quaternion rotation, int[] itemCount, int[] itemIds,
        int seed, List<GameObject> chuncks, int size, int maxHeight)
    {
        playerInfo.pos = pos;
        playerInfo.rotation = rotation;
        playerInfo.itemCount = itemCount;
        playerInfo.itemIds = itemIds;
        this.seed = seed;
        
        MakeChuncks(chuncks, size, maxHeight);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void MakeChuncks(List<GameObject> chuncks, int size, int maxHeight)
    {
        chuncksInfos = new List<ChunckInfo>();

        foreach (var chunck in chuncks)
        {
            var c = new ChunckInfo();
            c.pos = chunck.transform.position;

            var blockIds = new int[size * maxHeight * size];

            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < maxHeight; y++)
                {
                    for (var z = 0; z < size; z++)
                    {
                        blockIds[x * maxHeight * size + y * size + z] = chunck.GetComponent<Chunck>().BlockIDs[x, y, z];
                    }
                }
            }

            var waterIds = new int[size * size];

            for (var x = 0; x < size; x++)
            {
                for (var z = 0; z < size; z++)
                {
                    waterIds[x * size + z] = chunck.GetComponent<Chunck>().WaterIDs[x, z];
                }
            }

            c.blockIDs = blockIds;
            c.waterIDs = waterIds;
            chuncksInfos.Add(c);
        }
    }
}

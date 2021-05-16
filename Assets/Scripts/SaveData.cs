using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class SaveData
{

    public PlayerInfo playerInfo = new PlayerInfo();

    public SaveData(Vector3 pos, Quaternion rotation, int[] itemCount, int[] itemIds)
    {
        playerInfo.pos = pos;
        playerInfo.rotation = rotation;
        playerInfo.itemCount = itemCount;
        playerInfo.itemIds = itemIds;
    }
}

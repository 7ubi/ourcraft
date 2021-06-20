using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class PlayerInfo
{
    public Vector3 pos;
    public Quaternion rotation;
    public int health;
    public float currentTime;
    public int[] itemCount;
    public int[] itemIds;
    public int seed;
    public int air;
    public float underWaterTime;

    public PlayerInfo(Vector3 pos, Quaternion rotation, int[] itemCount, int[] itemIds,
        int seed, int health, float currentTime, int air, float underWaterTime)
    {
        this.pos = pos;
        this.rotation = rotation;
        this.itemCount = itemCount;
        this.itemIds = itemIds;
        this.seed = seed;
        this.health = health;
        this.currentTime = currentTime;
        this.air = air;
        this.underWaterTime = underWaterTime;
    }
}

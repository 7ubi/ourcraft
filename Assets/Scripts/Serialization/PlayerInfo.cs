using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class PlayerInfo
{
    public Vector3 pos;
    public float yaw;
    public float pitch;
    public int health;
    public float currentTime;
    public int[] itemCount;
    public int[] itemIds;
    public int seed;
    public int air;
    public float underWaterTime;

    public PlayerInfo(Vector3 pos, float yaw, float pitch, int[] itemCount, int[] itemIds,
        int seed, int health, float currentTime, int air, float underWaterTime)
    {
        this.pos = pos;
        this.yaw = yaw;
        this.pitch = pitch;
        this.itemCount = itemCount;
        this.itemIds = itemIds;
        this.seed = seed;
        this.health = health;
        this.currentTime = currentTime;
        this.air = air;
        this.underWaterTime = underWaterTime;
    }
}

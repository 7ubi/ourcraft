using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class PlayerInfo
{
    public Vector3 pos;
    public Quaternion rotation;
    public int health;
    public int[] itemCount;
    public int[] itemIds;
    public int seed;

    public PlayerInfo(Vector3 pos, Quaternion rotation, int[] itemCount, int[] itemIds, int seed, int health)
    {
        this.pos = pos;
        this.rotation = rotation;
        this.itemCount = itemCount;
        this.itemIds = itemIds;
        this.seed = seed;
        this.health = health;
    }
}

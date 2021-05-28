using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class PlayerInfo
{
    public Vector3 pos;
    public Quaternion rotation;
    public int[] itemCount;
    public int[] itemIds;
}

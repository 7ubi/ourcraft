using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockShape", menuName = "Ourcraft/BlockShape")]
public class BlockShape : ScriptableObject
{
    public string blockName;
    public FaceData[] faceData;
}

[System.Serializable]
public class VertData
{
    public Vector3 position;
    public Vector2 uv;

    public Vector3 GetRotatedPosition(Vector3 angles)
    {
        var center = new Vector3(.5f, .5f, .5f);
        var direction = position - center;
        direction = Quaternion.Euler(angles) * direction;
        return direction + center;
    }
}


[System.Serializable]
public class FaceData
{
    public string name;
    public VertData[] vertData;
    public int[] triangles;
    public Vector3 faceOffset;

    public VertData GetVertData(int index)
    {
        return vertData[index];
    }
}

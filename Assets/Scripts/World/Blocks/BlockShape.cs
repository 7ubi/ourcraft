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
}


[System.Serializable]
public class FaceData
{
    public string name;
    public Vector3 normal;
    public VertData[] vertData;
    public int[] triangles;
    public Vector3 faceOffset;
}

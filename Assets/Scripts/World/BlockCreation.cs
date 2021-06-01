using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BlockCreation : MonoBehaviour
{
    [SerializeField] private worldCreation worldCreation;
    [SerializeField] private DestroyBlock destroyBlock;

    public void GenerateBlock(ref int currentIndex, Vector3Int offset, List<Vector3> vertices,
        List<Vector3> normals, List<Vector2> uvs, List<int> indices, FaceData face, Vector2 rect)
    {
        foreach (var vert in face.vertData)
        {
            vertices.Add(vert.position + offset + face.faceOffset);
            uvs.Add(new Vector2(rect.x + vert.uv.x * 0.0625f, rect.y + vert.uv.y * 0.0625f));
            normals.Add(face.normal);
        }

        foreach (var tri in face.triangles)
        {
            indices.Add(currentIndex + tri);
        }

        currentIndex += 4;
    }
    
    public void GenerateWaterBlock(ref int currentIndex, Vector3Int offset, List<Vector3> vertices,
        List<Vector3> normals, List<Vector2> uvs, List<int> indices, FaceData face)
    {
        foreach (var vert in face.vertData)
        {
            vertices.Add(vert.position + offset + face.faceOffset);
            uvs.Add(vert.uv);
            normals.Add(face.normal);
        }

        foreach (var tri in face.triangles)
        {
            indices.Add(currentIndex + tri);
        }

        currentIndex += 4;
    }
}

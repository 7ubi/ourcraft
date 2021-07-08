using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BlockCreation : MonoBehaviour
{
    [SerializeField] private worldCreation worldCreation;
    [SerializeField] private DestroyBlock destroyBlock;

    private Vector3Int[] _normals = new Vector3Int[6]
    {
        new Vector3Int(0, 0, -1),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0),
    };

    public void GenerateBlock(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals,
         List<Vector2> uvs, List<int> indices, FaceData face, Vector2 rect, int index, int orientation)
    {
        var rot = orientation switch
        {
            0 => 180f,
            5 => 270f,
            1 => 0f,
            _ => 90f
        };

        foreach (var vert in face.vertData)
        {
            vertices.Add(vert.GetRotatedPosition(new Vector3(0, rot, 0)) + offset + face.faceOffset);
            uvs.Add(new Vector2(rect.x + vert.uv.x * 0.0625f, rect.y + vert.uv.y * 0.0625f));
            normals.Add(_normals[index]);
        }

        foreach (var tri in face.triangles)
        {
            indices.Add(currentIndex + tri);
        }

        currentIndex += 4;
    }
    
    public void GenerateWaterBlock(ref int currentIndex, Vector3Int offset, List<Vector3> vertices,
        List<Vector3> normals, List<Vector2> uvs, List<int> indices, FaceData face, int index, int orientation)
    {
        var rot = orientation switch
        {
            0 => 180f,
            5 => 270f,
            1 => 0f,
            _ => 90f
        };
        
        foreach (var vert in face.vertData)
        {
            vertices.Add(vert.GetRotatedPosition(new Vector3(0, rot, 0)) + offset + face.faceOffset);
            uvs.Add(vert.uv);
            normals.Add(_normals[index]);
        }

        foreach (var tri in face.triangles)
        {
            indices.Add(currentIndex + tri);
        }

        currentIndex += 4;
    }

    public void GenerateSpriteToVoxel(ref int currentIndex, Vector3Int offset, List<Vector3> vertices,
        List<Vector3> normals, List<Color32> colors, Color32 color, List<Vector2> uvs, List<int> indices,
        FaceData face, int index)
    {
        foreach (var vert in face.vertData)
        {
            vertices.Add(vert.position + offset + face.faceOffset);
            colors.Add(color);
            uvs.Add(vert.uv);
            normals.Add(_normals[index]);
        }

        foreach (var tri in face.triangles)
        {
            indices.Add(currentIndex + tri);
        }

        currentIndex += 4;
    }
}

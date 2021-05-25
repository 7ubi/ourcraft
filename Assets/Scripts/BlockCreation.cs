using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BlockCreation : MonoBehaviour
{
    [SerializeField] private worldCreation worldCreation;
    [SerializeField] private DestroyBlock destroyBlock;
    
    public void GenerateBlock_Top(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int id, float min, float max, bool isBlock)
    {
        vertices.Add(new Vector3(min, max, max) + offset);
        vertices.Add(new Vector3(max, max, max) + offset);
        vertices.Add(new Vector3(max, max, min) + offset);
        vertices.Add(new Vector3(min, max, min) + offset);

        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);

        uvs.AddRange(isBlock ? worldCreation.Blocks[id].TopUVs() : destroyBlock.GetUVs());

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    public void GenerateBlock_Right(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int id, float min, float max, bool isBlock)
    {
        vertices.Add(new Vector3(max, max, min) + offset);
        vertices.Add(new Vector3(max, max, max) + offset);
        vertices.Add(new Vector3(max, min, max) + offset);
        vertices.Add(new Vector3(max, min, min) + offset);

        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        
        uvs.AddRange(isBlock ? worldCreation.Blocks[id].RightUVs() : destroyBlock.GetUVs());

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    public void GenerateBlock_Left(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int id, float min, float max, bool isBlock)
    {
        vertices.Add(new Vector3(min, max, max) + offset);
        vertices.Add(new Vector3(min, max, min) + offset);
        vertices.Add(new Vector3(min, min, min) + offset);
        vertices.Add(new Vector3(min, min, max) + offset);

        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        
        uvs.AddRange(isBlock ? worldCreation.Blocks[id].LeftUVs() : destroyBlock.GetUVs());

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    public void GenerateBlock_Forward(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int id, float min, float max, bool isBlock)
    {
        vertices.Add(new Vector3(max, max, max) + offset);
        vertices.Add(new Vector3(min, max, max) + offset);
        vertices.Add(new Vector3(min, min, max) + offset);
        vertices.Add(new Vector3(max, min, max) + offset);

        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);

        uvs.AddRange(isBlock ? worldCreation.Blocks[id].FrontUVs() : destroyBlock.GetUVs());

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    public void GenerateBlock_Back(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int id, float min, float max, bool isBlock)
    {
        vertices.Add(new Vector3(min, max, min) + offset);
        vertices.Add(new Vector3(max, max, min) + offset);
        vertices.Add(new Vector3(max, min, min) + offset);
        vertices.Add(new Vector3(min, min, -min) + offset);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        uvs.AddRange(isBlock ? worldCreation.Blocks[id].BackUVs() : destroyBlock.GetUVs());
        
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    public void GenerateBlock_Bottom(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int id, float min, float max, bool isBlock)
    {
        vertices.Add(new Vector3(min, min, min) + offset);
        vertices.Add(new Vector3(max, min, min) + offset);
        vertices.Add(new Vector3(max, min, max) + offset);
        vertices.Add(new Vector3(min, min, max) + offset);

        normals.Add(Vector3.down);
        normals.Add(Vector3.down);
        normals.Add(Vector3.down);
        normals.Add(Vector3.down);

        uvs.AddRange(isBlock ? worldCreation.Blocks[id].BotUVs() : destroyBlock.GetUVs());

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }
}

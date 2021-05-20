using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Water : MonoBehaviour
{
    [SerializeField] private GameObject waterChunck;
    [SerializeField] private BlockTypes blockTypes;
    [SerializeField] private Blocks blocks;
    [SerializeField] private worldCreation worldCreation;
    [SerializeField] private WaterBlocks waterBlocks;
    
    // water shader https://www.youtube.com/watch?v=gRq-IdShxpU

    private int[,,] _waterIds;
    [SerializeField] private int height;
    private Vector2 _offset;

    public void CreateWater(Vector2 offset)
    {
        _waterIds = new int[worldCreation.Size1,  worldCreation.MAXHeight,worldCreation.Size1];
        _offset = offset;
    }

    public void AddWater(int x, int y, int z)
    {
        _waterIds[x, y, z] = 1;
    }

    public void GenerateWater(Transform chunckParent)
    {
        var chunck = chunckParent.GetComponent<Chunck>();

        if (chunckParent.childCount > 0)
        {
            for (var i = chunckParent.childCount - 1; i >= 0; i--)
            {
                Destroy(chunckParent.GetChild(i).gameObject);
            }
        }
        
        var w = Instantiate(waterChunck, chunckParent);
        
        if (chunck.WaterIDs != null)
            _waterIds = chunck.WaterIDs;
        else
        {
            chunck.WaterIDs = _waterIds;
        }

        var newMesh = new Mesh();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var indices = new List<int>();

        var currentIndex = 0;

        for (var x = 0; x < worldCreation.Size1; x++)
        {
            for(var y = 0; y < worldCreation.MAXHeight; y++)
            {
                for (var z = 0; z < worldCreation.Size1; z++)
                {
                    var offset = new Vector3(x, y, z);
                    if (_waterIds[x, y, z] == 0) continue;
                    var isTop = false;
                    if (y <= worldCreation.MAXHeight - 1)
                    {
                        if (_waterIds[x, y + 1, z] == 0)
                        {
                            GenerateBlock_Top(ref currentIndex, offset, vertices, normals, uvs, indices);
                            GenerateBlock_BottomOfTop(ref currentIndex, offset, vertices, normals, uvs, indices);
                            isTop = true;
                        }
                    }
                    
                    if (x < worldCreation.Size1 - 1)
                    {
                        if (_waterIds[x + 1, y, z] == 0)
                            GenerateBlock_Right(ref currentIndex, offset, vertices, normals, uvs, indices, isTop);
                    }

                    if (x >= 1)
                    {
                        if (_waterIds[x - 1, y, z] == 0)
                            GenerateBlock_Left(ref currentIndex, offset, vertices, normals, uvs, indices, isTop);
                    }

                    if (z < worldCreation.Size1 - 1)
                    {
                        if (_waterIds[x, y, z + 1] == 0)
                            GenerateBlock_Forward(ref currentIndex, offset, vertices, normals, uvs, indices, isTop);
                    }

                    if (z >= 1)
                    {
                        if (_waterIds[x, y, z - 1] == 0)
                            GenerateBlock_Back(ref currentIndex, offset, vertices, normals, uvs, indices, isTop);
                    }
                    
                    if (y != 0)
                    {
                        if (_waterIds[x, y - 1, z] == 0)
                            GenerateBlock_Bottom(ref currentIndex, offset, vertices, normals, uvs, indices);
                    }
                }
            }
        }
        
        newMesh.SetVertices(vertices);
        newMesh.SetNormals(normals);
        newMesh.SetUVs(0, uvs);
        newMesh.SetIndices(indices, MeshTopology.Triangles, 0);

        newMesh.RecalculateTangents();
        w.GetComponent<MeshFilter>().mesh = newMesh;
        
    }

    private void GenerateBlock_Top(ref int currentIndex, Vector3 offset, List<Vector3> vertices,
        List<Vector3> normals, List<Vector2> uvs, List<int> indices)
    {
        vertices.Add(new Vector3(0f, 0.9f, 1f) + offset);
        vertices.Add(new Vector3(1f, 0.9f, 1f) + offset);
        vertices.Add(new Vector3(1f, 0.9f, 0f) + offset);
        vertices.Add(new Vector3(0f, 0.9f, 0f) + offset);

        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        
        uvs.AddRange(blocks.Water());
        

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }
    
    private void GenerateBlock_BottomOfTop(ref int currentIndex, Vector3 offset, List<Vector3> vertices,
        List<Vector3> normals, List<Vector2> uvs, List<int> indices)
    {
        vertices.Add(new Vector3(0f, 0.9f, 0f) + offset);
        vertices.Add(new Vector3(1f, 0.9f, 0f) + offset);
        vertices.Add(new Vector3(1f, 0.9f, 1f) + offset);
        vertices.Add(new Vector3(0f, 0.9f, 1f) + offset);

        normals.Add(Vector3.down);
        normals.Add(Vector3.down);
        normals.Add(Vector3.down);
        normals.Add(Vector3.down);

        uvs.AddRange(blocks.Water());

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }
    
    private void GenerateBlock_Right(ref int currentIndex, Vector3 offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, bool isTop)
    {
        vertices.Add(new Vector3(1f, waterBlocks.GetSideHeight(isTop), 0f) + offset);
        vertices.Add(new Vector3(1f, waterBlocks.GetSideHeight(isTop), 1f) + offset);
        vertices.Add(new Vector3(1f, 0f, 1f) + offset);
        vertices.Add(new Vector3(1f, 0f, 0f) + offset);

        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        
        uvs.AddRange(blocks.Water());

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    private void GenerateBlock_Left(ref int currentIndex, Vector3 offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, bool isTop)
    {
        vertices.Add(new Vector3(0f, waterBlocks.GetSideHeight(isTop), 1f) + offset);
        vertices.Add(new Vector3(0f, waterBlocks.GetSideHeight(isTop), 0f) + offset);
        vertices.Add(new Vector3(0f, 0f, 0f) + offset);
        vertices.Add(new Vector3(0f, 0f, 1f) + offset);

        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        
        uvs.AddRange(blocks.Water());
        

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    private void GenerateBlock_Forward(ref int currentIndex, Vector3 offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, bool isTop)
    {
        vertices.Add(new Vector3(1f, waterBlocks.GetSideHeight(isTop), 1f) + offset);
        vertices.Add(new Vector3(0f, waterBlocks.GetSideHeight(isTop), 1f) + offset);
        vertices.Add(new Vector3(0f, 0f, 1f) + offset);
        vertices.Add(new Vector3(1f, 0f, 1f) + offset);

        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);

        uvs.AddRange(blocks.Water());

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    private void GenerateBlock_Back(ref int currentIndex, Vector3 offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, bool isTop)
    {
        vertices.Add(new Vector3(0f, waterBlocks.GetSideHeight(isTop), 0f) + offset);
        vertices.Add(new Vector3(1f, waterBlocks.GetSideHeight(isTop), 0f) + offset);
        vertices.Add(new Vector3(1f, 0f, 0f) + offset);
        vertices.Add(new Vector3(0f, 0f, -0) + offset);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        uvs.AddRange(blocks.Water());
        
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    private void GenerateBlock_Bottom(ref int currentIndex, Vector3 offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices)
    {
        vertices.Add(new Vector3(0f, 0f, 0f) + offset);
        vertices.Add(new Vector3(1f, 0f, 0f) + offset);
        vertices.Add(new Vector3(1f, 0f, 1f) + offset);
        vertices.Add(new Vector3(0f, 0f, 1f) + offset);

        normals.Add(Vector3.down);
        normals.Add(Vector3.down);
        normals.Add(Vector3.down);
        normals.Add(Vector3.down);

        uvs.AddRange(blocks.Water());

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    public int Height => height;
}

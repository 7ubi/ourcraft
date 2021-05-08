using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class world_creation : MonoBehaviour
{
    //base code: https://github.com/Absurdponcho/BlockGame/blob/master/Assets/VoxelChunk.cs
    [SerializeField] int Size = 16;
    [SerializeField] private int maxHeight;
    public short[,,] BlockIDs;
    private bool RequiresMeshGeneration = false;
    [SerializeField] private int seed;
    [SerializeField] private GameObject Chunck;


    private void Update()
    {
        if (RequiresMeshGeneration)
        {
            GenerateChunck();
        }
    }

    private void Start()
    {
        
        RequiresMeshGeneration = true;
    }

    void GenerateBlocks(Vector2 offset)
    {
        BlockIDs = new short[Size, Size, Size];
        for (var x = 0; x < Size; x++)
        {
            for (var z = 0; z < Size; z++)
            {
                var height = (int)(Mathf.PerlinNoise((x  + offset.x * 2) * 0.05f + seed, (z + offset.y * 2) * 0.05f  + seed) * (maxHeight));
                for(var y = height; y >= 0; y--){
                    BlockIDs[x, y, z] = 1;
                }
            }
            
        }
    }

    void GenerateChunck()
    {
        for (var x = 0; x < 3; x++)
        {
            for (var z = 0; z < 3; z++)
            {
                var chunck = Instantiate(Chunck, new Vector3(x * Size * 0.5f, 0, z * Size * 0.5f), Quaternion.identity);
                GenerateMesh(chunck);
            }
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    void GenerateMesh(GameObject chunck)
    {
        var position = chunck.transform.position;
        GenerateBlocks(new Vector2(position.x, position.z));
        var newMesh = new Mesh();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var indices = new List<int>();

        var currentIndex = 0;

        for (var x = 0; x < Size; x++)
        {
            for (var y = 0; y < Size; y++)
            {
                for (var z = 0; z < Size; z++)
                {
                    var offset = new Vector3Int(x, y, z) + Vector3Int.FloorToInt(chunck.transform.position);
                    if (BlockIDs[x, y, z] == 0) continue;
                    else
                    {
                        if (y < Size - 1)
                        {
                            if (BlockIDs[x, y + 1, z] == 0)
                                GenerateBlock_Top(ref currentIndex, offset, vertices, normals, uvs, indices,
                                    new Rect());
                        }
                        else
                        {
                            GenerateBlock_Top(ref currentIndex, offset, vertices, normals, uvs, indices,
                                new Rect());
                        }

                        if (x < Size - 1)
                        {
                            if (BlockIDs[x + 1, y, z] == 0)
                                GenerateBlock_Right(ref currentIndex, offset, vertices, normals, uvs, indices,
                                    new Rect());
                        }
                        else
                        {
                            GenerateBlock_Right(ref currentIndex, offset, vertices, normals, uvs, indices,
                                new Rect());
                        }

                        if (x >= 1)
                        {
                            if (BlockIDs[x - 1, y, z] == 0)
                                GenerateBlock_Left(ref currentIndex, offset, vertices, normals, uvs, indices,
                                    new Rect());
                        }
                        else
                        {
                            GenerateBlock_Left(ref currentIndex, offset, vertices, normals, uvs, indices,
                                new Rect());
                        }

                        if (z < Size - 1)
                        {
                            if (BlockIDs[x, y, z + 1] == 0)
                                GenerateBlock_Forward(ref currentIndex, offset, vertices, normals, uvs, indices,
                                    new Rect());
                        }
                        else
                        {
                            GenerateBlock_Forward(ref currentIndex, offset, vertices, normals, uvs, indices,
                                new Rect());
                        }

                        if (z >= 1)
                        {
                            if (BlockIDs[x, y, z - 1] == 0)
                                GenerateBlock_Back(ref currentIndex, offset, vertices, normals, uvs, indices,
                                    new Rect());
                        }
                        else
                        {
                            GenerateBlock_Back(ref currentIndex, offset, vertices, normals, uvs, indices,
                                new Rect());
                        }

                        if (y >= 1)
                        {
                            if (BlockIDs[x, y - 1, z] == 0)
                                GenerateBlock_Bottom(ref currentIndex, offset, vertices, normals, uvs, indices,
                                    new Rect());
                        }
                        else
                        {
                            GenerateBlock_Back(ref currentIndex, offset, vertices, normals, uvs, indices,
                                new Rect());
                        }
                    }
                }
            }
        }

        newMesh.SetVertices(vertices);
        newMesh.SetNormals(normals);
        newMesh.SetUVs(0, uvs);
        newMesh.SetIndices(indices, MeshTopology.Triangles, 0);

        newMesh.RecalculateTangents();
        chunck.GetComponent<MeshFilter>().mesh = newMesh;
        chunck.GetComponent<MeshCollider>().sharedMesh = newMesh;
        // Set texture

        RequiresMeshGeneration = false;
    }

    void GenerateBlock_Top(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, Rect blockUVs)
    {
        vertices.Add(new Vector3(-0.5f, 0.5f, 0.5f) + offset);
        vertices.Add(new Vector3(0.5f, 0.5f, 0.5f) + offset);
        vertices.Add(new Vector3(0.5f, 0.5f, -0.5f) + offset);
        vertices.Add(new Vector3(-0.5f, 0.5f, -0.5f) + offset);

        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);

        uvs.Add(new Vector2(blockUVs.xMin, blockUVs.yMax));
        uvs.Add(new Vector2(blockUVs.xMax, blockUVs.yMax));
        uvs.Add(new Vector2(blockUVs.xMax, blockUVs.yMin));
        uvs.Add(new Vector2(blockUVs.xMin, blockUVs.yMin));

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    void GenerateBlock_Right(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, Rect blockUVs)
    {
        vertices.Add(new Vector3(0.5f, 0.5f, -0.5f) + offset);
        vertices.Add(new Vector3(0.5f, 0.5f, 0.5f) + offset);
        vertices.Add(new Vector3(0.5f, -0.5f, 0.5f) + offset);
        vertices.Add(new Vector3(0.5f, -0.5f, -0.5f) + offset);

        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        normals.Add(Vector3.right);

        uvs.Add(new Vector2(blockUVs.xMin, blockUVs.yMax));
        uvs.Add(new Vector2(blockUVs.xMax, blockUVs.yMax));
        uvs.Add(new Vector2(blockUVs.xMax, blockUVs.yMin));
        uvs.Add(new Vector2(blockUVs.xMin, blockUVs.yMin));

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    void GenerateBlock_Left(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, Rect blockUVs)
    {
        vertices.Add(new Vector3(-0.5f, 0.5f, 0.5f) + offset);
        vertices.Add(new Vector3(-0.5f, 0.5f, -0.5f) + offset);
        vertices.Add(new Vector3(-0.5f, -0.5f, -0.5f) + offset);
        vertices.Add(new Vector3(-0.5f, -0.5f, 0.5f) + offset);

        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        normals.Add(Vector3.left);

        uvs.Add(new Vector2(blockUVs.xMin, blockUVs.yMax));
        uvs.Add(new Vector2(blockUVs.xMax, blockUVs.yMax));
        uvs.Add(new Vector2(blockUVs.xMax, blockUVs.yMin));
        uvs.Add(new Vector2(blockUVs.xMin, blockUVs.yMin));

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    void GenerateBlock_Forward(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, Rect blockUVs)
    {
        vertices.Add(new Vector3(0.5f, 0.5f, 0.5f) + offset);
        vertices.Add(new Vector3(-0.5f, 0.5f, 0.5f) + offset);
        vertices.Add(new Vector3(-0.5f, -0.5f, 0.5f) + offset);
        vertices.Add(new Vector3(0.5f, -0.5f, 0.5f) + offset);

        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);

        uvs.Add(new Vector2(blockUVs.xMin, blockUVs.yMax));
        uvs.Add(new Vector2(blockUVs.xMax, blockUVs.yMax));
        uvs.Add(new Vector2(blockUVs.xMax, blockUVs.yMin));
        uvs.Add(new Vector2(blockUVs.xMin, blockUVs.yMin));

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    void GenerateBlock_Back(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, Rect blockUVs)
    {
        vertices.Add(new Vector3(-0.5f, 0.5f, -0.5f) + offset);
        vertices.Add(new Vector3(0.5f, 0.5f, -0.5f) + offset);
        vertices.Add(new Vector3(0.5f, -0.5f, -0.5f) + offset);
        vertices.Add(new Vector3(-0.5f, -0.5f, -0.5f) + offset);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        uvs.Add(new Vector2(blockUVs.xMin, blockUVs.yMax));
        uvs.Add(new Vector2(blockUVs.xMax, blockUVs.yMax));
        uvs.Add(new Vector2(blockUVs.xMax, blockUVs.yMin));
        uvs.Add(new Vector2(blockUVs.xMin, blockUVs.yMin));

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    void GenerateBlock_Bottom(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, Rect blockUVs)
    {
        vertices.Add(new Vector3(-0.5f, -0.5f, -0.5f) + offset);
        vertices.Add(new Vector3(0.5f, -0.5f, -0.5f) + offset);
        vertices.Add(new Vector3(0.5f, -0.5f, 0.5f) + offset);
        vertices.Add(new Vector3(-0.5f, -0.5f, 0.5f) + offset);

        normals.Add(Vector3.down);
        normals.Add(Vector3.down);
        normals.Add(Vector3.down);
        normals.Add(Vector3.down);

        uvs.Add(new Vector2(blockUVs.xMin, blockUVs.yMax));
        uvs.Add(new Vector2(blockUVs.xMax, blockUVs.yMax));
        uvs.Add(new Vector2(blockUVs.xMax, blockUVs.yMin));
        uvs.Add(new Vector2(blockUVs.xMin, blockUVs.yMin));

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }
}


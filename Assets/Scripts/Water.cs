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
    
    // water shader https://www.youtube.com/watch?v=gRq-IdShxpU

    private int[,] _waterIds;
    [SerializeField] private int height;
    private Vector2 _offset;

    public void CreateWater(Vector2 offset)
    {
        _waterIds = new int[worldCreation.Size1,  worldCreation.Size1];
        _offset = offset;
    }

    public void AddWater(int x, int z)
    {
        _waterIds[x, z] = 1;
    }

    public void GenerateWater(Transform chunckParent)
    {
        var w = Instantiate(waterChunck, chunckParent);

        var chunck = chunckParent.GetComponent<Chunck>();
        
        if (chunck.WaterIDs != null)
            _waterIds = chunck.WaterIDs;
        else
            chunck.WaterIDs = _waterIds;

        var newMesh = new Mesh();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var indices = new List<int>();

        var currentIndex = 0;

        for (var x = 0; x < worldCreation.Size1; x++)
        {
            for (var z = 0; z < worldCreation.Size1; z++)
            {
                var offset = new Vector3(x, height - 0.1f, z);
                if (_waterIds[x, z] == 0) continue;
                GenerateBlock_Top(ref currentIndex, offset, vertices, normals, uvs, indices, _waterIds[x, z]);
                GenerateBlock_Bottom(ref currentIndex, offset, vertices, normals, uvs, indices, _waterIds[x, z]);
            }
        }
        
        newMesh.SetVertices(vertices);
        newMesh.SetNormals(normals);
        newMesh.SetUVs(0, uvs);
        newMesh.SetIndices(indices, MeshTopology.Triangles, 0);

        newMesh.RecalculateTangents();
        w.GetComponent<MeshFilter>().mesh = newMesh;
    }

    private void GenerateBlock_Top(ref int currentIndex, Vector3 offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int id)
    {
        vertices.Add(new Vector3(0f, 1f, 1f) + offset);
        vertices.Add(new Vector3(1f, 1f, 1f) + offset);
        vertices.Add(new Vector3(1f, 1f, 0f) + offset);
        vertices.Add(new Vector3(0f, 1f, 0f) + offset);

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
    
    private void GenerateBlock_Bottom(ref int currentIndex, Vector3 offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int id)
    {
        vertices.Add(new Vector3(0f, 1f, 0f) + offset);
        vertices.Add(new Vector3(1f, 1f, 0f) + offset);
        vertices.Add(new Vector3(1f, 1f, 1f) + offset);
        vertices.Add(new Vector3(0f, 1f, 1f) + offset);

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

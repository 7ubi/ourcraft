using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyedBlock : MonoBehaviour
{
    public int ID { get; set; }
    public float CurrentTime { get; set; } = 0f;
    private worldCreation _worldCreation;
    
    private Mesh _newMesh;
    private List<Vector3> _vertices;
    private List<Vector3> _normals;
    private List<Vector2> _uvs;
    private List<int> _indices;

    [SerializeField] private MeshFilter meshFilter;

    public void Init(int id, bool isBlock, worldCreation worldCreation, PlayerInventory playerInventory)
    {
        ID = id;
        _worldCreation = worldCreation;
        
        if (isBlock)
        {
            _newMesh = new Mesh();
            _vertices = new List<Vector3>();
            _normals = new List<Vector3>();
            _uvs = new List<Vector2>();
            _indices = new List<int>();

            _worldCreation.destroyedBlocksToCreate.Add(this);
        }
        else
        {
            Debug.Log(playerInventory.Items.Count);
            var mesh = _worldCreation.voxelizer.SpriteToVoxel(playerInventory.Items[id].texture2d,
                _worldCreation.standardBlockShape, _worldCreation.blockCreation,
                new Vector3(-8f, -8f, 0));

            meshFilter.mesh = mesh;
        }
    }

    public void CreateBlock()
    {
        var currentIndex = 0;
        
        var b = _worldCreation.Blocks[ID];

        var offset = new Vector3(-.5f, -.5f, -.5f);

        _worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
            b.blockShape.faceData[2], b.GETRect(b.topIndex), 2, 1);
        _worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
            b.blockShape.faceData[5], b.GETRect(b.rightIndex), 5, 1);
        _worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
            b.blockShape.faceData[4], b.GETRect(b.leftIndex), 4, 1);
        _worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
            b.blockShape.faceData[1], b.GETRect(b.frontIndex), 1, 1);
        _worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
            b.blockShape.faceData[0], b.GETRect(b.backIndex), 0, 1);
        _worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
            b.blockShape.faceData[3], b.GETRect(b.botIndex), 3, 1);
        
        _worldCreation.destroyedBlocksToApply.Add(this);
    }

    public void ApplyBlock()
    {
        _newMesh.SetVertices(_vertices);
        _newMesh.SetNormals(_normals);
        _newMesh.SetUVs(0, _uvs);
        _newMesh.SetIndices(_indices, MeshTopology.Triangles, 0);

        _newMesh.RecalculateTangents();
        meshFilter.mesh = _newMesh;
    }
    
}

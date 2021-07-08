using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyBlock : MonoBehaviour
{
    [SerializeField] private Blocks[] destroyStagesBlocks;
    [SerializeField] private GameObject destroyBlockGameObject;
    [SerializeField] private worldCreation worldCreation;
    [SerializeField] private BlockCreation blockCreation;
    
    private bool _isDestroyingBlock = false;
    private float _destroyTime = 0f;
    private const int DestroyStates = 10;
    private int _currentDestroyState = 0;
    private int _lastDestroyState = -1;
    private Vector3Int _block;
    private int _blockId;
    private Vector3Int _lastBlock;
    
    private void Update()
    {
        if (_isDestroyingBlock)
        {
            if(_lastBlock != _block)
                ResestStates();

            destroyBlockGameObject.SetActive(true);
            
            _lastBlock = _block;
            
            _destroyTime += Time.deltaTime;
            
            if(_blockId == 0)
                return;
            
            if (_destroyTime > worldCreation.Blocks[_blockId].destroyTime)
            {
                worldCreation.DestroyBlock(_block);
                return;
            }

            _currentDestroyState = (int) (_destroyTime / worldCreation.Blocks[_blockId].destroyTime * DestroyStates);

            if (_currentDestroyState != _lastDestroyState)
            {
                GenerateMesh();
            }
            
            _lastDestroyState = _currentDestroyState;
        }
        else
        {
            ResestStates();
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void GenerateMesh()
    {
        destroyBlockGameObject.transform.position = _block;
        
        var newMesh = new Mesh();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var indices = new List<int>();
        var offset = new Vector3Int(0, 0, 0);

        var currentIndex = 0;

        var b = destroyStagesBlocks[_currentDestroyState];
        var shape = worldCreation.Blocks[_blockId].blockShape;
        
        blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
            shape.faceData[2], b.GETRect(b.topIndex), 2, 1);
        blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
            shape.faceData[5], b.GETRect(b.topIndex), 5, 1);
        blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
            shape.faceData[4], b.GETRect(b.topIndex), 4, 1);
        blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
            shape.faceData[1], b.GETRect(b.topIndex), 1, 1);
        blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
            shape.faceData[0], b.GETRect(b.topIndex), 0, 1);
        blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
            shape.faceData[3], b.GETRect(b.topIndex), 3, 1);
        
        newMesh.SetVertices(vertices);
        newMesh.SetNormals(normals);
        newMesh.SetUVs(0, uvs);
        newMesh.SetIndices(indices, MeshTopology.Triangles, 0);

        newMesh.RecalculateTangents();
        destroyBlockGameObject.GetComponent<MeshFilter>().mesh = newMesh;
    }

    private void ResestStates()
    {
        _destroyTime = 0f;
        _currentDestroyState = 0;
        _lastDestroyState = -1;
        destroyBlockGameObject.SetActive(false);
    }

    public bool IsDestroyingBlock
    {
        set => _isDestroyingBlock = value;
    }

    public Vector3Int Block
    {
        set
        {
            _block = value;
            _blockId = worldCreation.GetBlock(_block);
        }
    }
}

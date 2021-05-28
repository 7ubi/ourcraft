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

        var currentIndex = 0;

        
        blockCreation.GenerateBlock_Top(ref currentIndex, new Vector3Int(0, 0, 0), vertices, normals, uvs, indices,_currentDestroyState, 0, 1, false);
        blockCreation.GenerateBlock_Left(ref currentIndex, new Vector3Int(0, 0, 0), vertices, normals, uvs, indices,_currentDestroyState, 0, 1, false);
        blockCreation.GenerateBlock_Right(ref currentIndex, new Vector3Int(0, 0, 0), vertices, normals, uvs, indices,_currentDestroyState, 0, 1, false);
        blockCreation.GenerateBlock_Back(ref currentIndex, new Vector3Int(0, 0, 0), vertices, normals, uvs, indices,_currentDestroyState, 0, 1, false);
        blockCreation.GenerateBlock_Forward(ref currentIndex, new Vector3Int(0, 0, 0), vertices, normals, uvs, indices,_currentDestroyState, 0, 1, false);
        blockCreation.GenerateBlock_Bottom(ref currentIndex, new Vector3Int(0, 0, 0), vertices, normals, uvs, indices,_currentDestroyState, 0, 1, false);
        
        
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

    public List<Vector2> GetUVs()
    {
        return destroyStagesBlocks[_currentDestroyState].TopUVs();
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

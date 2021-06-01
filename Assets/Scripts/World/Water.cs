using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

public class Water : MonoBehaviour
{
    [SerializeField] private GameObject waterChunck;
    public worldCreation worldCreation;
    [SerializeField] private float waterAnimTime = 1f;
    private MeshFilter _meshFilter;

    [SerializeField] private BlockShape topWater;
    [SerializeField] private BlockShape normalWater;
    
    // water shader https://www.youtube.com/watch?v=gRq-IdShxpU

    private int[,,] _waterIds;
    [SerializeField] private int height;
    private Vector2 _offset;
    private Chunck _chunck;
    private GameObject _water;
    
    private Mesh _newMesh;
    private List<Vector3> _vertices;
    private List<Vector3> _normals;
    private List<Vector2> _uvs;
    private List<int> _indices;
    private Vector3 _position;
    
    public bool CanGenerateMesh { get; set; } = true;

    public void CreateWater(Vector2 offset, Transform chunckParent)
    {
        _waterIds = new int[worldCreation.Size,  worldCreation.MAXHeight,worldCreation.Size];
        _offset = offset;
        _position = transform.position;
         
        if (chunckParent.childCount > 0)
        {
            for (var i = chunckParent.childCount - 1; i >= 0; i--)
            {
                Destroy(chunckParent.GetChild(i).gameObject);
            }
        }
        
        _water = Instantiate(waterChunck, chunckParent);
        _chunck = chunckParent.GetComponent<Chunck>();
        _meshFilter = _water.GetComponent<MeshFilter>();
        
        ResetMesh();
    }

    private void ResetMesh()
    {
        _newMesh = new Mesh();
        _vertices = new List<Vector3>();
        _normals = new List<Vector3>();
        _uvs = new List<Vector2>();
        _indices = new List<int>();
    }

    public void AddWater(int x, int y, int z)
    {
        _waterIds[x, y, z] = 1;
    }

    public void UpdateWaterDown(Vector3 block)
    {
        StartCoroutine(UpdateWaterDownEnumerator(block));
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator UpdateWaterDownEnumerator(Vector3 block)
    {
        yield return new WaitForSeconds(waterAnimTime);
        
        if (worldCreation.GetBlock(block) != 0) yield break;
        
        var chunckPos = worldCreation.GetChunck(block).transform.position;
        
        var bix = Mathf.FloorToInt(block.x) - (int)chunckPos.x;
        var biy = Mathf.FloorToInt(block.y);
        var biz = Mathf.FloorToInt(block.z) - (int)chunckPos.z;
        
        _chunck.WaterIDs[bix, biy, biz] = 1;
        _waterIds[bix, biy, biz] = 1;
        worldCreation.saveManager.SaveChunck(_chunck);
        
        if (bix == 0)
        {
            if(worldCreation.GetWater(new Vector3(-1, 0, 0) + block) == 1)
                worldCreation.ReloadChunck(new Vector3(-1, 0, 0) + block);
        }
        
        if (bix == worldCreation.Size - 1){
            if(worldCreation.GetWater(new Vector3(1, 0, 0) + block) == 1)
                worldCreation.ReloadChunck(new Vector3(1, 0, 0) + block);
        }
        
        if (biz == 0)
        {
            if(worldCreation.GetWater(new Vector3(0, 0, -1) + block) == 1)
                worldCreation.ReloadChunck(new Vector3(0, 0, -1) + block);
        }
        
        if (biz == worldCreation.Size - 1)
        {
            if(worldCreation.GetWater(new Vector3(0, 0, 1) + block) == 1)
                worldCreation.ReloadChunck(new Vector3(0, 0, 1) + block);
        }
        
        var c = _chunck.GetComponent<MeshCreation>();
        c.ResetMesh();
        ResetMesh();
        c.GenerateMeshThreaded();
        StartCoroutine(UpdateWaterDownEnumerator(block + new Vector3(0, -1, 0)));
    }
    
    public void GenerateWater()
    {
        CanGenerateMesh = false;
        if (_chunck.WaterIDs != null)
            _waterIds = _chunck.WaterIDs;
        else
            _chunck.WaterIDs = _waterIds;
        var currentIndex = 0;

        for (var x = 0; x < worldCreation.Size; x++)
        {
            for(var y = 0; y < worldCreation.MAXHeight; y++)
            {
                for (var z = 0; z < worldCreation.Size; z++)
                {
                    var offset = new Vector3Int(x, y, z);
                    if (_waterIds[x, y, z] == 0) continue;

                    var shape = normalWater;
                    
                    if (y <= worldCreation.MAXHeight - 1)
                    {
                        if (_waterIds[x, y + 1, z] == 0)
                        {
                            shape = topWater;
                            
                            worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                                _normals, _uvs, _indices, shape.faceData[2]);
                            worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                                _normals, _uvs, _indices, shape.faceData[6]);
                        }
                    }
                    else
                    {
                        shape = topWater;
                        
                        worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                            _normals, _uvs, _indices, shape.faceData[2]);
                        worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                            _normals, _uvs, _indices, shape.faceData[6]);
                    }

                    if (x < worldCreation.Size - 1)
                    {
                        if (_waterIds[x + 1, y, z] == 0)
                            worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                                _normals, _uvs, _indices, shape.faceData[5]);
                    }
                    else
                    {
                        if (worldCreation.GetWater(new Vector3(x + _position.x + 1, y + _position.y,
                            z + _position.z)) == 0)
                        {
                            if (worldCreation.GetBlock(new Vector3(x + _position.x + 1, y + _position.y,
                                z + _position.z)) == 0)
                            {
                                worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                                    _normals, _uvs, _indices, shape.faceData[5]);
                            }
                            else if (worldCreation.Blocks[
                                    worldCreation.GetBlock(new Vector3(x + _position.x + 1, y + _position.y,
                                        z + _position.z))].isTransparent)
                                worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                                    _normals, _uvs, _indices, shape.faceData[5]);
                        }
                    }

                    if (x >= 1)
                    {
                        if (_waterIds[x - 1, y, z] == 0)
                            worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                                _normals, _uvs, _indices, shape.faceData[4]);
                    }
                    else
                    {
                        if (worldCreation.GetWater(new Vector3(x + _position.x - 1, y + _position.y,
                            z + _position.z)) == 0)
                        {
                            if (worldCreation.GetBlock(new Vector3(x + _position.x - 1, y + _position.y,
                                z + _position.z)) == 0)
                                worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                                    _normals, _uvs, _indices, shape.faceData[4]);
                            else if (worldCreation
                                .Blocks[
                                    worldCreation.GetBlock(new Vector3(x + _position.x - 1, y + _position.y,
                                        z + _position.z))].isTransparent)
                                worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                                    _normals, _uvs, _indices, shape.faceData[4]);
                        }
                    }

                    if (z < worldCreation.Size - 1)
                    {
                        if (_waterIds[x, y, z + 1] == 0)
                            worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                                _normals, _uvs, _indices, shape.faceData[1]);
                    }
                    else
                    {
                        if (worldCreation.GetWater(new Vector3(x + _position.x, y + _position.y,
                            z + _position.z + 1)) == 0)
                        {
                            if (worldCreation.GetBlock(new Vector3(x + _position.x, y + _position.y,
                                z + _position.z + 1)) == 0)
                                worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                                    _normals, _uvs, _indices, shape.faceData[1]);
                            else if (worldCreation
                                .Blocks[
                                    worldCreation.GetBlock(new Vector3(x + _position.x, y + _position.y,
                                        z + _position.z + 1))].isTransparent)
                                worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                                    _normals, _uvs, _indices, shape.faceData[1]);
                        }
                    }

                    if (z >= 1)
                    {
                        if (_waterIds[x, y, z - 1] == 0)
                            worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                                _normals, _uvs, _indices, shape.faceData[0]);
                    }
                    else
                    {
                        if (worldCreation.GetWater(new Vector3(x + _position.x, y + _position.y,
                            z + _position.z - 1)) == 0)
                        {
                            if (worldCreation.GetBlock(new Vector3(x + _position.x, y + _position.y,
                                z + _position.z - 1)) == 0)
                                worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                                    _normals, _uvs, _indices, shape.faceData[0]);
                            else if (worldCreation
                                .Blocks[
                                    worldCreation.GetBlock(new Vector3(x + _position.x, y + _position.y,
                                        z + _position.z - 1))].isTransparent)
                                worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                                    _normals, _uvs, _indices, shape.faceData[0]);
                        }
                    }
                    
                    if (y != 0)
                    {
                        if (_waterIds[x, y - 1, z] == 0)
                            worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, _vertices,
                                _normals, _uvs, _indices, shape.faceData[3]);
                    }
                }
            }
        }

        CanGenerateMesh = true;
        worldCreation.waterMeshesToApply.Add(this);
    }
    
    public void ApplyMesh()
    {
        _newMesh.SetVertices(_vertices);
        _newMesh.SetNormals(_normals);
        _newMesh.SetUVs(0, _uvs);
        _newMesh.SetIndices(_indices, MeshTopology.Triangles, 0);

        _newMesh.RecalculateTangents();
        _meshFilter.mesh = _newMesh;
    }

    public int Height => height;
}

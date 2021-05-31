using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

public class MeshCreation : MonoBehaviour
{
    public worldCreation worldCreation; 
    private int[,,] _blockIDs;
    private Mesh _newMesh;
    private List<Vector3> _vertices;
    private List<Vector3> _normals;
    private List<Vector2> _uvs;
    private List<int> _indices;
    private Vector3 _position;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshCollider meshCollider;
    [SerializeField] private Chunck chunck;
    private Vector2 _offset;
    
    [SerializeField] public Water water;
    public bool CanGenerateMesh { get; set; } = true;
    
    
    // ReSharper disable Unity.PerformanceAnalysis
    public void GenerateMesh()
    {
        _position = transform.position;
        _offset = new Vector2(_position.x, _position.z);
        
        ResetMesh();
        
        water.worldCreation = worldCreation;
        water.CreateWater(_offset, transform);
        
        if (chunck.BlockIDs == null)
        {
            var t = new Thread(new ThreadStart(GenerateBlocks));
            t.Start();
        }
        else
        {
            _blockIDs = chunck.BlockIDs;
        
            var t = new Thread(new ThreadStart(GenerateMeshThreaded));
            t.Start();
        }
    }

    public void ResetMesh()
    {
        _newMesh = new Mesh();
        _vertices = new List<Vector3>();
        _normals = new List<Vector3>();
        _uvs = new List<Vector2>();
        _indices = new List<int>();
        CanGenerateMesh = false;
    }

    public void GenerateMeshThreaded()
    {

        var currentIndex = 0;

        for (var x = 0; x < worldCreation.Size; x++)
        {
            for (var y = 0; y < worldCreation.MAXHeight; y++)
            {
                for (var z = 0; z < worldCreation.Size; z++)
                {
                    var offset = new Vector3Int(x, y, z);
                    if (_blockIDs[x, y, z] == 0) continue;
                    if (y < worldCreation.MAXHeight - 1)
                    {
                        if (_blockIDs[x, y + 1, z] == 0)
                            worldCreation.blockCreation.GenerateBlock_Top(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                    _blockIDs[x, y, z], 0, 1, true);
                        else if (worldCreation.Blocks[_blockIDs[x, y + 1, z]].isTransparent)
                            worldCreation.blockCreation.GenerateBlock_Top(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                    _blockIDs[x, y, z], 0, 1, true);
                    }
                    else
                    {
                        worldCreation.blockCreation.GenerateBlock_Top(ref currentIndex, offset, _vertices, _normals, _uvs, _indices, _blockIDs[x, y, z], 0, 1, true);
                    }

                    if (x < worldCreation.Size - 1)
                    {
                        if (_blockIDs[x + 1, y, z] == 0)
                            worldCreation.blockCreation.GenerateBlock_Right(ref currentIndex, offset, _vertices, _normals, _uvs, _indices, _blockIDs[x, y, z], 0, 1, true);
                        else if (worldCreation.Blocks[_blockIDs[x + 1, y, z]].isTransparent)
                            worldCreation.blockCreation.GenerateBlock_Right(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }
                    else
                    {
                        if (worldCreation.GetBlock(new Vector3(x + _position.x + 1, y + _position.y, z + _position.z)) == 0)
                            worldCreation.blockCreation.GenerateBlock_Right(ref currentIndex, offset, _vertices, _normals, _uvs, _indices, _blockIDs[x, y, z], 0, 1, true);
                        else if (worldCreation.Blocks[worldCreation.GetBlock(new Vector3(x + _position.x + 1, y + _position.y, z + _position.z))].isTransparent)
                            worldCreation.blockCreation.GenerateBlock_Right(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }

                    if (x >= 1)
                    {
                        if (_blockIDs[x - 1, y, z] == 0)
                            worldCreation.blockCreation.GenerateBlock_Left(ref currentIndex, offset, _vertices, _normals, _uvs, _indices, _blockIDs[x, y, z], 0, 1, true);
                        else if (worldCreation.Blocks[_blockIDs[x - 1, y, z]].isTransparent)
                            worldCreation.blockCreation.GenerateBlock_Left(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }
                    else
                    {
                        if (worldCreation.GetBlock(new Vector3(x + _position.x - 1, y + _position.y, z + _position.z)) == 0)
                            worldCreation.blockCreation.GenerateBlock_Left(ref currentIndex, offset, _vertices, _normals, _uvs, _indices, _blockIDs[x, y, z], 0, 1, true);
                        else if (worldCreation.Blocks[worldCreation.GetBlock(new Vector3(x + _position.x - 1, y + _position.y, z + _position.z))].isTransparent)
                            worldCreation.blockCreation.GenerateBlock_Left(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }

                    if (z < worldCreation.Size - 1)
                    {
                        if (_blockIDs[x, y, z + 1] == 0)
                            worldCreation.blockCreation.GenerateBlock_Forward(ref currentIndex, offset, _vertices, _normals, _uvs, _indices, _blockIDs[x, y, z], 0, 1, true);
                        else if (worldCreation.Blocks[_blockIDs[x, y, z + 1]].isTransparent)
                            worldCreation.blockCreation.GenerateBlock_Forward(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }
                    else
                    {
                        if (worldCreation.GetBlock(new Vector3(x + _position.x, y + _position.y, z + _position.z + 1)) == 0)
                            worldCreation.blockCreation.GenerateBlock_Forward(ref currentIndex, offset, _vertices, _normals, _uvs, _indices, _blockIDs[x, y, z], 0, 1, true);
                        else if (worldCreation.Blocks[worldCreation.GetBlock(new Vector3(x + _position.x, y + _position.y, z + _position.z + 1))].isTransparent)
                            worldCreation.blockCreation.GenerateBlock_Forward(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }

                    if (z >= 1)
                    {
                        if (_blockIDs[x, y, z - 1] == 0)
                            worldCreation.blockCreation.GenerateBlock_Back(ref currentIndex, offset, _vertices, _normals, _uvs, _indices, _blockIDs[x, y, z], 0, 1, true);
                        else if (worldCreation.Blocks[_blockIDs[x, y, z - 1]].isTransparent)
                            worldCreation.blockCreation.GenerateBlock_Back(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }
                    else
                    {
                        if (worldCreation.GetBlock(new Vector3(x + _position.x, y + _position.y, z + _position.z - 1)) == 0)
                            worldCreation.blockCreation.GenerateBlock_Back(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                _blockIDs[x, y, z], 0, 1, true);
                        else if (worldCreation.Blocks[worldCreation.GetBlock(new Vector3(x + _position.x, y + _position.y, z + _position.z - 1))].isTransparent)
                            worldCreation.blockCreation.GenerateBlock_Back(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }

                    if (y >= 1)
                    {
                        if (_blockIDs[x, y - 1, z] == 0)
                            worldCreation.blockCreation.GenerateBlock_Bottom(ref currentIndex, offset, _vertices, _normals, _uvs, _indices, _blockIDs[x, y, z], 0, 1, true);
                        else if (worldCreation.Blocks[_blockIDs[x, y - 1, z]].isTransparent)
                            worldCreation.blockCreation.GenerateBlock_Bottom(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }
                    
                }
            }
        }
        
        worldCreation.meshesToApply.Add(this);
        if (water.CanGenerateMesh)
        {
            var waterThread = new Thread(new ThreadStart(water.GenerateWater));
            waterThread.Start();
        }
        else
        {
            worldCreation.waterMeshesToUpdate.Add(water);
        }
    }
    
    private void GenerateBlocks()
    {
        _blockIDs = new int[worldCreation.Size, worldCreation.MAXHeight, worldCreation.Size];
        var treeGen = new System.Random((int)(_offset.x * 1000 + _offset.y));
        
        for (var x = 0; x < worldCreation.Size; x++)
        {
            for (var z = 0; z < worldCreation.Size; z++)
            {
                var height = worldCreation.GetHeight(x + _offset.x, z + _offset.y);

                var biome = worldCreation.biomes[worldCreation.GetBiome(x + _offset.x, z + _offset.y)];
                
                if (height <= 1)
                    height = 1;

                var grassTop = true;
                
                if (height + worldCreation.minHeight - 1 < water.Height)
                {
                    _blockIDs[x, height + worldCreation.minHeight - 1, z] = biome.secondaryBlock;
                    for (var y = water.Height; y >= height + worldCreation.minHeight; y--)
                    {
                        water.AddWater(x, y, z);
                    }
                }
                else
                {
                    if (worldCreation.Perlin3D((x + _offset.x) * 0.05f + worldCreation.Seed, height * 0.05f  + worldCreation.Seed,
                        (z + _offset.y) * 0.05f + worldCreation.Seed) >= worldCreation.noiseThreshold)
                    {
                        _blockIDs[x, height + worldCreation.minHeight, z] = biome.topBlock;
                        if (biome.hasTree)
                        {
                            if (x > 1 && x < worldCreation.Size - 2 && z > 1 && z < worldCreation.Size - 2)
                            {
                                if (treeGen.NextDouble() <= worldCreation.treeThreshold)
                                {
                                    _blockIDs[x, height + worldCreation.minHeight, z] = 1;
                                    var h = treeGen.Next(worldCreation.minTreeHeight, worldCreation.maxTreeHeight);
                                    for (var y = 1; y <= h; y++)
                                    {
                                        _blockIDs[x, height + worldCreation.minHeight + y, z] = worldCreation.blockTypes.Log;
                                        if (y <= h - 2) continue;
                                        for (var i = -1; i <= 1; i++)
                                        {
                                            for (var j = -1; j <= 1; j++)
                                            {
                                                if (i == 0 && j == 0) continue;
                                                _blockIDs[x + i, height + worldCreation.minHeight + y, z + j] = worldCreation.blockTypes.Leave;
                                            }
                                        }
                                    }

                                    _blockIDs[x, height + worldCreation.minHeight + h + 1, z] = worldCreation.blockTypes.Leave;
                                }
                            }
                        }
                    }
                    else
                    {
                        grassTop = false;
                    }
                }

                for(var y = height + worldCreation.minHeight - 1; y >= 1; y--)
                {
                    if (worldCreation.Perlin3D((x + _offset.x) * 0.05f + worldCreation.Seed, (float) y * 0.05f + worldCreation.Seed,
                        (z + _offset.y) * 0.05f + worldCreation.Seed) < worldCreation.noiseThreshold)
                    {
                        continue;
                    }

                    if (!grassTop && y > height + worldCreation.minHeight - 4)
                    {
                        _blockIDs[x, y, z] = biome.topBlock;
                        grassTop = true;
                    }
                    else
                    {
                        if (y <= height + worldCreation.minHeight - 4)
                        {
                            if (worldCreation.Perlin3D((x + worldCreation.ironNoiseOffset.x) * 0.5f + worldCreation.Seed,
                                    (y + worldCreation.ironNoiseOffset.y) * 0.5f + worldCreation.Seed, (z + worldCreation.ironNoiseOffset.z) * 0.5f + worldCreation.Seed) <
                                worldCreation.ironNoiseThreshold)
                            {
                                _blockIDs[x, y, z] = worldCreation.blockTypes.Ironore;
                            }
                            else
                                _blockIDs[x, y, z] = worldCreation.blockTypes.Stone;
                        }
                        else
                            _blockIDs[x, y, z] = biome.secondaryBlock;
                        
                    }
                }
                _blockIDs[x, 0, z] = worldCreation.blockTypes.Bedrock;
            }
        }
        chunck.BlockIDs = _blockIDs;
        
        var t2 = new Thread(new ThreadStart(GenerateMeshThreaded));
        t2.Start();
    }

    public void ApplyMesh()
    {
        CanGenerateMesh = true;
        _newMesh.SetVertices(_vertices);
        _newMesh.SetNormals(_normals);
        _newMesh.SetUVs(0, _uvs);
        _newMesh.SetIndices(_indices, MeshTopology.Triangles, 0);

        _newMesh.RecalculateTangents();
        meshFilter.mesh = _newMesh;
        meshCollider.sharedMesh = _newMesh;
    }
}

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
    private int[,,] _orienation;
    private Mesh _newMesh;
    private Mesh _colliderMesh;
    private List<Vector3> _vertices;
    private List<Vector3> _normals;
    private List<Vector2> _uvs;
    private List<int> _indices;
    private List<Vector3> _verticesCollider;
    private List<Vector3> _normalsCollider;
    private List<Vector2> _uvsCollider;
    private List<int> _indicesCollider;
    private Vector3 _position;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshCollider meshCollider;
    [SerializeField] public Chunck chunck;
    private Vector2 _offset;
    
    [SerializeField] public Water water;
    [SerializeField] public WaterGeneration waterGeneration;
    [SerializeField] private BlockShape standardBlock;
    public bool CanGenerateMesh { get; set; } = true;
    public bool InRange { get; set; } = true;

    private int _currentOrientation;
    
    // ReSharper disable Unity.PerformanceAnalysis
    public void Init()
    {
        _position = transform.position;
        _offset = new Vector2(_position.x, _position.z);
        
        ResetMesh();
        
        water.worldCreation = worldCreation;
        waterGeneration.worldCreation = worldCreation;
        water.CreateWater(_offset, transform);
        
        if (chunck.BlockIDs == null)
        {
            worldCreation.meshesToCreate.Add(this);
        }
        else
        {
            _blockIDs = chunck.BlockIDs;
            if(InRange)
                if (!worldCreation.meshesToUpdate.Contains(this)) 
                    worldCreation.meshesToUpdate.Add(this);
        }
    }

    private void ResetMesh()
    {
        _newMesh = new Mesh();
        _vertices = new List<Vector3>();
        _normals = new List<Vector3>();
        _uvs = new List<Vector2>();
        _indices = new List<int>();
        CanGenerateMesh = false;
        
        _colliderMesh = new Mesh();
        _verticesCollider = new List<Vector3>();
        _normalsCollider = new List<Vector3>();
        _uvsCollider = new List<Vector2>();
        _indicesCollider = new List<int>();
    }

    public void GenerateMesh()
    {
        var currentIndex = 0;
        var currentColliderIndex = 0;
        
        for (var x = 0; x < worldCreation.Size; x++)
        {
            for (var y = 0; y < worldCreation.MAXHeight; y++)
            {
                for (var z = 0; z < worldCreation.Size; z++)
                {
                    var offset = new Vector3Int(x, y, z);
                    if (_blockIDs[x, y, z] == 0) continue;
                    var b = worldCreation.Blocks[_blockIDs[x, y, z]];
                    _currentOrientation = chunck.Orientation[x, y, z];
                    if (b.isTransparent)
                    {
                        Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[2], b.GETRect(b.topIndex), 2);
                        Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[5], b.GETRect(b.rightIndex), 5);
                        Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[4], b.GETRect(b.leftIndex), 4);
                        Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[1], b.GETRect(b.frontIndex), 1);
                        Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[0], b.GETRect(b.backIndex), 0);
                        Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[3], b.GETRect(b.botIndex), 3);
                    }
                    
                    if (y < worldCreation.MAXHeight - 1)
                    {
                        if (_blockIDs[x, y + 1, z] == 0)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[2], b.GETRect(b.topIndex), 2);
                        else if (worldCreation.Blocks[_blockIDs[x, y + 1, z]].isTransparent)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[2], b.GETRect(b.topIndex), 2);
                    }
                    else
                    {
                        Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[2], b.GETRect(b.topIndex), 2);
                    }

                    if (x < worldCreation.Size - 1)
                    {
                        if (_blockIDs[x + 1, y, z] == 0)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[5], b.GETRect(b.rightIndex), 5);
                        else if (worldCreation.Blocks[_blockIDs[x + 1, y, z]].isTransparent)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[5], b.GETRect(b.rightIndex), 5);
                    }
                    else
                    {
                        if (worldCreation.GetBlock(new Vector3(x + _position.x + 1, y + _position.y, z + _position.z)) == 0)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[5], b.GETRect(b.rightIndex), 5);
                        else if (worldCreation.Blocks[worldCreation.GetBlock(new Vector3(x + _position.x + 1, y + _position.y, z + _position.z))].isTransparent)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[5], b.GETRect(b.rightIndex), 5);
                    }

                    if (x >= 1)
                    {
                        if (_blockIDs[x - 1, y, z] == 0)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[4], b.GETRect(b.leftIndex), 4);
                        else if (worldCreation.Blocks[_blockIDs[x - 1, y, z]].isTransparent)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[4], b.GETRect(b.leftIndex), 4);
                    }
                    else
                    {
                        if (worldCreation.GetBlock(new Vector3(x + _position.x - 1, y + _position.y, z + _position.z)) == 0)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[4], b.GETRect(b.leftIndex), 4);
                        else if (worldCreation.Blocks[worldCreation.GetBlock(new Vector3(x + _position.x - 1, y + _position.y, z + _position.z))].isTransparent)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[4], b.GETRect(b.leftIndex), 4);
                    }

                    if (z < worldCreation.Size - 1)
                    {
                        if (_blockIDs[x, y, z + 1] == 0)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[1], b.GETRect(b.frontIndex), 1);
                        else if (worldCreation.Blocks[_blockIDs[x, y, z + 1]].isTransparent)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[1], b.GETRect(b.frontIndex), 1);
                    }
                    else
                    {
                        if (worldCreation.GetBlock(new Vector3(x + _position.x, y + _position.y, z + _position.z + 1)) == 0)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[1], b.GETRect(b.frontIndex), 1);
                        else if (worldCreation.Blocks[worldCreation.GetBlock(new Vector3(x + _position.x, y + _position.y, z + _position.z + 1))].isTransparent)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[1], b.GETRect(b.frontIndex), 1);
                    }

                    if (z >= 1)
                    {
                        if (_blockIDs[x, y, z - 1] == 0)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[0], b.GETRect(b.backIndex), 0);
                        else if (worldCreation.Blocks[_blockIDs[x, y, z - 1]].isTransparent)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[0], b.GETRect(b.backIndex), 0);
                    }
                    else
                    {
                        if (worldCreation.GetBlock(new Vector3(x + _position.x, y + _position.y, z + _position.z - 1)) == 0)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[0], b.GETRect(b.backIndex), 0);
                        else if (worldCreation.Blocks[worldCreation.GetBlock(
                            new Vector3(x + _position.x, y + _position.y, z + _position.z - 1))].isTransparent)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[0], b.GETRect(b.backIndex), 0);
                    }

                    if (y >= 1)
                    {
                        if (_blockIDs[x, y - 1, z] == 0)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[3], b.GETRect(b.botIndex), 3);
                        else if (worldCreation.Blocks[_blockIDs[x, y - 1, z]].isTransparent)
                            Block(ref currentIndex, ref currentColliderIndex, offset, b.blockShape.faceData[3], b.GETRect(b.botIndex), 3);
                    }
                }
            }
        }

        if (!worldCreation.meshesToApply.Contains(this))
            worldCreation.meshesToApply.Add(this);
        
        
        if(!worldCreation.waterMeshesToUpdate.Contains(waterGeneration))
            worldCreation.waterMeshesToUpdate.Add(waterGeneration);
    }
    
    public void GenerateBlocks()
    {
        _blockIDs = new int[worldCreation.Size, worldCreation.MAXHeight, worldCreation.Size];
        _orienation = new int[worldCreation.Size, worldCreation.MAXHeight, worldCreation.Size];
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
                
                if (height + worldCreation.minHeight < water.Height)
                {
                    _blockIDs[x, height + worldCreation.minHeight, z] = biome.secondaryBlock;
                    _orienation[x, height + worldCreation.minHeight, z] = 1;
                    for (var y = water.Height; y >= height + worldCreation.minHeight + 1; y--)
                    {
                        water.AddWater(x, y, z);
                        _orienation[x, y, z] = 1;
                    }
                }
                else
                {
                    if (Noise.Perlin3D((x + _offset.x) * 0.05f + worldCreation.Seed, height * 0.05f  + worldCreation.Seed,
                        (z + _offset.y) * 0.05f + worldCreation.Seed) >= worldCreation.noiseThreshold)
                    {
                        _blockIDs[x, height + worldCreation.minHeight, z] = biome.topBlock;
                        _orienation[x, height + worldCreation.minHeight, z] = 1;
                        if (biome.hasTree || biome.hasCactus)
                        {
                            if (x > 1 && x < worldCreation.Size - 2 && z > 1 && z < worldCreation.Size - 2)
                            {
                                if (treeGen.NextDouble() <= biome.vegetationThreshold)
                                {
                                    if (biome.hasTree)
                                    {
                                        _blockIDs[x, height + worldCreation.minHeight, z] = 1;
                                        var h = treeGen.Next(biome.minVegetationHeight, biome.maxVegetationHeight);
                                        for (var y = 1; y <= h; y++)
                                        {
                                            _blockIDs[x, height + worldCreation.minHeight + y, z] =
                                                BlockTypes.Log;
                                            _orienation[x, height + worldCreation.minHeight + y, z] = 1;
                                            if (y <= h - 2) continue;
                                            for (var i = -1; i <= 1; i++)
                                            {
                                                for (var j = -1; j <= 1; j++)
                                                {
                                                    if (i == 0 && j == 0) continue;
                                                    _blockIDs[x + i, height + worldCreation.minHeight + y, z + j] =
                                                        BlockTypes.Leave;
                                                    _orienation[x + i, height + worldCreation.minHeight + y, z + j] = 1;
                                                }
                                            }
                                        }

                                        _blockIDs[x, height + worldCreation.minHeight + h + 1, z] =
                                            BlockTypes.Leave;
                                        _orienation[x, height + worldCreation.minHeight + h + 1, z] = 1;
                                    }
                                    else if (biome.hasCactus)
                                    {
                                        var h = treeGen.Next(biome.minVegetationHeight, biome.maxVegetationHeight);;
                                        for (var y = 1; y <= h; y++)
                                        {
                                            _blockIDs[x, height + worldCreation.minHeight + y, z] =
                                                BlockTypes.Cactus;
                                            _orienation[x, height + worldCreation.minHeight + y, z] = 1;
                                        }
                                    }
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
                    if (Noise.Perlin3D((x + _offset.x) * 0.05f + worldCreation.Seed, (float) y * 0.05f + worldCreation.Seed,
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
                        grassTop = true;
                        if (y <= height + worldCreation.minHeight - 4)
                        {
                            if (Noise.Perlin3D((x + worldCreation.coalNoiseOffset.x) * 0.5f + worldCreation.Seed,
                                    (y + worldCreation.coalNoiseOffset.y) * 0.5f + worldCreation.Seed, (z + worldCreation.coalNoiseOffset.z) * 0.5f + worldCreation.Seed) <
                                worldCreation.coalNoiseThreshold)
                            {
                                _blockIDs[x, y, z] = BlockTypes.Coalore;
                            }
                            else if (Noise.Perlin3D((x + worldCreation.ironNoiseOffset.x) * 0.5f + worldCreation.Seed,
                                    (y + worldCreation.ironNoiseOffset.y) * 0.5f + worldCreation.Seed, (z + worldCreation.ironNoiseOffset.z) * 0.5f + worldCreation.Seed) <
                                worldCreation.ironNoiseThreshold)
                            {
                                _blockIDs[x, y, z] = BlockTypes.Ironore;
                            }
                            else
                                _blockIDs[x, y, z] = BlockTypes.Stone;
                        }
                        else
                            _blockIDs[x, y, z] = biome.secondaryBlock;
                        
                    }

                    _orienation[x, y, z] = 1;
                }
                _blockIDs[x, 0, z] = BlockTypes.Bedrock;
                _orienation[x, 0, z] = 1;
            }
        }
        chunck.BlockIDs = _blockIDs;
        chunck.Orientation = _orienation;
        
        if (!worldCreation.meshesToUpdate.Contains(this))
            worldCreation.meshesToUpdate.Add(this);
    }

    private void Block(ref int currentIndex, ref int currentColliderIndex, Vector3Int offset, FaceData face, Vector2 rect, int faceIndex)
    {
        worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
            face, rect, faceIndex, _currentOrientation);
        worldCreation.blockCreation.GenerateBlock(ref currentColliderIndex, offset, _verticesCollider,
            _normalsCollider, _uvsCollider, _indicesCollider, standardBlock.faceData[faceIndex], rect,
            faceIndex, _currentOrientation);
    }

    public void ApplyMesh()
    {
        _newMesh.SetVertices(_vertices);
         _newMesh.SetNormals(_normals);
        _newMesh.SetUVs(0, _uvs);
        _newMesh.SetIndices(_indices, MeshTopology.Triangles, 0);

        _colliderMesh.SetVertices(_verticesCollider);
        _colliderMesh.SetNormals(_normalsCollider);
        _colliderMesh.SetUVs(0, _uvsCollider);
        _colliderMesh.SetIndices(_indicesCollider, MeshTopology.Triangles, 0);
        
        _newMesh.RecalculateTangents();
        meshFilter.mesh = _newMesh;
        meshCollider.sharedMesh = _colliderMesh;
        CanGenerateMesh = true;
    }
}

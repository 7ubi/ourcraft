using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class worldCreation : MonoBehaviour
{
    //base code: https://github.com/Absurdponcho/BlockGame/blob/master/Assets/VoxelChunk.cs
    
    [Header("Chunck Info")]
    [SerializeField] private int size = 16;
    [SerializeField] private int maxHeight;
    [SerializeField] private int minHeight;
    private int[,,] _blockIDs;
    [SerializeField] private float noiseThreshold;
    [SerializeField] private int seed;
    [SerializeField] private GameObject chunckGameObject;
    [SerializeField] private GameObject destroyedBlock;
    [SerializeField] private float renderDistance;
    
    [Header("Tree")]
    [SerializeField] private float treeThreshold;
    [SerializeField] private int minTreeHeight;
    [SerializeField] private int maxTreeHeight;

    [Header("Ores")]
    [SerializeField] private Vector3 ironNoiseOffset;
    [SerializeField] private float ironNoiseThreshold;
    
    [Header("Player")]
    [SerializeField] private GameObject player;
    private PlayerInventory _playerInventory;
    
    [Header("Scripts")]
    [SerializeField] private BlockTypes blockTypes;
    [SerializeField] private BlockCreation blockCreation;
    [SerializeField] private Water water;
    
    
    [Header("Blocks and Biomes")]
    [SerializeField] private Blocks[] blocks;
    [SerializeField] private Biome[] biomes;
    private Dictionary<Vector2, GameObject> _chuncks = new Dictionary<Vector2, GameObject>();
    private int _lastChunck = 0;

    private void Start()
    {
        _playerInventory = player.GetComponent<PlayerInventory>();

        foreach (var b in blocks)
        {
            Blocks.Add(b.id, b);    
        }
    }

    public void GenerateFirst()
    {
        seed = Random.Range(10000, 100000);
        StartCoroutine(GenerateChunck(true));
    }
    
    private void Update()
    {
        var position = Vector3Int.FloorToInt(player.transform.position);
        var currentChunck = (position.x - (position.x % 8) + position.z - (position.z % 8));
        if (currentChunck != _lastChunck)
        {
            StartCoroutine(GenerateChunck(false));
        }

        _lastChunck = currentChunck;
    }

    private void GenerateBlocks(Vector2 offset, Transform chunck)
    {
        _blockIDs = new int[size, maxHeight, size];
        var treeGen = new System.Random((int)(offset.x * 1000 + offset.y));
        water.CreateWater(offset);
        for (var x = 0; x < size; x++)
        {
            for (var z = 0; z < size; z++)
            {
                var height = GetHeight(x + offset.x, z + offset.y);

                var biome = biomes[GetBiome(x + offset.x, z + offset.y)];
                
                if (height <= 1)
                    height = 1;

                var grassTop = true;
                var waterTop = false;
                if (height + minHeight - 1 < water.Height)
                {
                    _blockIDs[x, height + minHeight - 4, z] = biome.secondaryBlock;
                    for (var y = water.Height; y >= height + minHeight; y--)
                    {
                        water.AddWater(x, y, z);
                    }

                    waterTop = true;
                }
                else
                {
                    if (Perlin3D((x + offset.x) * 0.05f + seed, (float) (height) * 0.05f  + seed,
                        (z + offset.y) * 0.05f + seed) >= noiseThreshold)
                    {
                        _blockIDs[x, height + minHeight, z] = biome.topBlock;
                        if (biome.hasTree)
                        {
                            if (x > 1 && x < size - 2 && z > 1 && z < size - 2)
                            {
                                if (treeGen.NextDouble() <= treeThreshold)
                                {
                                    _blockIDs[x, height + minHeight, z] = 1;
                                    var h = treeGen.Next(minTreeHeight, maxTreeHeight);
                                    for (var y = 1; y <= h; y++)
                                    {
                                        _blockIDs[x, height + minHeight + y, z] = blockTypes.Log;
                                        if (y <= h - 2) continue;
                                        for (var i = -1; i <= 1; i++)
                                        {
                                            for (var j = -1; j <= 1; j++)
                                            {
                                                if (i == 0 && j == 0) continue;
                                                _blockIDs[x + i, height + minHeight + y, z + j] = blockTypes.Leave;
                                            }
                                        }
                                    }

                                    _blockIDs[x, height + minHeight + h + 1, z] = blockTypes.Leave;
                                }
                            }
                        }
                    }
                    else
                    {
                        grassTop = false;
                    }
                }

                for(var y = height + minHeight - 1; y >= 1; y--)
                {
                    if (Perlin3D((x + offset.x) * 0.05f + seed, (float) y * 0.05f + seed,
                        (z + offset.y) * 0.05f + seed) < noiseThreshold)
                    {
                        if (waterTop)
                        {
                            water.AddWater(x, y, z);
                        }
                        continue;
                    }

                    
                    if (!grassTop && y > height + minHeight - 4)
                    {
                        _blockIDs[x, y, z] = biome.topBlock;
                        grassTop = true;
                    }
                    else
                    {
                        if (y <= height + minHeight - 4)
                        {
                            if (Perlin3D((x + ironNoiseOffset.x) * 0.5f + seed,
                                    (y + ironNoiseOffset.y) * 0.5f + seed, (z + ironNoiseOffset.z) * 0.5f + seed) <
                                ironNoiseThreshold)
                            {
                                _blockIDs[x, y, z] = blockTypes.Ironore;
                            }
                            else
                                _blockIDs[x, y, z] = blockTypes.Stone;
                        }
                        else
                            _blockIDs[x, y, z] = biome.secondaryBlock;
                        
                        waterTop = false;
                    }
                }
                _blockIDs[x, 0, z] = blockTypes.Bedrock;
            }
        }
    }

    private IEnumerator GenerateChunck(bool firstGeneration)
    {
        var position = player.transform.position;
        var playerX = position.x - (position.x % 16);
        var playerZ = position.z - (position.z % 16);
        
        var minX = Convert.ToInt32(playerX - size * renderDistance);
        var maxX = Convert.ToInt32(playerX + size * renderDistance);
        var minZ = Convert.ToInt32(playerZ - size * renderDistance);
        var maxZ = Convert.ToInt32(playerZ + size * renderDistance);

        for (var x = minX; x <= maxX; x += size)
        {
            for (var z = minZ; z <= maxZ; z += size)
            {
                var c = GetChunck(new Vector3(x, 0, z));

                if (!c)
                {
                    var newChunck = Instantiate(chunckGameObject, new Vector3(x, 0, z), Quaternion.identity);
                    GenerateMesh(newChunck);
                    Chuncks.Add(newChunck);
                    _chuncks.Add(new Vector2(newChunck.transform.position.x, newChunck.transform.position.z), newChunck);
                    if(!firstGeneration)
                        yield return new WaitForSeconds(.1f);
                }
                else
                {
                    c.SetActive(true);
                }
            }
        }
        
        if (Chuncks.Count < 0) yield break;
        {
            for (var i = Chuncks.Count - 1; i >= 0; i--)
            {
                var chunck = Chuncks[i];
                if (!(chunck.transform.position.x < minX) && !(chunck.transform.position.x > maxX) &&
                    !(chunck.transform.position.z < minZ) && !(chunck.transform.position.z > maxZ)) continue;
                
                chunck.SetActive(false);
            }
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void DestroyBlock(Vector3 block)
    {

        var chunck = GetChunck(block);
        
        var bix = Mathf.FloorToInt(block.x) - (int)chunck.transform.position.x;
        var biy = Mathf.FloorToInt(block.y);
        var biz = Mathf.FloorToInt(block.z) - (int)chunck.transform.position.z;
        var c = chunck.GetComponent<Chunck>();
        
        _playerInventory.AddDestroyedBlock(CreateDestroyedBlock(c.BlockIDs[bix, biy, biz], block + new Vector3(0.375f, 0.1f, 0.375f)));
        
        c.BlockIDs[bix, biy, biz] = 0;
        
        GenerateMesh(chunck);

        if (bix == 0)
        {
            if(GetBlock(new Vector3(-1, 0, 0) + block) != 0)
                ReloadChunck(new Vector3(-1, 0, 0) + block);
        }
        
        if (bix == size - 1){
            if(GetBlock(new Vector3(1, 0, 0) + block) != 0)
                ReloadChunck(new Vector3(1, 0, 0) + block);
        }
        
        if (biz == 0)
        {
            if(GetBlock(new Vector3(0, 0, -1) + block) != 0)
                ReloadChunck(new Vector3(0, 0, -1) + block);
        }
        
        if (biz == size - 1)
        {
            if(GetBlock(new Vector3(0, 0, 1) + block) != 0)
                ReloadChunck(new Vector3(0, 0, 1) + block);
        }
    }

    public void PlaceBlock(Vector3 block, int id)
    {
        var chunck = GetChunck(block);
        
        var bix = Mathf.FloorToInt(block.x) - (int)chunck.transform.position.x;
        var biy = Mathf.FloorToInt(block.y);
        var biz = Mathf.FloorToInt(block.z) - (int)chunck.transform.position.z;
        
        var c = chunck.GetComponent<Chunck>();
        
        c.BlockIDs[bix, biy, biz] = id;
        c.WaterIDs[bix, biy, biz] = 0;
        
        water.GenerateWater(chunck.transform);
        GenerateMesh(chunck);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void GenerateMesh(GameObject chunck)
    {
        var position = chunck.transform.position;
        var c = chunck.GetComponent<Chunck>();
        if (c.BlockIDs == null)
        {
            GenerateBlocks(new Vector2(position.x, position.z), chunck.transform);
            chunck.GetComponent<Chunck>().BlockIDs = _blockIDs;
        }
        else
        {
            _blockIDs = c.BlockIDs;
        }
        
        water.GenerateWater(chunck.transform);
        
        var newMesh = new Mesh();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var indices = new List<int>();

        var currentIndex = 0;

        for (var x = 0; x < size; x++)
        {
            for (var y = 0; y < maxHeight; y++)
            {
                for (var z = 0; z < size; z++)
                {
                    var offset = new Vector3Int(x, y, z);
                    if (_blockIDs[x, y, z] == 0) continue;
                    if (y < maxHeight - 1)
                    {
                        if (_blockIDs[x, y + 1, z] == 0)
                            blockCreation.GenerateBlock_Top(ref currentIndex, offset, vertices, normals, uvs, indices,
                                    _blockIDs[x, y, z], 0, 1, true);
                        else if (Blocks[_blockIDs[x, y + 1, z]].isTransparent)
                            blockCreation.GenerateBlock_Top(ref currentIndex, offset, vertices, normals, uvs, indices,
                                    _blockIDs[x, y, z], 0, 1, true);
                    }
                    else
                    {
                        blockCreation.GenerateBlock_Top(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z], 0, 1, true);
                    }

                    if (x < size - 1)
                    {
                        if (_blockIDs[x + 1, y, z] == 0)
                            blockCreation.GenerateBlock_Right(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z], 0, 1, true);
                        else if (Blocks[_blockIDs[x + 1, y, z]].isTransparent)
                            blockCreation.GenerateBlock_Right(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }
                    else
                    {
                        if (GetBlock(new Vector3(x + position.x + 1, y + position.y, z + position.z)) == 0)
                            blockCreation.GenerateBlock_Right(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z], 0, 1, true);
                        else if (Blocks[GetBlock(new Vector3(x + position.x + 1, y + position.y, z + position.z))].isTransparent)
                            blockCreation.GenerateBlock_Right(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }

                    if (x >= 1)
                    {
                        if (_blockIDs[x - 1, y, z] == 0)
                            blockCreation.GenerateBlock_Left(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z], 0, 1, true);
                        else if (Blocks[_blockIDs[x - 1, y, z]].isTransparent)
                            blockCreation.GenerateBlock_Left(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }
                    else
                    {
                        if (GetBlock(new Vector3(x + position.x - 1, y + position.y, z + position.z)) == 0)
                            blockCreation.GenerateBlock_Left(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z], 0, 1, true);
                        else if (Blocks[GetBlock(new Vector3(x + position.x - 1, y + position.y, z + position.z))].isTransparent)
                            blockCreation.GenerateBlock_Left(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }

                    if (z < size - 1)
                    {
                        if (_blockIDs[x, y, z + 1] == 0)
                            blockCreation.GenerateBlock_Forward(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z], 0, 1, true);
                        else if (Blocks[_blockIDs[x, y, z + 1]].isTransparent)
                            blockCreation.GenerateBlock_Forward(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }
                    else
                    {
                        if (GetBlock(new Vector3(x + position.x, y + position.y, z + position.z + 1)) == 0)
                            blockCreation.GenerateBlock_Forward(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z], 0, 1, true);
                        else if (Blocks[GetBlock(new Vector3(x + position.x, y + position.y, z + position.z + 1))].isTransparent)
                            blockCreation.GenerateBlock_Forward(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }

                    if (z >= 1)
                    {
                        if (_blockIDs[x, y, z - 1] == 0)
                            blockCreation.GenerateBlock_Back(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z], 0, 1, true);
                        else if (Blocks[_blockIDs[x, y, z - 1]].isTransparent)
                            blockCreation.GenerateBlock_Back(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }
                    else
                    {
                        if (GetBlock(new Vector3(x + position.x, y + position.y, z + position.z - 1)) == 0)
                            blockCreation.GenerateBlock_Back(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z], 0, 1, true);
                        else if (Blocks[GetBlock(new Vector3(x + position.x, y + position.y, z + position.z - 1))].isTransparent)
                            blockCreation.GenerateBlock_Back(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z], 0, 1, true);
                    }

                    if (y >= 1)
                    {
                        if (_blockIDs[x, y - 1, z] == 0)
                            blockCreation.GenerateBlock_Bottom(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z], 0, 1, true);
                        else if (Blocks[_blockIDs[x, y - 1, z]].isTransparent)
                            blockCreation.GenerateBlock_Bottom(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z], 0, 1, true);
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
    }

   

    private float Perlin3D(float x, float y, float z)
    {
        var ab = Mathf.PerlinNoise(x, y);
        var bc = Mathf.PerlinNoise(y, z);
        var ac = Mathf.PerlinNoise(x, z);
        
        var ba = Mathf.PerlinNoise(y, x);
        var cb = Mathf.PerlinNoise(z, y);
        var ca = Mathf.PerlinNoise(z, x);

        var abc = ab + bc + ac + ba + cb + ca;
        
        return abc / 6f;
    }

    public void LoadData(int seed, List<ChunckInfo> chunckInfos)
    {
        this.seed = seed;
        
        if(chunckInfos.Count == 0) return;
        
        foreach (var chunckInfo in chunckInfos)
        {
            var c = Instantiate(chunckGameObject, chunckInfo.pos, Quaternion.identity);

            var bIds = new int[size, maxHeight, size];

            for (var i = 0; i < chunckInfo.blockIDs.Length; i++)
            {
                var z = i % size;
                var y = (i / size) % maxHeight;
                var x = i / (size * maxHeight);

                bIds[x, y, z] = chunckInfo.blockIDs[i];
            }
            var cChunck = c.GetComponent<Chunck>();
            cChunck.BlockIDs = bIds;
            if (chunckInfo.waterIDs != null)
            {
                var wIds = new int[size, maxHeight, size];
                for (var i = 0; i < chunckInfo.waterIDs.Length; i++)
                {
                    var z = i % size;
                    var y = (i / size) % maxHeight;
                    var x = i / (size * maxHeight);

                    wIds[x, y, z] = chunckInfo.waterIDs[i];
                }
                cChunck.WaterIDs = wIds;
            }
        
            Chuncks.Add(c);
            _chuncks.Add(new Vector2(c.transform.position.x, c.transform.position.z), c);
            
            GenerateMesh(c);
        }
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    public int GetBlock(Vector3 block)
    {
        var chunkPosX = Mathf.FloorToInt(block.x / 16f) * 16;
        var chunkPosZ = Mathf.FloorToInt(block.z / 16f) * 16;
        var chunck = GetChunck(block);
        if (chunck != null)
        {
            var bix = Mathf.FloorToInt(block.x) - chunkPosX;
            var biy = Mathf.FloorToInt(block.y);
            var biz = Mathf.FloorToInt(block.z) - chunkPosZ;
            var c = chunck.GetComponent<Chunck>();
            return c.BlockIDs[bix, biy, biz];
        }

        var height = GetHeight(block.x, block.z);
        
        if (block.y >= height + minHeight)
            return 0;

        return Perlin3D((block.x) * 0.05f + seed, (float) (block.y) * 0.05f + seed,
            (block.z) * 0.05f + seed) >= noiseThreshold
            ? 1
            : 0;
    }

    private void ReloadChunck(Vector3 block)
    {
        GenerateMesh(GetChunck(block));
    }

    private GameObject GetChunck(Vector3 block)
    {
        var chunkPosX = Mathf.FloorToInt(block.x / 16f) * 16;
        var chunkPosZ = Mathf.FloorToInt(block.z / 16f) * 16;

        try
        {
            return _chuncks[new Vector2(chunkPosX, chunkPosZ)];
        }
        catch
        {
            return null;
        }
    }

    private int GetBiome(float x, float z)
    {
        var strongestWeight = 0f;
        var index = 0;
        
        for (var i = 0; i < biomes.Length; i++)
        {
            var weight = Mathf.PerlinNoise((x + biomes[i].offset.x) * 0.005f + seed,
                (z + biomes[i].offset.y) * 0.005f + seed);

            if (!(weight > strongestWeight)) continue;
            strongestWeight = weight;
            index = i;
        }
                       
        return index;
    }

    private int GetHeight(float x, float z)
    {
        var height = 0;
        var count = 0;
        
        for (var i = 0; i < biomes.Length; i++)
        {
            var weight = Mathf.PerlinNoise((x + biomes[i].offset.x) * 0.005f + seed,
                (z + biomes[i].offset.y) * 0.005f + seed);
            
            var height0 = Mathf.PerlinNoise(x * biomes[i].noiseMult1 + seed, z * biomes[i].noiseMult1 + seed);
            var height1 = Mathf.PerlinNoise(x * biomes[i].noiseMult2 + seed, z * biomes[i].noiseMult2 + seed);
            var h = (int) ((height0 + height1) * biomes[i].maxGeneratingHeight * weight);

            if (h < 0) continue;
            height += h;
            count++;
        }

        height /= count;
        return height;
    }

    private GameObject CreateDestroyedBlock(int id, Vector3 pos)
    {
        var b = Instantiate(destroyedBlock, pos, Quaternion.identity);
        b.GetComponent<DestroyedBlock>().ID = id;

        var newMesh = new Mesh();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var indices = new List<int>();

        var currentIndex = 0;

        
        blockCreation.GenerateBlock_Top(ref currentIndex, new Vector3Int(0, 0, 0), vertices, normals, uvs, indices, id, 0, 0.3f, true);
        blockCreation.GenerateBlock_Left(ref currentIndex, new Vector3Int(0, 0, 0), vertices, normals, uvs, indices, id, 0, 0.3f, true);
        blockCreation.GenerateBlock_Right(ref currentIndex, new Vector3Int(0, 0, 0), vertices, normals, uvs, indices, id, 0, 0.3f, true);
        blockCreation.GenerateBlock_Back(ref currentIndex, new Vector3Int(0, 0, 0), vertices, normals, uvs, indices, id, 0, 0.3f, true);
        blockCreation.GenerateBlock_Forward(ref currentIndex, new Vector3Int(0, 0, 0), vertices, normals, uvs, indices, id, 0, 0.3f, true);
        blockCreation.GenerateBlock_Bottom(ref currentIndex, new Vector3Int(0, 0, 0), vertices, normals, uvs, indices, id, 0, 0.3f, true);
        
        
        newMesh.SetVertices(vertices);
        newMesh.SetNormals(normals);
        newMesh.SetUVs(0, uvs);
        newMesh.SetIndices(indices, MeshTopology.Triangles, 0);

        newMesh.RecalculateTangents();
        b.GetComponent<MeshFilter>().mesh = newMesh;
        b.GetComponent<MeshCollider>().sharedMesh = newMesh;
        
        return b;
    }
    
    public int Seed
    {
        get => seed;
        set => seed = value;
    }

    public List<GameObject> Chuncks { get; set; } = new List<GameObject>();

    public int MAXHeight => maxHeight;

    public int Size => size;

    public Dictionary<int, Blocks> Blocks { get; } = new Dictionary<int, Blocks>();
}


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
    [SerializeField] int Size = 16;
    [SerializeField] private int maxHeight;
    private int[,,] _blockIDs;
    [SerializeField] private float noiseThreshold;
    
    [SerializeField] private float treeThreshold;
    [SerializeField] private int minTreeHeight;
    [SerializeField] private int maxTreeHeight;
    [SerializeField] private float biomeNoise;
    private Vector2 biomeNoiseOffset;
    
    [SerializeField] private int seed;
    [FormerlySerializedAs("chunck")] [SerializeField] private GameObject chunckGameObject;

    [SerializeField] private float renderDistance;
    [SerializeField] private GameObject player;
    private PlayerInventory _playerInventory;
    [SerializeField] private BlockTypes _blockTypes;
    [SerializeField] private Water water;
    private int _lastChunck = 0;
    private Dictionary<Vector2, GameObject> _chuncks = new Dictionary<Vector2, GameObject>();

    [SerializeField] private Blocks[] blocks;
    [SerializeField] private Biome[] biomes;

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
        _blockIDs = new int[Size, maxHeight, Size];
        var treeGen = new System.Random((int)(offset.x * 1000 + offset.y));
        water.CreateWater(offset);
        for (var x = 0; x < Size; x++)
        {
            for (var z = 0; z < Size; z++)
            {
                var height = GetHeight(x + offset.x, z + offset.y);

                var biome = biomes[GetBiome(x + offset.x, z + offset.y)];
                
                //Debug.Log(GetBiome(x + offset.x, z + offset.y));
                
                if (height <= 1)
                    height = 1;

                var grassTop = true;
                var waterTop = false;
                if (height + biome.minHeight - 1 < water.Height)
                {
                    _blockIDs[x, height + biome.minHeight - 4, z] = biome.secondaryBlock;
                    for (var y = water.Height; y >= height + biome.minHeight; y--)
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
                        _blockIDs[x, height + biome.minHeight, z] = biome.topBlock;
                        if (biome.hasTree)
                        {
                            if (x > 1 && x < Size - 2 && z > 1 && z < Size - 2)
                            {
                                if (treeGen.NextDouble() <= treeThreshold)
                                {
                                    _blockIDs[x, height + biome.minHeight, z] = 1;
                                    var h = treeGen.Next(minTreeHeight, maxTreeHeight);
                                    for (var y = 1; y <= h; y++)
                                    {
                                        _blockIDs[x, height + biome.minHeight + y, z] = _blockTypes.Log;
                                        if (y <= h - 2) continue;
                                        for (var i = -1; i <= 1; i++)
                                        {
                                            for (var j = -1; j <= 1; j++)
                                            {
                                                if (i == 0 && j == 0) continue;
                                                _blockIDs[x + i, height + biome.minHeight + y, z + j] = _blockTypes.Leave;
                                            }
                                        }
                                    }

                                    _blockIDs[x, height + biome.minHeight + h + 1, z] = _blockTypes.Leave;
                                }
                            }
                        }
                    }
                    else
                    {
                        grassTop = false;
                    }
                }

                for(var y = height + biome.minHeight - 1; y >= 1; y--)
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

                    
                    if (!grassTop && y > height + biome.minHeight - 4)
                    {
                        _blockIDs[x, y, z] = biome.topBlock;
                        grassTop = true;
                    }
                    else
                    {
                        if (y <= height + biome.minHeight - 4)
                            _blockIDs[x, y, z] = _blockTypes.Stone;
                        else
                            _blockIDs[x, y, z] = biome.secondaryBlock;
                        
                        waterTop = false;
                    }
                }
                _blockIDs[x, 0, z] = _blockTypes.Stone;
            }
        }
    }

    private IEnumerator GenerateChunck(bool firstGeneration)
    {
        var position = player.transform.position;
        var playerX = position.x - (position.x % 16);
        var playerZ = position.z - (position.z % 16);
        
        var minX = Convert.ToInt32(playerX - Size * renderDistance);
        var maxX = Convert.ToInt32(playerX + Size * renderDistance);
        var minZ = Convert.ToInt32(playerZ - Size * renderDistance);
        var maxZ = Convert.ToInt32(playerZ + Size * renderDistance);

        for (var x = minX; x <= maxX; x += Size)
        {
            for (var z = minZ; z <= maxZ; z += Size)
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
        _playerInventory.AddItem(c.BlockIDs[bix, biy, biz], 1);
        c.BlockIDs[bix, biy, biz] = 0;
        
        GenerateMesh(chunck);

        if (bix == 0)
        {
            if(GetBlock(new Vector3(-1, 0, 0) + block) != 0)
                ReloadChunck(new Vector3(-1, 0, 0) + block);
        }
        
        if (bix == Size - 1){
            if(GetBlock(new Vector3(1, 0, 0) + block) != 0)
                ReloadChunck(new Vector3(1, 0, 0) + block);
        }
        
        if (biz == 0)
        {
            if(GetBlock(new Vector3(0, 0, -1) + block) != 0)
                ReloadChunck(new Vector3(0, 0, -1) + block);
        }
        
        if (biz == Size - 1)
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

        for (var x = 0; x < Size; x++)
        {
            for (var y = 0; y < maxHeight; y++)
            {
                for (var z = 0; z < Size; z++)
                {
                    var offset = new Vector3Int(x, y, z);
                    if (_blockIDs[x, y, z] == 0) continue;
                    if (y < maxHeight - 1)
                    {
                        if (_blockIDs[x, y + 1, z] == 0)
                            GenerateBlock_Top(ref currentIndex, offset, vertices, normals, uvs, indices,
                                    _blockIDs[x, y, z]);
                        else if (Blocks[_blockIDs[x, y + 1, z]].isTransparent)
                            GenerateBlock_Top(ref currentIndex, offset, vertices, normals, uvs, indices,
                                    _blockIDs[x, y, z]);
                    }
                    else
                    {
                        GenerateBlock_Top(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z]);
                    }

                    if (x < Size - 1)
                    {
                        if (_blockIDs[x + 1, y, z] == 0)
                            GenerateBlock_Right(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z]);
                        else if (Blocks[_blockIDs[x + 1, y, z]].isTransparent)
                            GenerateBlock_Right(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z]);
                    }
                    else
                    {
                        if (GetBlock(new Vector3(x + position.x + 1, y + position.y, z + position.z)) == 0)
                            GenerateBlock_Right(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z]);
                        else if (Blocks[GetBlock(new Vector3(x + position.x + 1, y + position.y, z + position.z))].isTransparent)
                            GenerateBlock_Right(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z]);
                    }

                    if (x >= 1)
                    {
                        if (_blockIDs[x - 1, y, z] == 0)
                            GenerateBlock_Left(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z]);
                        else if (Blocks[_blockIDs[x - 1, y, z]].isTransparent)
                            GenerateBlock_Left(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z]);
                    }
                    else
                    {
                        if (GetBlock(new Vector3(x + position.x - 1, y + position.y, z + position.z)) == 0)
                            GenerateBlock_Left(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z]);
                        else if (Blocks[GetBlock(new Vector3(x + position.x - 1, y + position.y, z + position.z))].isTransparent)
                            GenerateBlock_Left(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z]);
                    }

                    if (z < Size - 1)
                    {
                        if (_blockIDs[x, y, z + 1] == 0)
                            GenerateBlock_Forward(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z]);
                        else if (Blocks[_blockIDs[x, y, z + 1]].isTransparent)
                            GenerateBlock_Forward(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z]);
                    }
                    else
                    {
                        if (GetBlock(new Vector3(x + position.x, y + position.y, z + position.z + 1)) == 0)
                            GenerateBlock_Forward(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z]);
                        else if (Blocks[GetBlock(new Vector3(x + position.x, y + position.y, z + position.z + 1))].isTransparent)
                            GenerateBlock_Forward(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z]);
                    }

                    if (z >= 1)
                    {
                        if (_blockIDs[x, y, z - 1] == 0)
                            GenerateBlock_Back(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z]);
                        else if (Blocks[_blockIDs[x, y, z - 1]].isTransparent)
                            GenerateBlock_Back(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z]);
                    }
                    else
                    {
                        if (GetBlock(new Vector3(x + position.x, y + position.y, z + position.z - 1)) == 0)
                            GenerateBlock_Back(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z]);
                        else if (Blocks[GetBlock(new Vector3(x + position.x, y + position.y, z + position.z - 1))].isTransparent)
                            GenerateBlock_Back(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z]);
                    }

                    if (y >= 1)
                    {
                        if (_blockIDs[x, y - 1, z] == 0)
                            GenerateBlock_Bottom(ref currentIndex, offset, vertices, normals, uvs, indices, _blockIDs[x, y, z]);
                        else if (Blocks[_blockIDs[x, y - 1, z]].isTransparent)
                            GenerateBlock_Bottom(ref currentIndex, offset, vertices, normals, uvs, indices,
                                _blockIDs[x, y, z]);
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

    private void GenerateBlock_Top(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int id)
    {
        vertices.Add(new Vector3(0f, 1f, 1f) + offset);
        vertices.Add(new Vector3(1f, 1f, 1f) + offset);
        vertices.Add(new Vector3(1f, 1f, 0f) + offset);
        vertices.Add(new Vector3(0f, 1f, 0f) + offset);

        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);

        uvs.AddRange(Blocks[id].TopUVs());

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    private void GenerateBlock_Right(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int id)
    {
        vertices.Add(new Vector3(1f, 1f, 0f) + offset);
        vertices.Add(new Vector3(1f, 1f, 1f) + offset);
        vertices.Add(new Vector3(1f, 0f, 1f) + offset);
        vertices.Add(new Vector3(1f, 0f, 0f) + offset);

        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        
        uvs.AddRange(Blocks[id].SideUVs());

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    private void GenerateBlock_Left(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int id)
    {
        vertices.Add(new Vector3(0f, 1f, 1f) + offset);
        vertices.Add(new Vector3(0f, 1f, 0f) + offset);
        vertices.Add(new Vector3(0f, 0f, 0f) + offset);
        vertices.Add(new Vector3(0f, 0f, 1f) + offset);

        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        
        uvs.AddRange(Blocks[id].SideUVs());
        

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    private void GenerateBlock_Forward(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int id)
    {
        vertices.Add(new Vector3(1f, 1f, 1f) + offset);
        vertices.Add(new Vector3(0f, 1f, 1f) + offset);
        vertices.Add(new Vector3(0f, 0f, 1f) + offset);
        vertices.Add(new Vector3(1f, 0f, 1f) + offset);

        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);

        uvs.AddRange(Blocks[id].SideUVs());

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    private void GenerateBlock_Back(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int id)
    {
        vertices.Add(new Vector3(0f, 1f, 0f) + offset);
        vertices.Add(new Vector3(1f, 1f, 0f) + offset);
        vertices.Add(new Vector3(1f, 0f, 0f) + offset);
        vertices.Add(new Vector3(0f, 0f, -0) + offset);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        uvs.AddRange(Blocks[id].SideUVs());
        
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    private void GenerateBlock_Bottom(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int id)
    {
        vertices.Add(new Vector3(0f, 0f, 0f) + offset);
        vertices.Add(new Vector3(1f, 0f, 0f) + offset);
        vertices.Add(new Vector3(1f, 0f, 1f) + offset);
        vertices.Add(new Vector3(0f, 0f, 1f) + offset);

        normals.Add(Vector3.down);
        normals.Add(Vector3.down);
        normals.Add(Vector3.down);
        normals.Add(Vector3.down);

        uvs.AddRange(Blocks[id].BotUVs());

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
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

            var bIds = new int[Size, maxHeight, Size];

            for (var i = 0; i < chunckInfo.blockIDs.Length; i++)
            {
                var z = i % Size;
                var y = (i / Size) % maxHeight;
                var x = i / (Size * maxHeight);

                bIds[x, y, z] = chunckInfo.blockIDs[i];
            }
            var cChunck = c.GetComponent<Chunck>();
            cChunck.BlockIDs = bIds;
            if (chunckInfo.waterIDs != null)
            {
                var wIds = new int[Size, maxHeight, Size];
                for (var i = 0; i < chunckInfo.waterIDs.Length; i++)
                {
                    var z = i % Size;
                    var y = (i / Size) % maxHeight;
                    var x = i / (Size * maxHeight);

                    wIds[x, y, z] = chunckInfo.waterIDs[i];
                }
                cChunck.WaterIDs = wIds;
            }
        
            Chuncks.Add(c);
            _chuncks.Add(new Vector2(c.transform.position.x, c.transform.position.z), c);
            
            GenerateMesh(c);
        }
    }
    
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
        var biome = biomes[GetBiome(block.x, block.z)];
        
        if (block.y >= height + biome.minHeight)
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
        catch (Exception e)
        {
            return null;
        }
    }

    private int GetBiome(float x, float z)
    {
        biomeNoiseOffset = new Vector2(seed * 0.5f, seed * 0.25f);   
        var height0 = Mathf.PerlinNoise((x + biomeNoiseOffset.x) * biomeNoise + seed, (z + biomeNoiseOffset.y) * biomeNoise + seed);
        return (int)(height0 * biomes.Length);
    }
    
    private int GetHeight(float x, float z)
    {
        var biome = biomes[GetBiome(x, z)];
        
        var height0 = Mathf.PerlinNoise(x * biome.noiseMult1 + seed, z * biome.noiseMult1 + seed);
        var height1 = Mathf.PerlinNoise(x * biome.noiseMult2 + seed, z * biome.noiseMult2 + seed);

        return (int)(height0 * height1 * (biome.maxGeneratingHeight - biome.minHeight));
    }
    
    public int Seed
    {
        get => seed;
        set => seed = value;
    }

    public List<GameObject> Chuncks { get; set; } = new List<GameObject>();

    public int MAXHeight => maxHeight;

    public int Size1 => Size;

    public Dictionary<int, Blocks> Blocks { get; } = new Dictionary<int, Blocks>();
}


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
    [SerializeField] public int minHeight;
    [SerializeField] public float noiseThreshold;
    [SerializeField] private int seed;
    [SerializeField] public GameObject chunckGameObject;
    [SerializeField] private GameObject destroyedBlock;
    [SerializeField] private float renderDistance;
    [SerializeField] private BlockShape destroyedBlockShape;
    
    public List<MeshCreation> meshesToUpdate = new List<MeshCreation>();
    public List<Water> waterMeshesToUpdate = new List<Water>();
    public List<MeshCreation> meshesToApply = new List<MeshCreation>();
    public List<Water> waterMeshesToApply = new List<Water>();
    
    [Header("Tree")]
    [SerializeField]
    public float treeThreshold;
    [SerializeField] public int minTreeHeight;
    [SerializeField] public int maxTreeHeight;

    [Header("Ores")]
    [SerializeField]
    public Vector3 ironNoiseOffset;
    [SerializeField] public float ironNoiseThreshold;
    
    [Header("Player")]
    [SerializeField] private GameObject player;
    private PlayerInventory _playerInventory;
    
    [Header("Scripts")]
    [SerializeField]
    public BlockTypes blockTypes;
    [SerializeField] public BlockCreation blockCreation;
    [SerializeField] public SaveManager saveManager;
    
    
    [Header("Blocks and Biomes")]
    [SerializeField] private Blocks[] blocks;
    [SerializeField] public Biome[] biomes;
    public readonly Dictionary<Vector2, GameObject> _chuncks = new Dictionary<Vector2, GameObject>();
    public Dictionary<Vector2, Chunck> _chuncksChunck = new Dictionary<Vector2, Chunck>();
    private int _lastChunck = 0;
    private bool _firstGen = true;
    

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
        GenerateChunck();
    }
    
    private void Update()
    {
        if (_firstGen)
        {
            for (var i = meshesToApply.Count - 1; i >= 0; i--)
            {
                var mesh = meshesToApply[i];
                if(mesh == null){ meshesToApply.Remove(mesh); continue;}
                mesh.ApplyMesh();
                meshesToApply.Remove(mesh);
                saveManager.SaveChunck(mesh.gameObject.GetComponent<Chunck>());
            }
            
            for (var i = waterMeshesToApply.Count - 1; i >= 0; i--)
            {
                var mesh = waterMeshesToApply[i];
                if(mesh == null){ waterMeshesToApply.Remove(mesh); continue;}
                mesh.ApplyMesh();
                waterMeshesToApply.Remove(mesh);
            }

            _firstGen = false;
        }
        
        if(meshesToUpdate.Count > 0)
        {
            for (var i = meshesToUpdate.Count - 1; i >= 0; i--)
            {
                var mesh = meshesToUpdate[i];
                if (!mesh.CanGenerateMesh) continue;
                
                mesh.GenerateMesh();
                meshesToUpdate.Remove(mesh);
            }
        }
        
        if(waterMeshesToUpdate.Count > 0)
        {
            for (var i = waterMeshesToUpdate.Count - 1; i >= 0; i--)
            {
                var mesh = waterMeshesToUpdate[i];
                if (!mesh.CanGenerateMesh) continue;
                
                mesh.GenerateWater();
                waterMeshesToUpdate.Remove(mesh);
            }
        }
        
        if(meshesToApply.Count > 0)
        {
            var mesh = meshesToApply[0];
            mesh.ApplyMesh();
            meshesToApply.Remove(mesh);
            saveManager.SaveChunck(mesh.gameObject.GetComponent<Chunck>());
        }
        
        
        if(waterMeshesToApply.Count > 0)
        {
            var mesh = waterMeshesToApply[0];
            mesh.ApplyMesh();
            waterMeshesToApply.Remove(mesh);
            saveManager.SaveChunck(mesh.gameObject.GetComponent<Chunck>());
        }
        
        
        var position = Vector3Int.FloorToInt(player.transform.position);
        var currentChunck = (position.x - (position.x % 8) + position.z - (position.z % 8));
        if (currentChunck != _lastChunck)
        {
            GenerateChunck();
        }

        _lastChunck = currentChunck;
    }


    // ReSharper disable Unity.PerformanceAnalysis
    private void GenerateChunck()
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

                if (c == null)
                {
                    var newChunck = Instantiate(chunckGameObject, new Vector3(x, 0, z), Quaternion.identity);
                    var m = newChunck.GetComponent<MeshCreation>();
                    m.worldCreation = this;
                    m.GenerateMesh();
                    Chuncks.Add(newChunck);
                    _chuncks.Add(new Vector2(newChunck.transform.position.x, newChunck.transform.position.z), newChunck);
                    _chuncksChunck.Add(new Vector2(newChunck.transform.position.x, newChunck.transform.position.z), newChunck.GetComponent<Chunck>());
                    
                }
                else
                {
                    c.gameObject.SetActive(true);
                }
            }
        }
        
        if (Chuncks.Count < 0) return;
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

        if (c.WaterIDs[bix, biy + 1, biz] != 0)
        {
            chunck.GetComponent<Water>().UpdateWaterDown(block);
        }

        var m = chunck.GetComponent<MeshCreation>();
        
        if(m.CanGenerateMesh)
            m.GenerateMesh();
        else
            meshesToUpdate.Add(m);
        
        saveManager.SaveChunck(chunck);

        if (bix == 0)
        {
            if(GetBlock(new Vector3(-1, 0, 0) + block) != 0 || GetWater(new Vector3(-1, 0, 0) + block) == 1)
                ReloadChunck(new Vector3(-1, 0, 0) + block);
        }
        
        if (bix == size - 1){
            if(GetBlock(new Vector3(1, 0, 0) + block) != 0 || GetWater(new Vector3(1, 0, 0) + block) == 1)
                ReloadChunck(new Vector3(1, 0, 0) + block);
        }
        
        if (biz == 0)
        {
            if(GetBlock(new Vector3(0, 0, -1) + block) != 0 || GetWater(new Vector3(0, 0, -1) + block) == 1)
                ReloadChunck(new Vector3(0, 0, -1) + block);
        }
        
        if (biz == size - 1)
        {
            if(GetBlock(new Vector3(0, 0, 1) + block) != 0 || GetWater(new Vector3(0, 0, 1) + block) == 1)
                ReloadChunck(new Vector3(0, 0, 1) + block);
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void PlaceBlock(Vector3 block, int id)
    {
        var chunck = GetChunck(block);
        
        var bix = Mathf.FloorToInt(block.x) - (int)chunck.transform.position.x;
        var biy = Mathf.FloorToInt(block.y);
        var biz = Mathf.FloorToInt(block.z) - (int)chunck.transform.position.z;
        
        var c = chunck.GetComponent<Chunck>();
        
        c.BlockIDs[bix, biy, biz] = id;
        c.WaterIDs[bix, biy, biz] = 0;
        
        saveManager.SaveChunck(c);

        var m = chunck.GetComponent<MeshCreation>();
        
        if(m.CanGenerateMesh)
            m.GenerateMesh();
        else
            meshesToUpdate.Add(m);
    }

    public float Perlin3D(float x, float y, float z)
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

    public void LoadData(int seed)
    {
        this.seed = seed;
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
            if (chunck.BlockIDs != null)
            {
                return chunck.BlockIDs[bix, biy, biz];
            }
        }

        var height = GetHeight(block.x, block.z);
        
        if (block.y >= height + minHeight)
            return 0;

        return Perlin3D((block.x) * 0.05f + seed, (float) (block.y) * 0.05f + seed,
            (block.z) * 0.05f + seed) >= noiseThreshold
            ? 1
            : 0;
    }
    
    public int GetWater(Vector3 block)
    {
        var chunkPosX = Mathf.FloorToInt(block.x / 16f) * 16;
        var chunkPosZ = Mathf.FloorToInt(block.z / 16f) * 16;
        var chunck = GetChunck(block);
        if (chunck == null) 
            return GetBlock(block) == 0 ?  GetHeight(block.x, block.z) < block.y ? 1 : 0: 1;
        var bix = Mathf.FloorToInt(block.x) - chunkPosX;
        var biy = Mathf.FloorToInt(block.y);
        var biz = Mathf.FloorToInt(block.z) - chunkPosZ;
        if (chunck.WaterIDs != null)
        {
            return chunck.WaterIDs[bix, biy, biz];
        }

        return 1;
    }

    public bool GetUnderWater(Vector3 pos)
    {
        var chunck = GetChunck(pos);
        var chunckPos = chunck.transform.position;
        var bix = Mathf.FloorToInt(pos.x) - (int)chunckPos.x;
        var biy = Mathf.FloorToInt(pos.y);
        var biz = Mathf.FloorToInt(pos.z) - (int)chunckPos.z;

        return chunck.WaterIDs[bix, biy, biz] != 0;
    }

    public void ReloadChunck(Vector3 block)
    {
        var m = GetChunck(block).GetComponent<MeshCreation>();
        
        if (m.CanGenerateMesh)
            m.GenerateMesh();
        else
            meshesToUpdate.Add(m);
    }

    public Chunck GetChunck(Vector3 block)
    {
        var chunkPosX = Mathf.FloorToInt(block.x / 16f) * 16;
        var chunkPosZ = Mathf.FloorToInt(block.z / 16f) * 16;

        try
        {
            return _chuncksChunck[new Vector2(chunkPosX, chunkPosZ)];
        }
        catch
        {
            return null;
        }
    }

    public int GetBiome(float x, float z)
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

    public int GetHeight(float x, float z)
    {
        var height = 0;
        var count = 0;
        
        foreach (var t in biomes)
        {
            var weight = Mathf.PerlinNoise((x + t.offset.x) * 0.005f + seed,
                (z + t.offset.y) * 0.005f + seed);
            
            var height0 = Mathf.PerlinNoise(x * t.noiseMult1 + seed, z * t.noiseMult1 + seed);
            var height1 = Mathf.PerlinNoise(x * t.noiseMult2 + seed, z * t.noiseMult2 + seed);
            var h = (int) ((height0 + height1) * t.maxGeneratingHeight * weight);

            if (h < 0) continue;
            height += h;
            count++;
        }

        height /= count;
        return height;
    }

    private GameObject CreateDestroyedBlock(int id, Vector3 pos)
    {
        var block = Instantiate(destroyedBlock, pos, Quaternion.identity);
        block.GetComponent<DestroyedBlock>().ID = id;

        var newMesh = new Mesh();
        var _vertices = new List<Vector3>();
        var _normals = new List<Vector3>();
        var _uvs = new List<Vector2>();
        var _indices = new List<int>();

        var currentIndex = 0;

        var b = Blocks[id];

        var offset = new Vector3Int(0, 0, 0);
        
        blockCreation.GenerateBlock(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                    destroyedBlockShape.faceData[2], b.GETRect(b.topIndex));
        blockCreation.GenerateBlock(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
            destroyedBlockShape.faceData[5], b.GETRect(b.rightIndex));
        blockCreation.GenerateBlock(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
            destroyedBlockShape.faceData[4], b.GETRect(b.leftIndex));
        blockCreation.GenerateBlock(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
            destroyedBlockShape.faceData[1], b.GETRect(b.frontIndex));
        blockCreation.GenerateBlock(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
            destroyedBlockShape.faceData[0], b.GETRect(b.backIndex));
        blockCreation.GenerateBlock(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
            destroyedBlockShape.faceData[3], b.GETRect(b.botIndex));
        
        newMesh.SetVertices(_vertices);
        newMesh.SetNormals(_normals);
        newMesh.SetUVs(0, _uvs);
        newMesh.SetIndices(_indices, MeshTopology.Triangles, 0);

        newMesh.RecalculateTangents();
        block.GetComponent<MeshFilter>().mesh = newMesh;
        block.GetComponent<MeshCollider>().sharedMesh = newMesh;
        
        return block;
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


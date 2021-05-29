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
    [SerializeField] private GameObject chunckGameObject;
    [SerializeField] private GameObject destroyedBlock;
    [SerializeField] private float renderDistance;
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
    
    
    [Header("Blocks and Biomes")]
    [SerializeField] private Blocks[] blocks;
    [SerializeField] public Biome[] biomes;
    private readonly Dictionary<Vector2, GameObject> _chuncks = new Dictionary<Vector2, GameObject>();
    private Dictionary<Vector2, Chunck> _chuncksChunck = new Dictionary<Vector2, Chunck>();
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
                mesh.ApplyMesh();
                meshesToApply.Remove(mesh);
            }
            
            for (var i = waterMeshesToApply.Count - 1; i >= 0; i--)
            {
                var mesh = waterMeshesToApply[i];
                mesh.ApplyMesh();
                waterMeshesToApply.Remove(mesh);
            }

            _firstGen = false;
        }
        
        if(meshesToApply.Count > 0)
        {
            var mesh = meshesToApply[0];
            mesh.ApplyMesh();
            meshesToApply.Remove(mesh);
        }
        
        if(waterMeshesToApply.Count > 0)
        {
            var mesh = waterMeshesToApply[0];
            mesh.ApplyMesh();
            waterMeshesToApply.Remove(mesh);
        }
        
        
        var position = Vector3Int.FloorToInt(player.transform.position);
        var currentChunck = (position.x - (position.x % 8) + position.z - (position.z % 8));
        if (currentChunck != _lastChunck)
        {
            GenerateChunck();
        }

        _lastChunck = currentChunck;
    }


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

                if (!c)
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
        
        chunck.GetComponent<MeshCreation>().GenerateMesh();

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
        
        chunck.GetComponent<MeshCreation>().water.GenerateWater();
        chunck.GetComponent<MeshCreation>().GenerateMesh();
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

    public void LoadData(int seed, List<ChunckInfo> chunckInfos)
    {
        this.seed = seed;
        
        if(chunckInfos.Count == 0) return;
        
        foreach (var chunckInfo in chunckInfos)
        {
            var c = Instantiate(chunckGameObject, chunckInfo.pos, Quaternion.identity);
            var m = c.GetComponent<MeshCreation>();
            m.worldCreation = this;

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
            _chuncksChunck.Add(new Vector2(c.transform.position.x, c.transform.position.z), c.GetComponent<Chunck>());
            
            c.GetComponent<MeshCreation>().GenerateMesh();
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

    private void ReloadChunck(Vector3 block)
    {
        GetChunck(block).GetComponent<MeshCreation>().GenerateMesh();;
    }

    private Chunck GetChunck(Vector3 block)
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


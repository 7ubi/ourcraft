using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
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
    [SerializeField] private GameObject destroyedItem;
    [SerializeField] private float renderDistance;

    public List<Chunck> chunksInRange = new List<Chunck>();
    public List<MeshCreation> meshesToCreate = new List<MeshCreation>();
    public List<MeshCreation> meshesToUpdate = new List<MeshCreation>();
    public List<WaterGeneration> waterMeshesToUpdate = new List<WaterGeneration>();
    public List<MeshCreation> meshesToApply = new List<MeshCreation>();
    public List<WaterGeneration> waterMeshesToApply = new List<WaterGeneration>();
    private List<GameObject> _chunksToDeactivate = new List<GameObject>();
    private List<GameObject> _chunksToActivate = new List<GameObject>();
    public List<DestroyedBlock> destroyedBlocksToCreate = new List<DestroyedBlock>();
    public List<DestroyedBlock> destroyedBlocksToApply = new List<DestroyedBlock>();

    
    [Header("Tree")]
    [SerializeField]
    public float treeThreshold;
    [SerializeField] public int minTreeHeight;
    [SerializeField] public int maxTreeHeight;

    [Header("Ores")]
    [SerializeField] public Vector3 ironNoiseOffset;
    [SerializeField] public float ironNoiseThreshold;
    [SerializeField] public Vector3 coalNoiseOffset;
    [SerializeField] public float coalNoiseThreshold;
    
    [Header("Player")]
    [SerializeField] private GameObject player;
    private PlayerInventory _playerInventory;
    
    [Header("Scripts")]
    [SerializeField]
    public BlockTypes blockTypes;
    [SerializeField] public BlockCreation blockCreation;
    [SerializeField] public SaveManager saveManager;
    [SerializeReference] public Voxelizer voxelizer;
    
    
    [Header("Blocks and Biomes")]
    [SerializeField] private Blocks[] blocks;
    [SerializeField] public Biome[] biomes;
    [SerializeField] public BlockShape standardBlockShape;
    public readonly Dictionary<Vector2, GameObject> _chuncks = new Dictionary<Vector2, GameObject>();
    public Dictionary<Vector2, Chunck> _chuncksChunck = new Dictionary<Vector2, Chunck>();
    private int _lastChunck = 0;

    private Thread _chunckTread; 

    private void Start()
    {
        _playerInventory = player.GetComponent<PlayerInventory>();

        foreach (var b in blocks)
        {
            Blocks.Add(b.id, b);    
        }
        
        _chunckTread = new Thread(UpdateChunck);
        
    }

    

    public void GenerateFirst()
    {
        seed = Random.Range(10000, 100000);
        LoadNearestChuncks();
    }
    

    private void UpdateChunck()
    {
        while (true)
        {
            if (meshesToUpdate.Count > 0)
            {
                
                var mesh = meshesToUpdate[meshesToUpdate.Count - 1];
                if(mesh == null) continue;   
                mesh.GenerateMesh();
                meshesToUpdate.Remove(mesh);
                
            }

            if (waterMeshesToUpdate.Count > 0)
            {
                
                var mesh = waterMeshesToUpdate[waterMeshesToUpdate.Count - 1];
                if(mesh == null) continue;   

                mesh.GenerateWater();
                waterMeshesToUpdate.Remove(mesh);
            
            }
            
            if (meshesToCreate.Count > 0)
            {
                
                var mesh = meshesToCreate[meshesToCreate.Count - 1];
                if(mesh == null) continue;                    
                mesh.GenerateBlocks();
                meshesToCreate.Remove(mesh);
            
            }

            if (destroyedBlocksToCreate.Count > 0)
            {
                var mesh = destroyedBlocksToCreate[destroyedBlocksToCreate.Count - 1];
                if(mesh == null) continue;                    
                mesh.CreateBlock();
                destroyedBlocksToCreate.Remove(mesh); 
            }
        }
    }

    private void OnDisable()
    {
        _chunckTread.Abort();
    }

    private void Update()
    {
        var position = Vector3Int.FloorToInt(player.transform.position);
        
        
        if (meshesToApply.Count > 0)
        {
            var mesh = meshesToApply[0];
            mesh.ApplyMesh();
            

            meshesToApply.Remove(mesh);
            saveManager.SaveChunck(mesh.chunck);
        }
        
        if (waterMeshesToApply.Count > 0)
        {
            var mesh = waterMeshesToApply[0];
            mesh.ApplyMesh();
            waterMeshesToApply.Remove(mesh);
            saveManager.SaveChunck(mesh.chunck);
        }

        
        
        if (_chunksToActivate.Count > 0)
        {
            _chunksToActivate[0].SetActive(true);
            _chunksToActivate.Remove(_chunksToActivate[0]);
        }
        else
        {
            if (_chunksToDeactivate.Count > 0)
            {
                _chunksToDeactivate[0].SetActive(false);
                _chunksToDeactivate.Remove(_chunksToDeactivate[0]);
            }
        }

        if (destroyedBlocksToApply.Count > 0)
        {
            var d = destroyedBlocksToApply[0];
            
            d.ApplyBlock();
            destroyedBlocksToApply.Remove(d);
        }

        var currentChunk = (position.x - (position.x % Size) + position.z - (position.z % Size));
        if (currentChunk != _lastChunck)
            GenerateChunck();
        
        

        _lastChunck = currentChunk;
    }

    public void LoadNearestChuncks()
    {
        var position = Vector3Int.FloorToInt(player.transform.position);
        
        var playerX = position.x - (position.x % 16);
        var playerZ = position.z - (position.z % 16);
        
        var minX = Convert.ToInt32(playerX - size * renderDistance);
        var maxX = Convert.ToInt32(playerX + size * renderDistance);
        var minZ = Convert.ToInt32(playerZ - size * renderDistance);
        var maxZ = Convert.ToInt32(playerZ + size * renderDistance);

        for (var _x = minX + size / 2; _x <= maxX + size / 2; _x += size)
        {
            for (var _z = minZ + size / 2; _z <= maxZ + size / 2; _z += size)
            {
                var x = _x - size / 2;
                var z = _z - size / 2;
                var c = GetChunck(new Vector3(x, 0, z));
                
            
                if (c == null)
                {
                    var newChunck = Instantiate(chunckGameObject, new Vector3(x, 0, z), Quaternion.identity);
                    c = newChunck.GetComponent<Chunck>();
                    var m = newChunck.GetComponent<MeshCreation>();
                    m.worldCreation = this;
                    m.Init();
                    Chuncks.Add(newChunck);
                    var position1 = newChunck.transform.position;
                    _chuncks.Add(new Vector2(position1.x, position1.z), newChunck);
                    _chuncksChunck.Add(new Vector2(position1.x, position1.z), newChunck.GetComponent<Chunck>());

                    newChunck.GetComponent<Chunck>().worldCreation = this;

                    var water = newChunck.GetComponent<WaterGeneration>();

                    m.GenerateBlocks();

                    m.GenerateMesh();
                    water.GenerateWater();

                    m.ApplyMesh();
                    water.ApplyMesh();

                    waterMeshesToUpdate.Remove(water);
                    waterMeshesToApply.Remove(water);
                    meshesToCreate.Remove(m);
                    meshesToUpdate.Remove(m);
                    meshesToApply.Remove(m);
                    saveManager.SaveChunck(m.chunck);
                }
                else
                {
                    var mesh = c.GetComponent<MeshCreation>();
                    var water = c.GetComponent<WaterGeneration>();

                    mesh.GenerateMesh();
                    water.GenerateWater();

                    mesh.ApplyMesh();
                    water.ApplyMesh();

                    waterMeshesToUpdate.Remove(water);
                    waterMeshesToApply.Remove(water);
                    meshesToUpdate.Remove(mesh);
                    meshesToApply.Remove(mesh);
                    saveManager.SaveChunck(mesh.chunck);
                }

                chunksInRange.Add(c);
                c.gameObject.SetActive(true);
            }
        }
        _chunckTread.Start();
    }
    

    // ReSharper disable Unity.PerformanceAnalysis
    public void GenerateChunck()
    {
        var position = player.transform.position;
        var playerX = position.x - (position.x % 16);
        var playerZ = position.z - (position.z % 16);
        
        var minX = Convert.ToInt32(playerX - size * renderDistance);
        var maxX = Convert.ToInt32(playerX + size * renderDistance);
        var minZ = Convert.ToInt32(playerZ - size * renderDistance);
        var maxZ = Convert.ToInt32(playerZ + size * renderDistance);

        chunksInRange.Clear();
        
        for (var _x = minX + size / 2; _x <= maxX + size / 2; _x += size)
        {
            for (var _z = minZ + size / 2; _z <= maxZ + size / 2; _z += size)
            {
                var x = _x - size / 2;
                var z = _z - size / 2;
                
                var c = GetChunck(new Vector3(x, 0, z));

                if (c == null)
                {
                    var newChunck = Instantiate(chunckGameObject, new Vector3(x, 0, z), Quaternion.identity);
                    var m = newChunck.GetComponent<MeshCreation>();
                    m.worldCreation = this;
                    m.Init();
                    Chuncks.Add(newChunck);
                    _chuncks.Add(new Vector2(newChunck.transform.position.x, newChunck.transform.position.z),
                        newChunck);
                    _chuncksChunck.Add(new Vector2(newChunck.transform.position.x, newChunck.transform.position.z),
                        newChunck.GetComponent<Chunck>());
                    newChunck.GetComponent<Chunck>().worldCreation = this;
                    c = newChunck.GetComponent<Chunck>();
                }
                else
                {
                    if (!c.GetComponent<MeshCreation>().InRange)
                    {
                        meshesToUpdate.Add(c.GetComponent<MeshCreation>());
                        c.GetComponent<MeshCreation>().InRange = true;
                    }

                    if (c.gameObject.activeInHierarchy == false)
                    {
                        _chunksToActivate.Add(c.gameObject);
                    }
                }

                chunksInRange.Add(c);
            }
        }
        
        if (Chuncks.Count < 0) return;
        {
            for (var i = _chuncksChunck.Count - 1; i >= 0; i--)
            {
                var chunck = _chuncksChunck.ElementAt(i).Value;
                if (chunksInRange.Contains(chunck)) continue;
                if (!chunck.gameObject.activeInHierarchy) continue;
                
                _chunksToDeactivate.Add(chunck.gameObject);
            }
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void DestroyBlock(Vector3 block)
    {

        var chunck = GetChunck(block);

        var position = chunck.transform.position;
        var bix = Mathf.FloorToInt(block.x) - (int)position.x;
        var biy = Mathf.FloorToInt(block.y);
        var biz = Mathf.FloorToInt(block.z) - (int)position.z;
        var c = chunck.GetComponent<Chunck>();
        
        _playerInventory.AddDestroyedBlock(
            CreateDestroyedBlock(Blocks[c.BlockIDs[bix, biy, biz]].DropID,
                block + new Vector3(0.375f, 0.1f, 0.375f)));

        if (c.BlockIDs[bix, biy, biz] == BlockTypes.Furnace)
        {
            c.furnaces.Remove(new Vector3Int(bix, biy, biz));
        }
        
        c.BlockIDs[bix, biy, biz] = 0;

        if (c.WaterIDs[bix, biy + 1, biz] != 0)
        {
            chunck.GetComponent<Water>().UpdateWaterDown(block);
        }

        var m = chunck.GetComponent<MeshCreation>();
        
        m.Init();


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
        
        for (var x = -1; x <= 1; x++)
        {
            for (var z = -1; z <= 1; z++)
            {
                if (GetWater(block + new Vector3(x, 0, z)) == 0) continue;
                GetChunck(block).GetComponent<Water>().DistributeWater(block + new Vector3(x, 0, z));
                break;
            }
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void PlaceBlock(Vector3 block, int id, int orientation)
    {
        var chunck = GetChunck(block);

        var position = chunck.transform.position;
        
        var bix = Mathf.FloorToInt(block.x) - (int) position.x;
        var biy = Mathf.FloorToInt(block.y);
        var biz = Mathf.FloorToInt(block.z) - (int) position.z;

        var c = chunck.GetComponent<Chunck>();

        c.BlockIDs[bix, biy, biz] = id;
        c.WaterIDs[bix, biy, biz] = 0;

        c.Orientation[bix, biy, biz] = Blocks[id].isRotatable ? orientation : 1;
        
        if (id == BlockTypes.Furnace)
        {
            var pos = new Vector3Int(bix, biy, biz);
            c.furnaces.Add(pos, new Furnace(pos));
        }
        
        saveManager.SaveChunck(c);

        var m = chunck.GetComponent<MeshCreation>();

        m.Init();
        
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

        return Noise.Perlin3D((block.x) * 0.05f + seed, (float) (block.y) * 0.05f + seed,
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
        try
        {
            if (chunck.WaterIDs != null)
            {
                return chunck.WaterIDs[bix, biy, biz];
            }
        }catch{}

        return 1;
    }

    public bool GetUnderWater(Vector3 pos)
    {
        return GetWater(pos) != 0;
    }

    public void ReloadChunck(Vector3 block)
    {
        var m = GetChunck(block).GetComponent<MeshCreation>();
        
        m.Init();
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
            var t = biomes[i];
            var weight = Mathf.PerlinNoise((x + t.offset.x  + seed) / t.scale * 0.5f,
                (z + t.offset.y + seed) / t.scale * 0.5f);

            if (!(weight > strongestWeight)) continue;
            strongestWeight = weight;
            index = i;
        }
                       
        return index;
    }

    public int GetHeight(float x, float z)
    {
        var height = 0;
        
        foreach (var t in biomes)
        {
            var weight = Mathf.PerlinNoise((x + t.offset.x  + seed) / t.scale * 0.5f,
                (z + t.offset.y + seed) / t.scale * 0.5f);

            var h = (int) (t.GetHeight(x, z, seed) * weight);

            if (h < 0) continue;
            height += h;
        }

        height /= biomes.Length;
        return height;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public GameObject CreateDestroyedBlock(int id, Vector3 pos)
    {
        var block = id < _playerInventory.itemIndexStart ? 
            Instantiate(destroyedBlock, pos, Quaternion.identity, GetChunck(pos).transform):
            Instantiate(destroyedItem, pos, Quaternion.identity, GetChunck(pos).transform);

        block.GetComponent<DestroyedBlock>().Init(id, id < _playerInventory.itemIndexStart,
            this, _playerInventory);
        
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

    public float RenderDistance => renderDistance;

    public Dictionary<int, Blocks> Blocks { get; } = new Dictionary<int, Blocks>();
}


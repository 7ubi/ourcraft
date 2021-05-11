using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Serialization;

public class worldCreation : MonoBehaviour
{
    //base code: https://github.com/Absurdponcho/BlockGame/blob/master/Assets/VoxelChunk.cs
    [SerializeField] int Size = 16;
    [SerializeField] private int maxHeight;
    private short[,,] BlockIDs;
    [SerializeField] private float noiseThreshold;
    
    [SerializeField] private int seed;
    [SerializeField] private GameObject Chunck;

    [SerializeField] private float renderDistance;
    [SerializeField] private GameObject Player;
    private List<GameObject> chuncks = new List<GameObject>();
    private int _lastChunck = 0;

    private Blocks _blocks;

    private void Start()
    {
        _blocks = GetComponent<Blocks>();
        GenerateChunck();
        
    }
    
    private void Update()
    {
        var position = Vector3Int.FloorToInt(Player.transform.position);
        var currentChunck = (position.x - (position.x % 8) + position.z - (position.z % 8));
        if (currentChunck != _lastChunck)
        {
            GenerateChunck();
        }

        _lastChunck = currentChunck;
    }

    void GenerateBlocks(Vector2 offset)
    {
        BlockIDs = new short[Size, maxHeight, Size];
        for (var x = 0; x < Size; x++)
        {
            for (var z = 0; z < Size; z++)
            {
                var height0 = Mathf.PerlinNoise((x + offset.x) * 0.1f + seed, (z + offset.y) * 0.1f + seed);
                var height1 = Mathf.PerlinNoise((x  + offset.x) * 0.05f + seed, (z + offset.y) * 0.05f  + seed);
                var height2 = Mathf.PerlinNoise((x + offset.x) * 0.001f + seed, (z + offset.y) * 0.001f + seed);

                var height = (int)(height0 * height1 * height2 * (maxHeight - 5));
                
                if (height <= 1)
                    height = 1;

                if (Perlin3D((x + offset.x) * 0.05f + seed, (float) height * 0.05f + seed,
                    (z + offset.y) * 0.05f + seed) >= noiseThreshold)
                {
                    try
                    {
                        BlockIDs[x, height, z] = 2;
                    }
                    catch (Exception e)
                    {
                        Debug.Log(height);
                        Debug.Log(Mathf.PerlinNoise((x  + offset.x) * 0.05f + seed, (z + offset.y) * 0.05f  + seed) * (maxHeight - 2));
                    }
                }

                for(var y = height - 1; y >= 1; y--)
                {
                    if(Perlin3D((x  + offset.x) * 0.05f + seed, (float)y * 0.05f + seed, (z + offset.y) * 0.05f  + seed) < noiseThreshold)
                        continue;
                    
                    if (y <= height - 4)
                        BlockIDs[x, y, z] = 3;
                    else
                        BlockIDs[x, y, z] = 1;
                }
                BlockIDs[x, 0, z] = 3;
            }
            
        }
    }

    private void GenerateChunck()
    {
        var position = Player.transform.position;
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
                var exists = chuncks.Any(chunck =>
                {
                    Vector3 position1;
                    return Math.Abs((float) ((position1 = chunck.transform.position).x - x)) < 0.1f && Math.Abs((float) (position1.z - z)) < 0.1f;
                });

                if (exists) continue;
                var newChunck = Instantiate(Chunck, new Vector3(x, 0, z), Quaternion.identity);
                GenerateMesh(newChunck);
                chuncks.Add(newChunck);
            }
        }

        if(chuncks.Count == 0)
            return;
        
        for(var i = chuncks.Count - 1; i >= 0; i--)
        {
            var chunck = chuncks[i];
            if (!(chunck.transform.position.x < minX) && !(chunck.transform.position.x > maxX) &&
                !(chunck.transform.position.z < minZ) && !(chunck.transform.position.z > maxZ)) continue;
            chuncks.Remove(chunck);
            Destroy(chunck);
        }
    }

    public void DestroyBlock(Vector3 block)
    {
        var chunkPosX = Mathf.FloorToInt(block.x / 16f) * 16;
        var chunkPosZ = Mathf.FloorToInt(block.z / 16f) * 16;

        foreach (var chunck in chuncks.Where(chunck => Math.Abs((float) (chunck.transform.position.x - chunkPosX)) < 0.1f &&
                                                       Math.Abs((float) (chunck.transform.position.z - chunkPosZ)) < 0.1))
        {
            var bix = Mathf.FloorToInt(block.x) - chunkPosX;
            var biy = Mathf.FloorToInt(block.y);
            var biz = Mathf.FloorToInt(block.z) - chunkPosZ;
            chunck.GetComponent<Chunck>().BlockIDs[bix, biy, biz] = 0;
            GenerateMesh(chunck);
        }
    }

    public void PlaceBlock(Vector3 block)
    {
        var chunkPosX = Mathf.FloorToInt(block.x / 16f) * 16;
        var chunkPosZ = Mathf.FloorToInt(block.z / 16f) * 16;
        
        

        foreach (var chunck in chuncks.Where(chunck =>
            Math.Abs((float) (chunck.transform.position.x - chunkPosX)) < 0.1f &&
            Math.Abs((float) (chunck.transform.position.z - chunkPosZ)) < 0.1))
        {
            var bix = Mathf.FloorToInt(block.x) - chunkPosX;
            var biy = Mathf.FloorToInt(block.y);
            var biz = Mathf.FloorToInt(block.z) - chunkPosZ;
            
            
            chunck.GetComponent<Chunck>().BlockIDs[bix, biy, biz] = 1;
            GenerateMesh(chunck);
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void GenerateMesh(GameObject chunck)
    {
        var position = chunck.transform.position;
        var c = chunck.GetComponent<Chunck>();
        if (c.BlockIDs == null)
        {
            GenerateBlocks(new Vector2(position.x, position.z));
            chunck.GetComponent<Chunck>().BlockIDs = BlockIDs;
        }
        else
        {
            BlockIDs = c.BlockIDs;
        }

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
                    if (BlockIDs[x, y, z] == 0) continue;
                    else
                    {
                        if (y < Size - 1)
                        {
                            if (BlockIDs[x, y + 1, z] == 0)
                                GenerateBlock_Top(ref currentIndex, offset, vertices, normals, uvs, indices, BlockIDs[x, y, z]);
                        }
                        else
                        {
                            GenerateBlock_Top(ref currentIndex, offset, vertices, normals, uvs, indices, BlockIDs[x, y, z]);
                        }

                        if (x < Size - 1)
                        {
                            if (BlockIDs[x + 1, y, z] == 0)
                                GenerateBlock_Right(ref currentIndex, offset, vertices, normals, uvs, indices, BlockIDs[x, y, z]);
                        }
                        else
                        {
                            GenerateBlock_Right(ref currentIndex, offset, vertices, normals, uvs, indices, BlockIDs[x, y, z]);
                        }

                        if (x >= 1)
                        {
                            if (BlockIDs[x - 1, y, z] == 0)
                                GenerateBlock_Left(ref currentIndex, offset, vertices, normals, uvs, indices, BlockIDs[x, y, z]);
                        }
                        else
                        {
                            GenerateBlock_Left(ref currentIndex, offset, vertices, normals, uvs, indices, BlockIDs[x, y, z]);
                        }

                        if (z < Size - 1)
                        {
                            if (BlockIDs[x, y, z + 1] == 0)
                                GenerateBlock_Forward(ref currentIndex, offset, vertices, normals, uvs, indices, BlockIDs[x, y, z]);
                        }
                        else
                        {
                            GenerateBlock_Forward(ref currentIndex, offset, vertices, normals, uvs, indices, BlockIDs[x, y, z]);
                        }

                        if (z >= 1)
                        {
                            if (BlockIDs[x, y, z - 1] == 0)
                                GenerateBlock_Back(ref currentIndex, offset, vertices, normals, uvs, indices, BlockIDs[x, y, z]);
                        }
                        else
                        {
                            GenerateBlock_Back(ref currentIndex, offset, vertices, normals, uvs, indices, BlockIDs[x, y, z]);
                        }

                        if (y >= 1)
                        {
                            if (BlockIDs[x, y - 1, z] == 0)
                                GenerateBlock_Bottom(ref currentIndex, offset, vertices, normals, uvs, indices, BlockIDs[x, y, z]);
                        }
                        else
                        {
                            GenerateBlock_Back(ref currentIndex, offset, vertices, normals, uvs, indices, BlockIDs[x, y, z]);
                        }
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

    void GenerateBlock_Top(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int ID)
    {
        vertices.Add(new Vector3(0f, 1f, 1f) + offset);
        vertices.Add(new Vector3(1f, 1f, 1f) + offset);
        vertices.Add(new Vector3(1f, 1f, 0f) + offset);
        vertices.Add(new Vector3(0f, 1f, 0f) + offset);

        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);

        if (ID != 2)
        {
            uvs.AddRange(_blocks.GetBlockUV(ID));
        }
        else
        {
            uvs.AddRange(_blocks.GrassTop());
        }

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    void GenerateBlock_Right(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int ID)
    {
        vertices.Add(new Vector3(1f, 1f, 0f) + offset);
        vertices.Add(new Vector3(1f, 1f, 1f) + offset);
        vertices.Add(new Vector3(1f, 0f, 1f) + offset);
        vertices.Add(new Vector3(1f, 0f, 0f) + offset);

        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        
        if (ID != 2)
        {
            uvs.AddRange(_blocks.GetBlockUV(ID));
        }
        else
        {
            uvs.AddRange(_blocks.GrassSide());
        }

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    void GenerateBlock_Left(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int ID)
    {
        vertices.Add(new Vector3(0f, 1f, 1f) + offset);
        vertices.Add(new Vector3(0f, 1f, 0f) + offset);
        vertices.Add(new Vector3(0f, 0f, 0f) + offset);
        vertices.Add(new Vector3(0f, 0f, 1f) + offset);

        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        
        if (ID != 2)
        {
            uvs.AddRange(_blocks.GetBlockUV(ID));
        }
        else
        {
            uvs.AddRange(_blocks.GrassSide());
        }

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    void GenerateBlock_Forward(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int ID)
    {
        vertices.Add(new Vector3(1f, 1f, 1f) + offset);
        vertices.Add(new Vector3(0f, 1f, 1f) + offset);
        vertices.Add(new Vector3(0f, 0f, 1f) + offset);
        vertices.Add(new Vector3(1f, 0f, 1f) + offset);

        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);

        if (ID != 2)
        {
            uvs.AddRange(_blocks.GetBlockUV(ID));
        }
        else
        {
            uvs.AddRange(_blocks.GrassSide());
        }

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    void GenerateBlock_Back(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int ID)
    {
        vertices.Add(new Vector3(0f, 1f, 0f) + offset);
        vertices.Add(new Vector3(1f, 1f, 0f) + offset);
        vertices.Add(new Vector3(1f, 0f, 0f) + offset);
        vertices.Add(new Vector3(0f, 0f, -0) + offset);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        if (ID != 2)
        {
            uvs.AddRange(_blocks.GetBlockUV(ID));
        }
        else
        {
            uvs.AddRange(_blocks.GrassSide());
        }

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    void GenerateBlock_Bottom(ref int currentIndex, Vector3Int offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, int ID)
    {
        vertices.Add(new Vector3(0f, 0f, 0f) + offset);
        vertices.Add(new Vector3(1f, 0f, 0f) + offset);
        vertices.Add(new Vector3(1f, 0f, 1f) + offset);
        vertices.Add(new Vector3(0f, 0f, 1f) + offset);

        normals.Add(Vector3.down);
        normals.Add(Vector3.down);
        normals.Add(Vector3.down);
        normals.Add(Vector3.down);

        if (ID != 2)
        {
            uvs.AddRange(_blocks.GetBlockUV(ID));
        }
        else
        {
            uvs.AddRange(_blocks.GrassBot());
        }

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
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
}


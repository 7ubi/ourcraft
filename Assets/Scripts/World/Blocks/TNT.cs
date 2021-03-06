using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class TNT : MonoBehaviour
{
    public int Radius { get; set; } = 4;
    public float ExplosionTime { get; set; } = 2f;
    public GameObject explosion;
    private float _time;
    public worldCreation worldCreation;
    public PlayerInventory playerInventory;
    public int Orientation { get; set; }

    private void Start()
    {
        var newMesh = new Mesh();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var indices = new List<int>();

        var currentIndex = 0;

        var b = worldCreation.Blocks[BlockTypes.TNT];

        var offset = new Vector3Int(0, 0, 0);

        worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
            b.blockShape.faceData[2], b.GETRect(b.topIndex), 2, Orientation);
        worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
            b.blockShape.faceData[5], b.GETRect(b.rightIndex), 5, Orientation);
        worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
            b.blockShape.faceData[4], b.GETRect(b.leftIndex), 4, Orientation);
        worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
            b.blockShape.faceData[1], b.GETRect(b.frontIndex), 1, Orientation);
        worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
            b.blockShape.faceData[0], b.GETRect(b.backIndex), 0, Orientation);
        worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
            b.blockShape.faceData[3], b.GETRect(b.botIndex), 3, Orientation);

        newMesh.SetVertices(vertices);
        newMesh.SetNormals(normals);
        newMesh.SetUVs(0, uvs);
        newMesh.SetIndices(indices, MeshTopology.Triangles, 0);

        newMesh.RecalculateTangents();
        GetComponent<MeshFilter>().mesh = newMesh;
        GetComponent<MeshCollider>().sharedMesh = newMesh;
    }

    private void Update()
    {
        _time += Time.deltaTime;

        var size = .9f + .1f * (float) Math.Sin(_time * 4);
        transform.localScale = new Vector3(size, size, size);
        
        if (_time >= ExplosionTime)
        {
            Explode();
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void Explode()
    {
        var blockInt = Vector3Int.FloorToInt(transform.position);
        
        var chunksToUpdate = new List<Chunck>();
        var chunksLoaded = new Dictionary<Vector2Int, Chunck>();

        var rand = new Random((int) Time.time);
        
        for (var x = -Radius; x < Radius; x++)
        {
            for (var y = -Radius; y < Radius; y++)
            {
                for (var z = -Radius; z < Radius; z++)
                {
                    var pos = blockInt + new Vector3Int(x, y, z);
                    if (!(Vector3Int.Distance(pos, blockInt) < Radius)) continue;
                    Chunck chunk = null;
                    var ch = new Vector2Int(Mathf.FloorToInt(pos.x / 16f) * 16
                        , Mathf.FloorToInt(pos.z / 16f) * 16);
                    
                    chunk = chunksLoaded.ContainsKey(ch) ? chunksLoaded[ch]
                        : worldCreation.GetChunck(pos);

                    var position = chunk.Pos;
                        
                    var bix = Mathf.FloorToInt(pos.x) - (int)position.x;
                    var biy = Mathf.FloorToInt(pos.y);
                    var biz = Mathf.FloorToInt(pos.z) - (int)position.z;
                        
                    if (chunk.BlockIDs[bix, biy, biz] == 0)
                        continue;

                    if (rand.NextDouble() < 0)
                    {
                        playerInventory.AddDestroyedBlock(worldCreation.CreateDestroyedBlock(
                        worldCreation.Blocks[chunk.BlockIDs[bix, biy, biz]].DropID,
                        pos + new Vector3(0.375f, 0.1f, 0.375f)));
                    }
                        
                    chunk.BlockIDs[bix, biy, biz] = 0;
                        
                        
                        
                    if (!chunksToUpdate.Contains(chunk))
                        chunksToUpdate.Add(chunk);
                        
                    if (bix == 0)
                    {
                        var c = worldCreation.GetChunck(new Vector3(-1, 0, 0) + pos);
                        if (!chunksToUpdate.Contains(c))
                            chunksToUpdate.Add(c);
                    }
        
                    if (bix == worldCreation.Size - 1)
                    {
                        var c = worldCreation.GetChunck(new Vector3(1, 0, 0) + pos);
                        if (!chunksToUpdate.Contains(c))
                            chunksToUpdate.Add(c);
                    }
        
                    if (biz == 0)
                    {
                        var c = worldCreation.GetChunck(new Vector3(0, 0, -1) + pos);
                        if (!chunksToUpdate.Contains(c))
                            chunksToUpdate.Add(c);
                    }
        
                    if (biz == worldCreation.Size - 1)
                    {
                        var c = worldCreation.GetChunck(new Vector3(0, 0, 1) + pos);
                        if (!chunksToUpdate.Contains(c))
                            chunksToUpdate.Add(c);
                    }
                    
                    
                }
            }
        }

        foreach (var chunk in chunksToUpdate)
        {
            chunk.GetComponent<MeshCreation>().Init();
        }
        Destroy(gameObject);
        Instantiate(explosion, transform.position, Quaternion.identity);
        
        
    }
}

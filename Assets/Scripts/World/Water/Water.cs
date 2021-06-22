using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

public class Water : MonoBehaviour
{
    [SerializeField] private GameObject water;
    public worldCreation worldCreation;
    [SerializeField] private float waterAnimTime = 1f;
    public MeshFilter _meshFilter;

    [SerializeField] public BlockShape topWater;
    [SerializeField] public BlockShape normalWater;
    
    // water shader https://www.youtube.com/watch?v=gRq-IdShxpU

    public int[,,] _waterIds;
    [SerializeField] private int height;
    private Vector2 _offset;
    private Chunck _chunck;
    

    public Mesh _newMesh;
    public List<Vector3> _vertices;
    public List<Vector3> _normals;
    public List<Vector2> _uvs;
    public List<int> _indices;
    public Vector3 _position;
    
    public bool CanGenerateMesh { get; set; } = true;

    public void CreateWater(Vector2 offset, Transform chunckParent)
    {
        _waterIds = new int[worldCreation.Size,  worldCreation.MAXHeight,worldCreation.Size];
        _offset = offset;
        _position = transform.position;
        
        _chunck = chunckParent.GetComponent<Chunck>();
        _meshFilter = water.GetComponent<MeshFilter>();
        
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
        
        if (worldCreation.GetBlock(block) != 0)
        {
            DistributeWater(block);
            yield break;
        }
        
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
        
        if (!worldCreation.meshesToUpdate.Contains(c))
            worldCreation.meshesToUpdate.Add(c);
       

        if (worldCreation.GetBlock(block + new Vector3(0, -1, 0)) != 0)
        {
            DistributeWater(block);
            yield break;
        }
        
        StartCoroutine(UpdateWaterDownEnumerator(block + new Vector3(0, -1, 0)));
    }

    public void DistributeWater(Vector3 block)
    {
        if (worldCreation.GetBlock(block + new Vector3(0, 0, 1)) == 0)
        {
            StartCoroutine(DistributeWaterEnumerator(block, new Vector3Int(0, 0, 1)));
        }
        
        if (worldCreation.GetBlock(block + new Vector3(0, 0, -1)) == 0)
        {
            StartCoroutine(DistributeWaterEnumerator(block, new Vector3Int(0, 0, -1)));
        }
        
        if (worldCreation.GetBlock(block + new Vector3(1, 0, 0)) == 0)
        {
            StartCoroutine(DistributeWaterEnumerator(block, new Vector3Int(1, 0, 0)));
        }
        
        if (worldCreation.GetBlock(block + new Vector3(-1, 0, 0)) == 0)
        {
            StartCoroutine(DistributeWaterEnumerator(block, new Vector3Int(-1, 0, 0)));
        }
    }

    private IEnumerator DistributeWaterEnumerator(Vector3 block, Vector3Int dir)
    {
        yield return new WaitForSeconds(waterAnimTime);
        

        if (worldCreation.GetBlock(block + dir) != 0) yield break;
        if (worldCreation.GetWater(block + dir) != 0) yield break;
        var c = worldCreation.GetChunck(block + dir);

        var pos = block + dir;
        
        var bix = Mathf.FloorToInt(pos.x) - (int)c.Pos.x;
        var biy = Mathf.FloorToInt(pos.y);
        var biz = Mathf.FloorToInt(pos.z) - (int)c.Pos.z;

        c.WaterIDs[bix, biy, biz] = 2;
        
        var cM = _chunck.GetComponent<MeshCreation>();
      
        if (!worldCreation.meshesToUpdate.Contains(cM))
            worldCreation.meshesToUpdate.Add(cM);
        
        //if(worldCreation.GetBlock(pos + new Vector3(0, -1, 0)) != 0) yield break;

        //UpdateWaterDown(pos + new Vector3(0, -1, 0));
    }

    public int Height => height;
}

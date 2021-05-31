using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

public class Water : MonoBehaviour
{
    [SerializeField] private GameObject waterChunck;
    public worldCreation worldCreation;
    [SerializeField] private WaterBlocks waterBlocks;
    [SerializeField] private float waterAnimTime = 1f;
    private MeshFilter _meshFilter;
    
    // water shader https://www.youtube.com/watch?v=gRq-IdShxpU

    private int[,,] _waterIds;
    [SerializeField] private int height;
    private Vector2 _offset;
    private Chunck _chunck;
    private GameObject _water;
    
    private Mesh _newMesh;
    private List<Vector3> _vertices;
    private List<Vector3> _normals;
    private List<Vector2> _uvs;
    private List<int> _indices;
    private Vector3 _position;
    
    public bool CanGenerateMesh { get; set; } = true;

    public void CreateWater(Vector2 offset, Transform chunckParent)
    {
        _waterIds = new int[worldCreation.Size,  worldCreation.MAXHeight,worldCreation.Size];
        _offset = offset;
        _position = transform.position;
         
        if (chunckParent.childCount > 0)
        {
            for (var i = chunckParent.childCount - 1; i >= 0; i--)
            {
                Destroy(chunckParent.GetChild(i).gameObject);
            }
        }
        
        _water = Instantiate(waterChunck, chunckParent);
        _chunck = chunckParent.GetComponent<Chunck>();
        _meshFilter = _water.GetComponent<MeshFilter>();
        
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
        
        if (worldCreation.GetBlock(block) != 0) yield break;
        
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
        c.ResetMesh();
        ResetMesh();
        c.GenerateMeshThreaded();
        StartCoroutine(UpdateWaterDownEnumerator(block + new Vector3(0, -1, 0)));
    }
    
    public void GenerateWater()
    {
        CanGenerateMesh = false;
        if (_chunck.WaterIDs != null)
            _waterIds = _chunck.WaterIDs;
        else
            _chunck.WaterIDs = _waterIds;
        var currentIndex = 0;

        for (var x = 0; x < worldCreation.Size; x++)
        {
            for(var y = 0; y < worldCreation.MAXHeight; y++)
            {
                for (var z = 0; z < worldCreation.Size; z++)
                {
                    var offset = new Vector3(x, y, z);
                    if (_waterIds[x, y, z] == 0) continue;
                    var isTop = false;
                    if (y <= worldCreation.MAXHeight - 1)
                    {
                        if (_waterIds[x, y + 1, z] == 0)
                        {
                            GenerateBlock_Top(ref currentIndex, offset, _vertices, _normals, _uvs, _indices);
                            GenerateBlock_BottomOfTop(ref currentIndex, offset, _vertices, _normals, _uvs, _indices);
                            isTop = true;
                        }
                    }
                    else
                    {
                        GenerateBlock_Top(ref currentIndex, offset, _vertices, _normals, _uvs, _indices);
                        GenerateBlock_BottomOfTop(ref currentIndex, offset, _vertices, _normals, _uvs, _indices);
                        isTop = true;
                    }
                    
                    if (x < worldCreation.Size - 1)
                    {
                        if (_waterIds[x + 1, y, z] == 0)
                            GenerateBlock_Right(ref currentIndex, offset, _vertices, _normals, _uvs, _indices, isTop);
                    }
                    else
                    {
                        if (worldCreation.GetWater(new Vector3(x + _position.x + 1, y + _position.y,
                            z + _position.z)) == 0)
                        {
                            if (worldCreation.GetBlock(new Vector3(x + _position.x + 1, y + _position.y,
                                z + _position.z)) == 0)
                            {
                                GenerateBlock_Right(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                    isTop);
                            }
                            else if (worldCreation
                                .Blocks[
                                    worldCreation.GetBlock(new Vector3(x + _position.x + 1, y + _position.y,
                                        z + _position.z))].isTransparent)
                                GenerateBlock_Right(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                    isTop);
                        }
                    }

                    if (x >= 1)
                    {
                        if (_waterIds[x - 1, y, z] == 0)
                            GenerateBlock_Left(ref currentIndex, offset, _vertices, _normals, _uvs, _indices, isTop);
                    }
                    else
                    {
                        if (worldCreation.GetWater(new Vector3(x + _position.x - 1, y + _position.y,
                            z + _position.z)) == 0)
                        {
                            if (worldCreation.GetBlock(new Vector3(x + _position.x - 1, y + _position.y,
                                z + _position.z)) == 0)
                                GenerateBlock_Left(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                    isTop);
                            else if (worldCreation
                                .Blocks[
                                    worldCreation.GetBlock(new Vector3(x + _position.x - 1, y + _position.y,
                                        z + _position.z))].isTransparent)
                                GenerateBlock_Left(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                    isTop);
                        }
                    }

                    if (z < worldCreation.Size - 1)
                    {
                        if (_waterIds[x, y, z + 1] == 0)
                            GenerateBlock_Forward(ref currentIndex, offset, _vertices, _normals, _uvs, _indices, isTop);
                    }
                    else
                    {
                        if (worldCreation.GetWater(new Vector3(x + _position.x, y + _position.y,
                            z + _position.z + 1)) == 0)
                        {
                            if (worldCreation.GetBlock(new Vector3(x + _position.x, y + _position.y,
                                z + _position.z + 1)) == 0)
                                GenerateBlock_Forward(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                    isTop);
                            else if (worldCreation
                                .Blocks[
                                    worldCreation.GetBlock(new Vector3(x + _position.x, y + _position.y,
                                        z + _position.z + 1))].isTransparent)
                                GenerateBlock_Forward(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                    isTop);
                        }
                    }

                    if (z >= 1)
                    {
                        if (_waterIds[x, y, z - 1] == 0)
                            GenerateBlock_Back(ref currentIndex, offset, _vertices, _normals, _uvs, _indices, isTop);
                    }
                    else
                    {
                        if (worldCreation.GetWater(new Vector3(x + _position.x, y + _position.y,
                            z + _position.z - 1)) == 0)
                        {
                            if (worldCreation.GetBlock(new Vector3(x + _position.x, y + _position.y,
                                z + _position.z - 1)) == 0)
                                GenerateBlock_Back(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                    isTop);
                            else if (worldCreation
                                .Blocks[
                                    worldCreation.GetBlock(new Vector3(x + _position.x, y + _position.y,
                                        z + _position.z - 1))].isTransparent)
                                GenerateBlock_Back(ref currentIndex, offset, _vertices, _normals, _uvs, _indices,
                                    isTop);
                        }
                    }
                    
                    if (y != 0)
                    {
                        if (_waterIds[x, y - 1, z] == 0)
                            GenerateBlock_Bottom(ref currentIndex, offset, _vertices, _normals, _uvs, _indices);
                    }
                }
            }
        }

        CanGenerateMesh = true;
        worldCreation.waterMeshesToApply.Add(this);
    }
    
    public void ApplyMesh()
    {
        _newMesh.SetVertices(_vertices);
        _newMesh.SetNormals(_normals);
        _newMesh.SetUVs(0, _uvs);
        _newMesh.SetIndices(_indices, MeshTopology.Triangles, 0);

        _newMesh.RecalculateTangents();
        _meshFilter.mesh = _newMesh;
    }

    private void GenerateBlock_Top(ref int currentIndex, Vector3 offset, List<Vector3> vertices,
        List<Vector3> normals, List<Vector2> uvs, List<int> indices)
    {
        vertices.Add(new Vector3(0f, 0.9f, 1f) + offset);
        vertices.Add(new Vector3(1f, 0.9f, 1f) + offset);
        vertices.Add(new Vector3(1f, 0.9f, 0f) + offset);
        vertices.Add(new Vector3(0f, 0.9f, 0f) + offset);

        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }
    
    private void GenerateBlock_BottomOfTop(ref int currentIndex, Vector3 offset, List<Vector3> vertices,
        List<Vector3> normals, List<Vector2> uvs, List<int> indices)
    {
        vertices.Add(new Vector3(0f, 0.9f, 0f) + offset);
        vertices.Add(new Vector3(1f, 0.9f, 0f) + offset);
        vertices.Add(new Vector3(1f, 0.9f, 1f) + offset);
        vertices.Add(new Vector3(0f, 0.9f, 1f) + offset);

        normals.Add(Vector3.down);
        normals.Add(Vector3.down);
        normals.Add(Vector3.down);
        normals.Add(Vector3.down);

        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }
    
    private void GenerateBlock_Right(ref int currentIndex, Vector3 offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, bool isTop)
    {
        vertices.Add(new Vector3(1f, waterBlocks.GetSideHeight(isTop), 0f) + offset);
        vertices.Add(new Vector3(1f, waterBlocks.GetSideHeight(isTop), 1f) + offset);
        vertices.Add(new Vector3(1f, 0f, 1f) + offset);
        vertices.Add(new Vector3(1f, 0f, 0f) + offset);

        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        normals.Add(Vector3.right);
        
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    private void GenerateBlock_Left(ref int currentIndex, Vector3 offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, bool isTop)
    {
        vertices.Add(new Vector3(0f, waterBlocks.GetSideHeight(isTop), 1f) + offset);
        vertices.Add(new Vector3(0f, waterBlocks.GetSideHeight(isTop), 0f) + offset);
        vertices.Add(new Vector3(0f, 0f, 0f) + offset);
        vertices.Add(new Vector3(0f, 0f, 1f) + offset);

        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        normals.Add(Vector3.left);
        
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    private void GenerateBlock_Forward(ref int currentIndex, Vector3 offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, bool isTop)
    {
        vertices.Add(new Vector3(1f, waterBlocks.GetSideHeight(isTop), 1f) + offset);
        vertices.Add(new Vector3(0f, waterBlocks.GetSideHeight(isTop), 1f) + offset);
        vertices.Add(new Vector3(0f, 0f, 1f) + offset);
        vertices.Add(new Vector3(1f, 0f, 1f) + offset);

        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);

        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    private void GenerateBlock_Back(ref int currentIndex, Vector3 offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices, bool isTop)
    {
        vertices.Add(new Vector3(0f, waterBlocks.GetSideHeight(isTop), 0f) + offset);
        vertices.Add(new Vector3(1f, waterBlocks.GetSideHeight(isTop), 0f) + offset);
        vertices.Add(new Vector3(1f, 0f, 0f) + offset);
        vertices.Add(new Vector3(0f, 0f, -0) + offset);

        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);
        normals.Add(Vector3.back);

        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));
        
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    private void GenerateBlock_Bottom(ref int currentIndex, Vector3 offset, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> indices)
    {
        vertices.Add(new Vector3(0f, 0f, 0f) + offset);
        vertices.Add(new Vector3(1f, 0f, 0f) + offset);
        vertices.Add(new Vector3(1f, 0f, 1f) + offset);
        vertices.Add(new Vector3(0f, 0f, 1f) + offset);

        normals.Add(Vector3.down);
        normals.Add(Vector3.down);
        normals.Add(Vector3.down);
        normals.Add(Vector3.down);

        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 0));

        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 1);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 0);
        indices.Add(currentIndex + 2);
        indices.Add(currentIndex + 3);
        currentIndex += 4;
    }

    public int Height => height;
}

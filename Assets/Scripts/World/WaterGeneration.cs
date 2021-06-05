using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class WaterGeneration : MonoBehaviour
{
    public worldCreation worldCreation;
    [SerializeField] public Water water;
    [SerializeField] public Chunck chunck;
    
    public void GenerateWater()
    {
        water.CanGenerateMesh = false;
        if (chunck.WaterIDs != null)
            water._waterIds = chunck.WaterIDs;
        else
            chunck.WaterIDs = water._waterIds;
        var currentIndex = 0;

        for (var x = 0; x < worldCreation.Size; x++)
        {
            for(var y = 0; y < worldCreation.MAXHeight; y++)
            {
                for (var z = 0; z < worldCreation.Size; z++)
                {
                    var offset = new Vector3Int(x, y, z);
                    if (water._waterIds[x, y, z] == 0) continue;

                    var shape = water.normalWater;
                    
                    if (y <= worldCreation.MAXHeight - 1)
                    {
                        if (water._waterIds[x, y + 1, z] == 0)
                        {
                            if (water._waterIds[x, y, z] == 1)
                            {
                                shape = water.topWater;
                            }

                            worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                                water. _normals, water._uvs, water._indices, shape.faceData[2]);
                            worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                                water._normals, water._uvs, water._indices, shape.faceData[6]);
                        }
                    }
                    else
                    {
                        if (water._waterIds[x, y, z] == 1)
                        {
                            shape = water.topWater;
                        }
                        
                        worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                            water._normals, water._uvs, water._indices, shape.faceData[2]);
                        worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                            water._normals, water._uvs, water._indices, shape.faceData[6]);
                    }

                    if (x < worldCreation.Size - 1)
                    {
                        if (water._waterIds[x + 1, y, z] == 0)
                            worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                                water._normals, water._uvs, water._indices, shape.faceData[5]);
                    }
                    else
                    {
                        if (worldCreation.GetWater(new Vector3(x + water._position.x + 1, y + water._position.y,
                            z + water._position.z)) == 0)
                        {
                            if (worldCreation.GetBlock(new Vector3(x + water._position.x + 1, y + water._position.y,
                                z + water._position.z)) == 0)
                            {
                                worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                                    water._normals, water._uvs, water._indices, shape.faceData[5]);
                            }
                            else if (worldCreation.Blocks[
                                    worldCreation.GetBlock(new Vector3(x + water._position.x + 1, y + water._position.y,
                                        z + water._position.z))].isTransparent)
                                worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                                    water._normals, water._uvs, water._indices, shape.faceData[5]);
                        }
                    }

                    if (x >= 1)
                    {
                        if (water._waterIds[x - 1, y, z] == 0)
                            worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                                water._normals, water._uvs, water._indices, shape.faceData[4]);
                    }
                    else
                    {
                        if (worldCreation.GetWater(new Vector3(x + water._position.x - 1, y + water._position.y,
                            z + water._position.z)) == 0)
                        {
                            if (worldCreation.GetBlock(new Vector3(x + water._position.x - 1, y + water._position.y,
                                z + water._position.z)) == 0)
                                worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                                    water._normals, water._uvs, water._indices, shape.faceData[4]);
                            else if (worldCreation
                                .Blocks[
                                    worldCreation.GetBlock(new Vector3(x + water._position.x - 1, y + water._position.y,
                                        z + water._position.z))].isTransparent)
                                worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                                    water._normals, water._uvs, water._indices, shape.faceData[4]);
                        }
                    }

                    if (z < worldCreation.Size - 1)
                    {
                        if (water._waterIds[x, y, z + 1] == 0)
                            worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                                water._normals, water._uvs, water._indices, shape.faceData[1]);
                    }
                    else
                    {
                        if (worldCreation.GetWater(new Vector3(x + water._position.x, y + water._position.y,
                            z + water._position.z + 1)) == 0)
                        {
                            if (worldCreation.GetBlock(new Vector3(x + water._position.x, y + water._position.y,
                                z + water._position.z + 1)) == 0)
                                worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                                    water._normals, water._uvs, water._indices, shape.faceData[1]);
                            else if (worldCreation
                                .Blocks[
                                    worldCreation.GetBlock(new Vector3(x + water._position.x, y + water._position.y,
                                        z + water._position.z + 1))].isTransparent)
                                worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                                    water._normals, water._uvs, water._indices, shape.faceData[1]);
                        }
                    }

                    if (z >= 1)
                    {
                        if (water._waterIds[x, y, z - 1] == 0)
                            worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                                water._normals, water._uvs, water._indices, shape.faceData[0]);
                    }
                    else
                    {
                        if (worldCreation.GetWater(new Vector3(x + water._position.x, y + water._position.y,
                            z + water._position.z - 1)) == 0)
                        {
                            if (worldCreation.GetBlock(new Vector3(x + water._position.x, y + water._position.y,
                                z + water._position.z - 1)) == 0)
                                worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                                    water._normals, water._uvs, water._indices, shape.faceData[0]);
                            else if (worldCreation
                                .Blocks[
                                    worldCreation.GetBlock(new Vector3(x + water._position.x, y + water._position.y,
                                        z + water._position.z - 1))].isTransparent)
                                worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                                    water._normals, water._uvs, water._indices, shape.faceData[0]);
                        }
                    }
                    
                    if (y != 0)
                    {
                        if (water._waterIds[x, y - 1, z] == 0)
                            worldCreation.blockCreation.GenerateWaterBlock(ref currentIndex, offset, water._vertices,
                                water._normals, water._uvs, water._indices, shape.faceData[3]);
                    }
                }
            }
        }

        water.CanGenerateMesh = true;
        if (!worldCreation.waterMeshesToApply.Contains(this))
        {
            worldCreation.waterMeshesToApply.Add(this);
        }
    }
    
    public void ApplyMesh()
    {
        water._newMesh.SetVertices(water._vertices);
        water._newMesh.SetNormals(water._normals);
        water._newMesh.SetUVs(0, water._uvs);
        water._newMesh.SetIndices(water._indices, MeshTopology.Triangles, 0);

        water._newMesh.RecalculateTangents();
        water._meshFilter.mesh = water._newMesh;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Voxelizer : MonoBehaviour
{
    public Mesh SpriteToVoxel(Texture2D texture2D, BlockShape shape, BlockCreation blockCreation)
    {
        var currentIndex = 0;
        var mesh = new Mesh();
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var colors = new List<Color32>();
        var indices = new List<int>();
        var uvs = new List<Vector2>();


        for (var x = 0; x < texture2D.width; x++)
        {
            for (var y = 0; y < texture2D.height; y++)
            {
                var offset = new Vector3Int(x, y, 0);
                var color = texture2D.GetPixel(x, y);
                
                if (color.a == 0)
                    continue;
                
                blockCreation.GenerateSpriteToVoxel(ref currentIndex, offset, vertices, normals, colors, 
                    color, uvs, indices,shape.faceData[2]);
                blockCreation.GenerateSpriteToVoxel(ref currentIndex, offset, vertices, normals, colors,
                    color, uvs, indices, shape.faceData[5]);
                blockCreation.GenerateSpriteToVoxel(ref currentIndex, offset, vertices, normals, colors,
                    color, uvs, indices, shape.faceData[4]);
                blockCreation.GenerateSpriteToVoxel(ref currentIndex, offset, vertices, normals, colors,
                    color, uvs, indices, shape.faceData[1]);
                blockCreation.GenerateSpriteToVoxel(ref currentIndex, offset, vertices, normals, colors,
                    color, uvs, indices, shape.faceData[0]);
                blockCreation.GenerateSpriteToVoxel(ref currentIndex, offset, vertices, normals, colors,
                    color, uvs, indices, shape.faceData[3]);
            }
        }
        
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetColors(colors);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);

        mesh.RecalculateTangents();
        
        return mesh;
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ChunckInfo
{
    public Vector3 pos;
    public byte[] blockIDs;
    public FurnaceInfo[] furnaceInfos;
    private int _size = 16;
    private int _maxHeight = 256;
    
    public ChunckInfo(Vector3 pos, int[,,] blockIDsChunck, int[,,] waterIDsChunck, Dictionary<Vector3Int, Furnace> furnaces)
    {
        this.pos = pos;

        var blockIds = new byte[_size * _maxHeight * _size];

        for (var x = 0; x < _size; x++)
        {
            for (var y = 0; y < _maxHeight; y++)
            {
                for (var z = 0; z < _size; z++)
                {
                    if (waterIDsChunck != null)
                    {
                        if (waterIDsChunck[x, y, z] != 0)
                        {
                            blockIds[x * _maxHeight * _size + y * _size + z] = (byte) (255 - waterIDsChunck[x, y, z]);
                        }
                        else
                        {
                            blockIds[x * _maxHeight * _size + y * _size + z] = (byte) blockIDsChunck[x, y, z];
                        }
                    }
                    else
                    {
                        blockIds[x * _maxHeight * _size + y * _size + z] = (byte) blockIDsChunck[x, y, z];
                    }
                }
            }
        }

        this.blockIDs = blockIds;

        furnaceInfos = new FurnaceInfo[furnaces.Count];
        var furn = furnaces.ToArray();
        for (var i = 0; i < furnaceInfos.Length; i++)
        {
            furnaceInfos[i] = new FurnaceInfo(furn[i].Value);
        }
    }
}

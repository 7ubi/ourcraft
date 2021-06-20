﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ChunckInfo
{
    public Vector3 pos;
    public byte[] blockIDs;
    public int[] blockCount;
    public FurnaceInfo[] furnaceInfos;
    private int _size = 16;
    private int _maxHeight = 256;
    
    public ChunckInfo(Vector3 pos, int[,,] blockIDsChunck, int[,,] waterIDsChunck, Dictionary<Vector3Int, Furnace> furnaces)
    {
        this.pos = pos;

        var blockIdsList = new List<byte>();
        var blockCountList = new List<int>();

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
                            if (blockIdsList.Count > 2)
                            {
                                if (blockIdsList[blockIdsList.Count - 1] == (255 - waterIDsChunck[x, y, z]))
                                {
                                    blockCountList[blockCountList.Count - 1] += 1;
                                }
                                else
                                {
                                    blockIdsList.Add((byte)(255 - waterIDsChunck[x, y, z]));
                                    blockCountList.Add(1);
                                }
                            }
                            else
                            {
                                blockIdsList.Add((byte)(255 - waterIDsChunck[x, y, z]));
                                blockCountList.Add(1);
                            }
                        }
                        else
                        {
                            if (blockIdsList.Count > 2)
                            {
                                if (blockIdsList[blockIdsList.Count - 1] == blockIDsChunck[x, y, z])
                                {
                                    blockCountList[blockCountList.Count - 1] += 1;
                                }else
                                {
                                    blockIdsList.Add((byte) blockIDsChunck[x, y, z]);
                                    blockCountList.Add(1);
                                }
                            }
                            else
                            {
                                blockIdsList.Add((byte) blockIDsChunck[x, y, z]);
                                blockCountList.Add(1);
                            }
                        }
                    }
                    else
                    {
                        if (blockIdsList.Count > 2)
                        {
                            if (blockIdsList[blockIdsList.Count - 1] == blockIDsChunck[x, y, z])
                            {
                                blockCountList[blockCountList.Count - 1] += 1;
                            }else
                            {
                                blockIdsList.Add((byte) blockIDsChunck[x, y, z]);
                                blockCountList.Add(1);
                            }
                        }
                        else
                        {
                            blockIdsList.Add((byte) blockIDsChunck[x, y, z]);
                            blockCountList.Add(1);
                        }
                    }
                }
            }
        }
        
        blockIDs = blockIdsList.ToArray();
        blockCount = blockCountList.ToArray();
        
        furnaceInfos = new FurnaceInfo[furnaces.Count];
        var furn = furnaces.ToArray();
        for (var i = 0; i < furnaceInfos.Length; i++)
        {
            furnaceInfos[i] = new FurnaceInfo(furn[i].Value);
        }
    }
}

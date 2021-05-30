using UnityEngine;

[System.Serializable]
public class ChunckInfo
{
    public Vector3 pos;
    public byte[] blockIDs;
    public byte[] waterIDs;
    private int _size = 16;
    private int _maxHeight = 256;
    
    public ChunckInfo(Vector3 pos, int[,,] blockIDsChunck, int[,,] waterIDsChunck)
    {
        this.pos = pos;

        var blockIds = new byte[_size * _maxHeight * _size];

        for (var x = 0; x < _size; x++)
        {
            for (var y = 0; y < _maxHeight; y++)
            {
                for (var z = 0; z < _size; z++)
                {
                    blockIds[x * _maxHeight * _size + y * _size + z] = (byte)blockIDsChunck[x, y, z];
                }
            }
        }

        var waterIds = new byte[_size * _maxHeight * _size];

        for (var x = 0; x < _size; x++)
        {
            for (var y = 0; y < _maxHeight; y++)
            {
                for (var z = 0; z < _size; z++)
                {
                    waterIds[x * _maxHeight * _size + y * _size + z] = (byte)waterIDsChunck[x, y, z];
                }
            }
        }

        this.blockIDs = blockIds;
        this.waterIDs = waterIds;
        
    }
}

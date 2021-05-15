using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTypes : MonoBehaviour
{
    private const short dirt = 1;
    private const short grass = 2;
    private const short stone = 3;
    private const short log = 4;
    private const short leave = 5;

    public short Dirt => dirt;

    public short Grass => grass;

    public short Stone => stone;

    public short Log => log;

    public short Leave => leave;
}

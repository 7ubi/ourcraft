using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBlocks : MonoBehaviour
{
    public float GetSideHeight(bool isTopBlock)
    {
        return isTopBlock ? 0.9f : 1f;
    }
}

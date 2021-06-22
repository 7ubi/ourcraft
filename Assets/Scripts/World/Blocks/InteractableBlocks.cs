using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class InteractableBlocks : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerFurnace playerFurnace;
    [SerializeField] private GameObject tntPrefab;
    [SerializeField] private GameObject explosion;
    private worldCreation _worldCreation;

    private void Start()
    {
        _worldCreation = GetComponent<worldCreation>();
    }

    public void Furnace(Furnace furnace, Chunck chunck)
    {   
        playerInventory.OpenFurnace();
        playerFurnace.SetFurnace(furnace, chunck);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void Explode(int radius, Vector3 block)
    {
        var tnt = Instantiate(tntPrefab, block + new Vector3(-.5f, 0f, -.5f), Quaternion.identity);
        
        var t = tnt.GetComponent<TNT>();
        
        var chunk = _worldCreation.GetChunck(block);
        var position = chunk.transform.position;
                        
        var bix = Mathf.FloorToInt(block.x) - (int)position.x;
        var biy = Mathf.FloorToInt(block.y);
        var biz = Mathf.FloorToInt(block.z) - (int)position.z;

        chunk.BlockIDs[bix, biy, biz] = 0;
        _worldCreation.ReloadChunck(block);
        
        t.Radius = radius;
        t.ExplosionTime = 2f;
        t.worldCreation = _worldCreation;
        t.explosion = explosion;
        t.playerInventory = playerInventory;
    }
}

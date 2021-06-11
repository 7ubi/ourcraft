using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableBlocks : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerFurnace playerFurnace;
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
}

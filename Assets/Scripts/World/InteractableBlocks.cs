using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableBlocks : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;

    public void Interact(int id)
    {
        if (id == BlockTypes.Furnace)
        {
            Furnace();
        }
    }

    private void Furnace()
    {   
        playerInventory.OpenFurnace();
    }
}

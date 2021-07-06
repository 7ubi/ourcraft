using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryIndex : MonoBehaviour, IPointerClickHandler 
{
    public PlayerInventory playerInventory;
    [SerializeField] public Image img;
    
    public int Index { get; set; }


    public void OnPointerClick(PointerEventData data)
    {
        var mouseBtn = data.button == PointerEventData.InputButton.Left ? 0 : 1;
        playerInventory.ReplaceItem(Index, mouseBtn);
    }
}

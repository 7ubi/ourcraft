using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryIndex : MonoBehaviour, IPointerClickHandler 
{
    private PlayerInventory _playerInventory;
    [SerializeField] public Image img;
    
    public int Index { get; set; }

    private void Start()
    {
        _playerInventory = FindObjectOfType<PlayerInventory>().GetComponent<PlayerInventory>();
    }

    public void OnPointerClick(PointerEventData data)
    {
        _playerInventory.ReplaceItem(Index);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private RectTransform selector;
    private int _current = 0;

    [SerializeField] private TMP_Text[] itemCountText;
    private int[] itemCount = new int[9];
    
    private void Update()
    {
        _current = (_current - (int)Input.mouseScrollDelta.y) % 9;
        if (_current < 0)
            _current = 8;
        
        selector.localPosition = new Vector2(-400 + _current * 100, selector.localPosition.y);
    }

    public void AddItem(int id, int amount)
    {
        itemCount[id - 1] += amount;
        UpdateText(id - 1);
    }

    public bool CanPlaceBlock(int id)
    {
        return itemCount[id - 1] > 0;
    }

    private void UpdateText(int i)
    {
        itemCountText[i].text = "" + itemCount[i];
    }

    public int Current => _current;
}

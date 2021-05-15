using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private RectTransform selector;
    private int _current = 0;

    [SerializeField] private BlockTypes _blockTypes;

    [SerializeField] private Sprite[] icons;
    [SerializeField] private TMP_Text[] itemCountText;
    [SerializeField] private Image[] itemImages;
    private int[] itemCount = new int[9];
    private int[] itemIds = new int[9];


    private void Update()
    {
        _current = (_current - (int)Input.mouseScrollDelta.y) % 9;
        if (_current < 0)
            _current = 8;
        
        selector.localPosition = new Vector2(-400 + _current * 100, selector.localPosition.y);
    }

    public void AddItem(int id, int amount)
    {
        var index = -1;
        
        for (var i = 0; i < itemIds.Length; i++)
        {
            if (itemIds[i] != id) continue;
            index = i;
            break;
        }

        if (index == -1)
        {
            for (var i = 0; i < itemIds.Length; i++)
            {
                if (itemIds[i] != 0) continue;
                itemIds[i] = id;
                index = i;
                itemImages[i].sprite = icons[id - 1];
                itemImages[i].color = new Color(255, 255, 255, 100);
                break;
            }
        }

        itemCount[index] += amount;
        UpdateText(index);

        if (itemCount[index] > 0) return;
        itemIds[index] = 0;
        itemImages[index].sprite = null;
        itemImages[index].color = new Color(255, 255, 255, 0);
    }

    public bool CanPlaceBlock()
    {
        return itemIds[_current] != 0;
    }

    private void UpdateText(int i)
    {
        itemCountText[i].text = "" + itemCount[i];
    }

    public int Current => _current;

    public int[] ItemIds => itemIds;
}

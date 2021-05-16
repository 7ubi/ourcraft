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

    [SerializeField] private BlockTypes _blockTypes;

    [SerializeField] private Sprite[] icons;
    [SerializeField] private TMP_Text[] itemCountText;
    [SerializeField] private Image[] itemImages;


    private void Update()
    {
        Current = (Current - (int)Input.mouseScrollDelta.y) % 9;
        if (Current < 0)
            Current = 8;
        
        selector.localPosition = new Vector2(-400 + Current * 100, selector.localPosition.y);
    }

    public void AddItem(int id, int amount)
    {
        var index = -1;
        
        for (var i = 0; i < ItemIds.Length; i++)
        {
            if (ItemIds[i] != id) continue;
            index = i;
            break;
        }

        if (index == -1)
        {
            for (var i = 0; i < ItemIds.Length; i++)
            {
                if (ItemIds[i] != 0) continue;
                ItemIds[i] = id;
                index = i;
                itemImages[i].sprite = icons[id - 1];
                itemImages[i].color = new Color(255, 255, 255, 100);
                break;
            }
        }

        ItemCount[index] += amount;
        UpdateText(index);

        if (ItemCount[index] > 0) return;
        ItemIds[index] = 0;
        itemImages[index].sprite = null;
        itemImages[index].color = new Color(255, 255, 255, 0);
    }

    public bool CanPlaceBlock()
    {
        return ItemIds[Current] != 0;
    }

    private void UpdateText(int i)
    {
        itemCountText[i].text = "" + ItemCount[i];
    }

    public void LoadData(int[] itemCount, int[] itemIds)
    {
        this.ItemCount = itemCount;
        this.ItemIds = itemIds;

        UpdateUI();
    }

    private void UpdateUI()
    {
        for (var i = 0; i < ItemCount.Length; i++)
        {
            itemCountText[i].text = "" +  ItemCount[i];
            if (ItemIds[i] == 0) continue;

            itemImages[i].sprite = icons[ItemIds[i] - 1];
            
            itemImages[i].color = new Color(255, 255, 255, 100);
        }
    }

    public int Current { get; private set; } = 0;

    public int[] ItemIds { get; private set; } = new int[9];

    public int[] ItemCount { get; private set; } = new int[9];
}

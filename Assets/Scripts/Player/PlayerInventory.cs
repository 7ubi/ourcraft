using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private RectTransform selector;
    [SerializeField] private worldCreation worldCreation;
    [SerializeField] private GameObject canvas;

    [SerializeField] private InventoryCell itemCell;
    [SerializeField] private GameObject cell;
    [SerializeField] private Transform cellParent;
    private const int Rows = 9;
    private const int Cols = 4;
    public bool InInventory { get; set; } = false;
    
    [SerializeField] private int xStartInventory;
    [SerializeField] private int yStartInventory;
    [SerializeField] private int xDistInventory;
    [SerializeField] private int yDistInventory;

    [SerializeField] private GameObject[] itemGameObjects = new GameObject[5 * 9];
    [SerializeField] private TMP_Text[] itemCountText = new TMP_Text[5 * 9];
    [SerializeField] private Image[] itemImages = new Image[5 * 9];

    [SerializeField] private int destroyedBlockReach;

    private readonly List<GameObject> _destroyedBlocks = new List<GameObject>();
    
    public int Current { get; private set; } = 0;

    public int[] ItemIds { get; private set; } = new int[45];
    public int[] ItemCount { get; private set; } = new int[45];

    private void Start()
    {
        for (var x = 0; x < Rows; x++)
        {
            for (var y = 0; y < Cols; y++)
            {
                var newInv = Instantiate(cell, cell.transform.position, cell.transform.rotation);

                newInv.transform.SetParent(cellParent, false);
                newInv.GetComponent<RectTransform>().localPosition = new Vector3(xStartInventory + xDistInventory * x - 40
                    ,yStartInventory + yDistInventory * y, 0);
                itemGameObjects[y * Rows + x + 8 +1] = newInv;
                itemCountText[y * Rows + x + 8 + 1] = newInv.GetComponentInChildren<TMP_Text>();
                itemImages[y * Rows + x + 8 + 1] = newInv.GetComponent<InventoryIndex>().img;
            }
        }

        for (var i = 0; i < itemGameObjects.Length; i++)
        {
            itemGameObjects[i].GetComponent<InventoryIndex>().Index = i;
        }

        UpdateUI();
    }

    private void Update()
    {
        DestroyedBlockInRage();
        
        Current = (Current - (int)Input.mouseScrollDelta.y) % 9;
        if (Current < 0)
            Current = 8;
        
        selector.localPosition = new Vector2(-400 + Current * 100, selector.localPosition.y);


        if (!Input.GetKeyDown(KeyCode.E)) return;
        cellParent.gameObject.SetActive(!cellParent.gameObject.activeInHierarchy);
        if(InInventory)
            itemCell.ResetToOriginalPos();
        InInventory = !InInventory;
        Cursor.visible = InInventory;
        Cursor.lockState = InInventory ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void AddItem(int id, int amount)
    {
        var index = -1;
        
        for (var i = 0; i < ItemIds.Length; i++)
        {
            if (ItemIds[i] != id) continue;
            if (ItemCount[i] >= worldCreation.Blocks[ItemIds[i]].stackSize) continue;
            
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
                itemImages[i].sprite = worldCreation.Blocks[id].img;
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

    public void RemoveItem(int index)
    {
        ItemCount[index] -= 1;

        if (ItemCount[index] <= 0)
        {
            ItemIds[index] = 0;
            itemImages[index].sprite = null;
            itemImages[index].color = new Color(255, 255, 255, 0);
        }
        
        UpdateUI();
    }

    public bool CanPlaceBlock()
    {
        return ItemIds[Current] != 0;
    }

    private void UpdateText(int i)
    {
        if (ItemCount[i] == 0)
        {
            itemCountText[i].text =  "";
        }
        else
        {
            itemCountText[i].text = "" + ItemCount[i];
        }
    }

    public void LoadData(int[] itemCount, int[] itemIds)
    {
        this.ItemCount = itemCount;
        this.ItemIds = itemIds;
    }

    public void SetItem(int i, int id, int count)
    {
        itemImages[i].sprite = worldCreation.Blocks[id].img;
        itemImages[i].color = new Color(255, 255, 255, 100);
        ItemCount[i] = count;
        ItemIds[i] = id;
        
        UpdateText(i);
    }

    private void UpdateUI()
    {
        for (var i = 0; i < ItemCount.Length; i++)
        {
            UpdateText(i);
            if (ItemIds[i] == 0) continue;
            
            itemImages[i].sprite = worldCreation.Blocks[ItemIds[i]].img;
            
            itemImages[i].color = new Color(255, 255, 255, 100);
        }
    }

    public void AddDestroyedBlock(GameObject block)
    {
        _destroyedBlocks.Add(block);
    }

    public void ReplaceItem(int index, int mouseBtn)
    {
        var id = 0;
        var count = 0;
        var changed = false;
        if (mouseBtn == 0)
        {
            if (itemCell.Id == ItemIds[index])
            {
                ItemCount[index] += itemCell.Count;

                if (ItemCount[index] <= worldCreation.Blocks[ItemIds[index]].stackSize)
                {
                    itemCell.Reset();
                }
                else
                {
                    var dCount = ItemCount[index] % worldCreation.Blocks[ItemIds[index]].stackSize;
                    ItemCount[index] -= dCount;
                    itemCell.Count = dCount;
                    itemCell.UpdateText();
                }

                UpdateText(index);
                return;
            }


            if (ItemIds[index] != 0)
            {
                id = itemCell.Id;
                count = itemCell.Count;
                itemCell.SetSprite(worldCreation.Blocks[ItemIds[index]].img, ItemIds[index], ItemCount[index], index);


                ItemIds[index] = 0;
                itemImages[index].sprite = null;
                itemImages[index].color = new Color(255, 255, 255, 0);
                ItemCount[index] = 0;
                UpdateText(index);
                changed = true;

            }

            if (!changed)
            {
                if (itemCell.Id == 0) return;
                ItemIds[index] = itemCell.Id;
                itemImages[index].sprite = worldCreation.Blocks[ItemIds[index]].img;
                ItemCount[index] = itemCell.Count;
                itemImages[index].color = new Color(255, 255, 255, 100);
                itemCell.Reset();
                UpdateText(index);
            }
            else
            {
                if (id == 0) return;
                ItemIds[index] = id;
                itemImages[index].sprite = worldCreation.Blocks[id].img;
                ItemCount[index] = count;
                itemImages[index].color = new Color(255, 255, 255, 100);
                UpdateText(index);
            }
        }
        else
        {
            if (itemCell.Id == 0)
            {
                if (ItemIds[index] == 0) return;
                var c = ItemCount[index];
                ItemCount[index] /= 2;
                
                itemCell.SetSprite(worldCreation.Blocks[ItemIds[index]].img, ItemIds[index], c - ItemCount[index], index);
                
                if (ItemCount[index] == 0)
                {
                    ItemIds[index] = 0;
                    itemImages[index].sprite = null;
                    itemImages[index].color = new Color(255, 255, 255, 0);
                }
                
                UpdateText(index);
            }
            else
            {
                if (ItemIds[index] != 0 && ItemIds[index] != itemCell.Id) return;
                
                
                if (ItemIds[index] == 0)
                {
                    ItemIds[index] = itemCell.Id;
                    itemImages[index].sprite = worldCreation.Blocks[ItemIds[index]].img;
                    itemImages[index].color = new Color(255, 255, 255, 100);
                }

                if (ItemCount[index] < worldCreation.Blocks[ItemIds[index]].stackSize)
                {
                    itemCell.Count -= 1;
                    ItemCount[index] += 1;
                    UpdateText(index);
                }

                if (itemCell.Count <= 0)
                {
                    itemCell.Reset();
                }
                
                
                itemCell.UpdateText();
            }
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void DestroyedBlockInRage()
    {
        for (var i = _destroyedBlocks.Count - 1; i >= 0; i--)
        {
            var destroyed = _destroyedBlocks[i];
            
            if (Vector3.Distance(transform.position, destroyed.transform.position) > destroyedBlockReach) continue;
            AddItem(destroyed.GetComponent<DestroyedBlock>().ID, 1);
            Destroy(destroyed);
            _destroyedBlocks.Remove(destroyed);
        }
    }
}

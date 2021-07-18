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
    [Header("Prefabs")]
    [SerializeField] private RectTransform selector;
    [SerializeField] private worldCreation worldCreation;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private InventoryCell itemCell;
    [SerializeField] private GameObject cell;
    [SerializeField] private Transform cellParent;


    [Header("UI")] 
    [SerializeField] private GameObject furnace;
    [SerializeField] private GameObject crafting;
    
    [SerializeField] private int xStartInventory;
    [SerializeField] private int yStartInventory;
    [SerializeField] private int xDistInventory;
    [SerializeField] private int yDistInventory;

    [SerializeField] private GameObject[] itemGameObjects = new GameObject[5 * 9];
    [SerializeField] private TMP_Text[] itemCountText = new TMP_Text[5 * 9];
    [SerializeField] private Image[] itemImages = new Image[5 * 9];

    [Header("Const")]
    [SerializeField] private int destroyedBlockReach;
    [SerializeField] private float destroyedAliveTime;
    private const int Rows = 9;
    private const int Cols = 4;
    public bool InInventory { get; set; } = false;
    private readonly List<DestroyedBlock> _destroyedBlocks = new List<DestroyedBlock>();
    public List<DestroyedBlock> DestroyedBlocks => _destroyedBlocks;

    [Header("ItemHandel")] 
    [SerializeField] private GameObject itemHandel;

    public Transform ItemHandelTransform => itemHandel.transform;
    [SerializeField] private GameObject blockHandel;
    [SerializeField] private Voxelizer voxelizer;
    private MeshFilter _itemHandelMesh;
    private MeshFilter _blockHandelMesh;
    private int _currentHandel = -1;
    
    [Header("Items")] 
    [SerializeField] private Item[] itemsArr;
    public Dictionary<int, Item> Items { get; set; } = new Dictionary<int, Item>();
    public int itemIndexStart = 230;
    
    public int Current { get; private set; } = 0;

    public int[] ItemIds { get; private set; } = new int[45];
    public int[] ItemCount { get; private set; } = new int[45];

    private void Start()
    {
        _itemHandelMesh = itemHandel.GetComponent<MeshFilter>();
        _blockHandelMesh = blockHandel.GetComponent<MeshFilter>();
        
        
        
        for (var x = 0; x < Rows; x++)
        {
            for (var y = 0; y < Cols; y++)
            {
                var newInv = Instantiate(cell, cell.transform.position, cell.transform.rotation);

                newInv.transform.SetParent(cellParent, false);
                newInv.GetComponent<RectTransform>().localPosition = new Vector3(xStartInventory + xDistInventory * x - 40
                    ,yStartInventory + yDistInventory * y, 0);
                newInv.GetComponent<InventoryIndex>().playerInventory = this;
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
        
        CreateHandel();
        
        Current = (Current - (int)Input.mouseScrollDelta.y) % 9;
        if (Current < 0)
            Current = 8;
        
        selector.localPosition = new Vector2(-400 + Current * 100, selector.localPosition.y);

        if (!InInventory && Input.GetKeyDown(KeyCode.Q))
        {
            Drop(Current);
        }
        
        if (!Input.GetKeyDown(KeyCode.E)) return;
        cellParent.gameObject.SetActive(!cellParent.gameObject.activeInHierarchy);
        if(InInventory)
            itemCell.ResetToOriginalPos();
        InInventory = !InInventory;
        if(InInventory)
            OpenCrafting();
        Cursor.visible = InInventory;
        Cursor.lockState = InInventory ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void AddItem(int id, int amount)
    {
        var index = -1;
        
        for (var i = 0; i < ItemIds.Length; i++)
        {
            if (ItemIds[i] != id) continue;
            if (ItemCount[i] >= (id < itemIndexStart ?
                worldCreation.Blocks[id].stackSize: Items[id].stackSize)) continue;

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
                itemImages[i].sprite = id < itemIndexStart ? worldCreation.Blocks[id].img : Items[id].img;
                itemImages[i].color = new Color(255, 255, 255, 100);
                break;
            }
        }

        ItemCount[index] += amount;
        UpdateText(index);
        
        saveManager.SavePlayerData();

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
        itemImages[i].sprite = id < itemIndexStart ? worldCreation.Blocks[id].img : Items[id].img;
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
            
            itemImages[i].sprite = ItemIds[i] < itemIndexStart ?
                worldCreation.Blocks[ItemIds[i]].img : Items[ItemIds[i]].img;
            itemImages[i].color = new Color(255, 255, 255, 100);
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void AddDestroyedBlock(GameObject block)
    {
        _destroyedBlocks.Add(block.GetComponent<DestroyedBlock>());
        saveManager.SaveDestroyedBlocks(_destroyedBlocks);
    }

    public void ReplaceItem(int index, int mouseBtn)
    {
        var id = 0;
        var count = 0;
        var changed = false;
        if (mouseBtn == 0)
        {
            if (itemCell.Id == ItemIds[index] && itemCell.Id != 0)
            {
                ItemCount[index] += itemCell.Count;

                var stackSize = ItemIds[index] < itemIndexStart
                    ? worldCreation.Blocks[ItemIds[index]].stackSize
                    : Items[ItemIds[index]].stackSize;
                
                if (ItemCount[index] <= stackSize)
                {
                    itemCell.Reset();
                }
                else
                {
                    var dCount = ItemCount[index] % stackSize;
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
                itemCell.SetSprite(ItemIds[index] < itemIndexStart ? 
                    worldCreation.Blocks[ItemIds[index]].img : Items[ItemIds[index]].img,
                    ItemIds[index], ItemCount[index], index);


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
                itemImages[index].sprite = ItemIds[index] < itemIndexStart
                    ? worldCreation.Blocks[ItemIds[index]].img
                    : Items[ItemIds[index]].img;
                ItemCount[index] = itemCell.Count;
                itemImages[index].color = new Color(255, 255, 255, 100);
                itemCell.Reset();
                UpdateText(index);
            }
            else
            {
                if (id == 0) return;
                ItemIds[index] = id;
                itemImages[index].sprite = ItemIds[index] < itemIndexStart ?
                    worldCreation.Blocks[ItemIds[index]].img : Items[ItemIds[index]].img;
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
                
                itemCell.SetSprite(ItemIds[index] < itemIndexStart ?
                    worldCreation.Blocks[ItemIds[index]].img : Items[ItemIds[index]].img,
                    ItemIds[index], c - ItemCount[index], index);
                
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
                    itemImages[index].sprite = ItemIds[index] < itemIndexStart ?
                        worldCreation.Blocks[ItemIds[index]].img : Items[ItemIds[index]].img;
                    itemImages[index].color = new Color(255, 255, 255, 100);
                }

                var stack = ItemIds[index] < itemIndexStart
                    ? worldCreation.Blocks[ItemIds[index]].stackSize
                    : Items[ItemIds[index]].stackSize;
                if (ItemCount[index] < stack)
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
        
        saveManager.SavePlayerData();
    }

    public bool HasRequirements(List<int[]> requirements)
    {
        var req = new Dictionary<int, int>();

        foreach (var requirement in requirements)
        {
            if(requirement[0] == 0) continue;
            if (req.ContainsKey(requirement[0]))
            {
                req[requirement[0]] += requirement[1];
            }
            else
            {
                req.Add(requirement[0], requirement[1]);
            }
        }

        for (var i = 0; i < ItemIds.Length; i++)
        {
            if (req.ContainsKey(ItemIds[i]))
            {
                req[ItemIds[i]] -= ItemCount[i];
            }
        }

        return req.All(r => r.Value <= 0);
    }

    public void RemoveRequirements(List<int[]> requirements)
    {
        var req = new Dictionary<int, int>();

        foreach (var requirement in requirements)
        {
            if(requirement[0] == 0) continue;
            if (req.ContainsKey(requirement[0]))
            {
                req[requirement[0]] += requirement[1];
            }
            else
            {
                req.Add(requirement[0], requirement[1]);
            }
        }

        for (var j = 0; j < req.Count; j++)
        {
            var r = req.ElementAt(j);
            
            for (var i = 0; i < ItemIds.Length; i++)
            {
                if (ItemIds[i] == r.Key)
                {
                    ItemCount[i] -= r.Value;
                    if (ItemCount[i] <= 0)
                    {
                        req[r.Key] = -ItemCount[i];
                        ItemCount[i] = 0;
                        ItemIds[i] = 0;
                        itemImages[i].sprite = null;
                        itemImages[i].color = new Color(255, 255, 255, 0);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        
        UpdateUI();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void DestroyedBlockInRage()
    {
        for (var i = _destroyedBlocks.Count - 1; i >= 0; i--)
        {
            var destroyed = _destroyedBlocks[i];
            destroyed.CurrentTime += Time.deltaTime;

            if (destroyed.CurrentTime >= destroyedAliveTime)
            {
                Destroy(destroyed.gameObject);
                _destroyedBlocks.Remove(destroyed);
                continue;
            }
            
            if (Vector3.Distance(transform.position, destroyed.transform.position) > destroyedBlockReach) 
                continue;
            AddItem(destroyed.GetComponent<DestroyedBlock>().ID, 1);
            Destroy(destroyed.gameObject);
            _destroyedBlocks.Remove(destroyed);

            if (worldCreation.destroyedBlocksToCreate.Contains(destroyed))
            {
                worldCreation.destroyedBlocksToCreate.Remove(destroyed);
            }
            
            if (worldCreation.destroyedBlocksToApply.Contains(destroyed))
            {
                worldCreation.destroyedBlocksToApply.Remove(destroyed);
            }
        }
        if(Time.frameCount % 60 == 0)
            saveManager.SaveDestroyedBlocks(_destroyedBlocks);
    }

    private void CreateHandel()
    {
        if (ItemIds[Current] == 0)
        {
            _currentHandel = ItemIds[Current];
            _itemHandelMesh.mesh = null;
            _blockHandelMesh.mesh = null;
            return;
        }

        if (_currentHandel == ItemIds[Current])
            return;

        _currentHandel = ItemIds[Current];
        
        if (ItemIds[Current] < itemIndexStart)
        {
            var newMesh = new Mesh();
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var indices = new List<int>();

            var currentIndex = 0;

            var b = worldCreation.Blocks[ItemIds[Current]];

            var offset = new Vector3Int(0, 0, 0);

            worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
                b.blockShape.faceData[2], b.GETRect(b.topIndex), 2, 1);
            worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
                b.blockShape.faceData[5], b.GETRect(b.rightIndex), 5, 1);
            worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
                b.blockShape.faceData[4], b.GETRect(b.leftIndex), 4, 1);
            worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
                b.blockShape.faceData[1], b.GETRect(b.frontIndex), 1, 1);
            worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
                b.blockShape.faceData[0], b.GETRect(b.backIndex), 0, 1);
            worldCreation.blockCreation.GenerateBlock(ref currentIndex, offset, vertices, normals, uvs, indices,
                b.blockShape.faceData[3], b.GETRect(b.botIndex), 3, 1);

            newMesh.SetVertices(vertices);
            newMesh.SetNormals(normals);
            newMesh.SetUVs(0, uvs);
            newMesh.SetIndices(indices, MeshTopology.Triangles, 0);

            newMesh.RecalculateTangents();
            _blockHandelMesh.mesh = newMesh;
            
            _itemHandelMesh.mesh = null;
        }
        else
        {
            
            var mesh = voxelizer.SpriteToVoxel(Items[ItemIds[Current]].texture2d,
                worldCreation.standardBlockShape, worldCreation.blockCreation, new Vector3(0, 0, 0));

            _itemHandelMesh.mesh = mesh;
            
            _blockHandelMesh.mesh = null;
        }
    }

    

    private void OpenCrafting()
    {
        crafting.SetActive(true);
        furnace.SetActive(false);
    }

    public void OpenFurnace()
    {
        InInventory = true;
        cellParent.gameObject.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        crafting.SetActive(false);
        furnace.SetActive(true);
    }

    private void Drop(int index)
    {
        if (ItemIds[index] == 0) return;
        AddDestroyedBlock(worldCreation.CreateDestroyedBlock(ItemIds[index],
            ItemHandelTransform.position + transform.forward * 2f));
        ItemCount[index] -= 1;


        if (ItemCount[index] == 0)
        {
            ItemCount[index] = 0;
            ItemIds[index] = 0;
            itemImages[index].sprite = null;
            itemImages[index].color = new Color(255, 255, 255, 0);
        }

        UpdateText(index);
    }

    public void DeathDrop()
    {
        for (var i = 0; i < ItemIds.Length; i++)
        {
            var count = ItemCount[i];
            for (var j = 0; j < count; j++)
            {
                Drop(i);
            }
        }
    }

    public void ItemDict()
    {
        foreach (var item in itemsArr)
        {
            Items.Add(item.id, item);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerFurnace : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private InventoryCell inventoryCell;
    [SerializeField] private worldCreation worldCreation;
    public Furnace Furnace { get; private set; }

    private Chunck _chunck;

    [SerializeField] private Image meltingImage;
    [SerializeField] private Image melterImage;
    [SerializeField] private Image resultImage;
    
    [SerializeField] private TMP_Text meltingText;
    [SerializeField] private TMP_Text melterText;
    [SerializeField] private TMP_Text resultText;


    public void SetFurnace(Furnace furnace, Chunck chunck)
    {
        Furnace = furnace;
        _chunck = chunck;
        UpdateUI();
    }

    public void AddMelting(int mouseBtn)
    {
        if (Furnace.meltingID == 0 && inventoryCell.Id == 0) return;
        if (inventoryCell.Id == 0)
        {
            var lastIndex = 0;
            for (var i = 0; i < playerInventory.ItemIds.Length; i++)
            {
                if (playerInventory.ItemIds[i] != 0) continue;
                lastIndex = i;
                break;
            }
            if (mouseBtn == 0)
            {
                inventoryCell.SetSprite(Furnace.meltingID < playerInventory.itemIndexStart 
                        ? worldCreation.Blocks[Furnace.meltingID].img :
                        playerInventory.Items[Furnace.meltingID].img,
                    Furnace.meltingID, Furnace.meltingCount,lastIndex);
                Furnace.meltingID = 0;
                Furnace.meltingCount = 0;
            }
            else
            {
                var temp = Furnace.meltingCount;
                Furnace.meltingCount /= 2;
                inventoryCell.SetSprite(Furnace.meltingID < playerInventory.itemIndexStart 
                        ? worldCreation.Blocks[Furnace.meltingID].img :
                        playerInventory.Items[Furnace.meltingID].img,
                    Furnace.meltingID, temp - Furnace.meltingCount,lastIndex);

                if (Furnace.meltingCount == 0)
                {
                    Furnace.meltingID = 0;
                }
            }
        }
        else
        {
            if (inventoryCell.Id < playerInventory.itemIndexStart ? 
                worldCreation.Blocks[inventoryCell.Id].meltable : playerInventory.Items[inventoryCell.Id].meltable)
            {
                if (mouseBtn == 0)
                {
                    if (Furnace.meltingID == 0)
                    {
                        Furnace.meltingID = inventoryCell.Id;
                        Furnace.meltingCount = inventoryCell.Count;
                        inventoryCell.Reset();
                    }
                    else if (Furnace.meltingID == inventoryCell.Id)
                    {
                        if (Furnace.meltingCount + inventoryCell.Count <=
                            (inventoryCell.Id < playerInventory.itemIndexStart
                                ? worldCreation.Blocks[inventoryCell.Id].stackSize
                                : playerInventory.Items[inventoryCell.Id].stackSize))
                        {
                            Furnace.meltingCount += inventoryCell.Count;
                            inventoryCell.Reset();
                        }
                        else
                        {
                            var temp = Furnace.meltingCount + inventoryCell.Count;
                            Furnace.meltingCount = inventoryCell.Id < playerInventory.itemIndexStart
                                ? worldCreation.Blocks[inventoryCell.Id].stackSize
                                : playerInventory.Items[inventoryCell.Id].stackSize;
                            inventoryCell.Count = temp - Furnace.meltingCount;
                        }
                    }
                }
                else
                {
                    if (Furnace.meltingID == 0 || Furnace.meltingID == inventoryCell.Id)
                    {
                        Furnace.meltingID = inventoryCell.Id;
                        
                        if (Furnace.meltingCount >= (inventoryCell.Id < playerInventory.itemIndexStart ? 
                            worldCreation.Blocks[inventoryCell.Id].stackSize :
                            playerInventory.Items[inventoryCell.Id].stackSize))
                            return;

                        Furnace.meltingCount += 1;
                        inventoryCell.Count -= 1;
                        if (inventoryCell.Count == 0)
                        {
                            inventoryCell.Reset();
                        }
                    }
                }
            }
        }
        UpdateUI();
        inventoryCell.UpdateText();
        worldCreation.saveManager.SaveChunck(_chunck);
    }
    
    public void AddMelter(int mouseBtn)
    {
        if (Furnace.melterID == 0 && inventoryCell.Id == 0) return;
        if (inventoryCell.Id == 0)
        {
            var lastIndex = 0;
            for (var i = 0; i < playerInventory.ItemIds.Length; i++)
            {
                if (playerInventory.ItemIds[i] != 0) continue;
                lastIndex = i;
                break;
            }
            if (mouseBtn == 0)
            {
                inventoryCell.SetSprite(Furnace.melterID < playerInventory.itemIndexStart 
                        ? worldCreation.Blocks[Furnace.melterID].img :
                        playerInventory.Items[Furnace.melterID].img,
                    Furnace.melterID, Furnace.melterCount,lastIndex);
                Furnace.melterID = 0;
                Furnace.melterCount = 0;
            }
            else
            {
                var temp = Furnace.melterCount;
                Furnace.melterCount /= 2;
                inventoryCell.SetSprite(Furnace.melterID < playerInventory.itemIndexStart 
                        ? worldCreation.Blocks[Furnace.melterID].img :
                        playerInventory.Items[Furnace.melterID].img,
                    Furnace.melterID, temp - Furnace.melterCount,lastIndex);

                if (Furnace.melterCount == 0)
                {
                    Furnace.melterID = 0;
                }
            }
        }
        else
        {
            if (inventoryCell.Id < playerInventory.itemIndexStart ? 
                worldCreation.Blocks[inventoryCell.Id].melter : playerInventory.Items[inventoryCell.Id].melter)
            {
                if (mouseBtn == 0)
                {
                    if (Furnace.melterID == 0)
                    {
                        Furnace.melterID = inventoryCell.Id;
                        Furnace.melterCount = inventoryCell.Count;
                        inventoryCell.Reset();
                    }
                    else if (Furnace.melterID == inventoryCell.Id)
                    {
                        if (Furnace.melterCount + inventoryCell.Count <=
                            (inventoryCell.Id < playerInventory.itemIndexStart
                                ? worldCreation.Blocks[inventoryCell.Id].stackSize
                                : playerInventory.Items[inventoryCell.Id].stackSize))
                        {
                            Furnace.melterCount += inventoryCell.Count;
                            inventoryCell.Reset();
                        }
                        else
                        {
                            var temp = Furnace.melterCount + inventoryCell.Count;
                            Furnace.melterCount = inventoryCell.Id < playerInventory.itemIndexStart
                                ? worldCreation.Blocks[inventoryCell.Id].stackSize
                                : playerInventory.Items[inventoryCell.Id].stackSize;
                            inventoryCell.Count = temp - Furnace.melterCount;
                        }
                    }
                }
                else
                {
                    if (Furnace.melterID == 0 || Furnace.melterID == inventoryCell.Id)
                    {
                        Furnace.melterID = inventoryCell.Id;
                        
                        if (Furnace.melterCount >= (inventoryCell.Id < playerInventory.itemIndexStart ? 
                            worldCreation.Blocks[inventoryCell.Id].stackSize :
                            playerInventory.Items[inventoryCell.Id].stackSize))
                            return;

                        Furnace.melterCount += 1;
                        inventoryCell.Count -= 1;
                        if (inventoryCell.Count == 0)
                        {
                            inventoryCell.Reset();
                        }
                    }
                }
            }
        }
        UpdateUI();
        inventoryCell.UpdateText();
        worldCreation.saveManager.SaveChunck(_chunck);
    }

    public void TakeResult(int mouseBtn)
    {
        if (Furnace.resultCount == 0) return;
        
        var lastIndex = 0;
        for (var i = 0; i < playerInventory.ItemIds.Length; i++)
        {
            if (playerInventory.ItemIds[i] != 0) continue;
            lastIndex = i;
            break;
        }
        
        if (mouseBtn == 0)
        {
            inventoryCell.SetSprite(Furnace.resultID < playerInventory.itemIndexStart 
                    ? worldCreation.Blocks[Furnace.resultID].img :
                    playerInventory.Items[Furnace.resultID].img,
                Furnace.resultID, Furnace.resultCount,lastIndex);
            Furnace.resultID = 0;
            Furnace.resultCount = 0;
        }
        else
        {
            var temp = Furnace.resultCount;
            Furnace.resultCount /= 2;
            inventoryCell.SetSprite(Furnace.resultID < playerInventory.itemIndexStart 
                    ? worldCreation.Blocks[Furnace.resultID].img :
                    playerInventory.Items[Furnace.resultID].img,
                Furnace.resultID, temp - Furnace.resultCount,lastIndex);

            if (Furnace.resultCount == 0)
            {
                Furnace.resultID = 0;
            }
        }
        
        UpdateUI();
        inventoryCell.UpdateText();
        worldCreation.saveManager.SaveChunck(_chunck);
    }

    public void UpdateUI()
    {
        if (Furnace.meltingID != 0)
        {
            meltingImage.sprite = Furnace.meltingID < playerInventory.itemIndexStart 
                ? worldCreation.Blocks[Furnace.meltingID].img : playerInventory.Items[Furnace.meltingID].img;
            meltingImage.color = new Color(255, 255, 255, 100);
            meltingText.text = "" + Furnace.meltingCount;
        }
        else
        {
            meltingImage.sprite = null;
            meltingImage.color = new Color(255, 255, 255, 0);
            meltingText.text = "";
        }

        if (Furnace.melterID != 0)
        {
            melterImage.sprite = Furnace.melterID < playerInventory.itemIndexStart 
                ? worldCreation.Blocks[Furnace.melterID].img : playerInventory.Items[Furnace.melterID].img;
            melterImage.color = new Color(255, 255, 255, 100);
            melterText.text = "" + Furnace.melterCount;
        }
        else
        {
            melterImage.sprite = null;
            melterImage.color = new Color(255, 255, 255, 0);
            melterText.text = "";
        }
        
        if (Furnace.resultID != 0)
        {
            resultImage.sprite = Furnace.resultID < playerInventory.itemIndexStart 
                ? worldCreation.Blocks[Furnace.resultID].img : playerInventory.Items[Furnace.resultID].img;
            resultImage.color = new Color(255, 255, 255, 100);
            resultText.text = "" + Furnace.resultCount;
        }
        else
        {
            resultImage.sprite = null;
            resultImage.color = new Color(255, 255, 255, 0);
            resultText.text = "";
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class Chunck : MonoBehaviour
{
    public Vector3 Pos { get; set; }
    public int[,,] WaterIDs { get; set; }

    public int[,,] BlockIDs { get; set; }

    public Dictionary<Vector3Int, Furnace> furnaces = new Dictionary<Vector3Int, Furnace>();

    public worldCreation worldCreation;
    private PlayerInventory _playerInventory;
    private PlayerFurnace _playerFurnace;

    private void Awake()
    {
        Pos = transform.position;
        _playerInventory = FindObjectOfType<PlayerInventory>();
        _playerFurnace = _playerInventory.gameObject.GetComponent<PlayerFurnace>();
    }

    private void Update()
    {
        foreach (var furnace in furnaces)
        {
            if (furnace.Value.meltingCount > 0 && furnace.Value.melterCount > 0
            && (furnace.Value.resultID == 0 || 
                (furnace.Value.resultID == (furnace.Value.meltingID < _playerInventory.itemIndexStart ? 
                worldCreation.Blocks[furnace.Value.meltingID].meltedId :
                _playerInventory.Items[furnace.Value.meltingID].meltedId) 
                 && furnace.Value.resultCount < (furnace.Value.meltingID < _playerInventory.itemIndexStart ? 
                     worldCreation.Blocks[furnace.Value.meltingID].stackSize :
                     _playerInventory.Items[furnace.Value.meltingID].stackSize))))
            {
                furnace.Value.time += Time.deltaTime;
            }
            else
            {
                furnace.Value.time = 0f;
            }

            if (furnace.Value.time >= 2)
            {
                furnace.Value.resultID = furnace.Value.meltingID < _playerInventory.itemIndexStart
                    ? worldCreation.Blocks[furnace.Value.meltingID].meltedId
                    : _playerInventory.Items[furnace.Value.meltingID].meltedId;
                furnace.Value.resultCount += 1;
                furnace.Value.meltingCount -= 1;
                if (furnace.Value.meltingCount == 0)
                {
                    furnace.Value.meltingID = 0;
                }
                
                furnace.Value.melterCount -= 1;
                if (furnace.Value.melterCount == 0)
                {
                    furnace.Value.melterID = 0;
                }

                if (_playerFurnace.Furnace == furnace.Value)
                {
                    _playerFurnace.UpdateUI();
                }
            }
            
        }

        if (furnaces.Count > 0)
        {
            worldCreation.saveManager.SaveChunck(this);
        }
    }    
}

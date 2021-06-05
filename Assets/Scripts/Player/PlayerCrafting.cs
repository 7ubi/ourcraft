using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCrafting : MonoBehaviour
{
    [SerializeField] private GameObject craftableGameObject;
    [SerializeField] private Transform panel;
    [SerializeField] private CraftableItem[] craftableItems;
    private readonly List<Image> _craftableItemsObjects = new List<Image>();
    [SerializeField] private Image[] craftingSlots;
    [SerializeField] private Image result;
    [SerializeField] private worldCreation worldCreation;

    private PlayerInventory _playerInventory;

    private bool _generated = false;
    private int _resultID;
    private int _resultIndex;

    private void Start()
    {
        _playerInventory = GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        if (!_generated)
        {
            var index = 0;
            foreach (var craftable in craftableItems)
            {
                var craft = Instantiate(craftableGameObject, panel);
                _craftableItemsObjects.Add(craft.GetComponent<Image>());
                craft.GetComponent<SetRecipe>().Index = index;
                craft.transform.GetChild(0).GetComponent<Image>().sprite = worldCreation.Blocks[craftable.resultID].img;
                index++;
            }

            _generated = true;
        }

        for (var i = 0; i < _craftableItemsObjects.Count; i++)
        {
            var req = craftableItems[i].GetRequirements();
            _craftableItemsObjects[i].color = _playerInventory.HasRequirements(req) ? Color.black : Color.red;
        }
    }

    public void SetRecipe(int index)
    {
        if(!_playerInventory.HasRequirements(craftableItems[index].GetRequirements())) return;
        
        _resultID = craftableItems[index].resultID;
        _resultIndex = index;
        for (var i = 0; i < craftableItems[index].recipe.Length; i++)
        {
            if(craftableItems[index].recipe[i] == 0) continue;
            
            craftingSlots[i].sprite = worldCreation.Blocks[craftableItems[index].recipe[i]].img;
            craftingSlots[i].color = new Color(255, 255, 255, 100);
        }
        
        
        result.sprite = worldCreation.Blocks[craftableItems[index].resultID].img;
        result.color = new Color(255, 255, 255, 100);
    }

    public void Craft()
    {
        if(!_playerInventory.HasRequirements(craftableItems[_resultIndex].GetRequirements())) return;
        
        _playerInventory.RemoveRequirements(craftableItems[_resultIndex].GetRequirements());
        _playerInventory.AddItem(_resultID, 1);
    }
}

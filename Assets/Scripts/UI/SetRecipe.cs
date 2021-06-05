using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class SetRecipe : MonoBehaviour, IPointerClickHandler 
{
    private PlayerCrafting _playerCrafting;
    [SerializeField] public Image img;
    
    public int Index { get; set; }

    private void Start()
    {
        _playerCrafting = FindObjectOfType<PlayerCrafting>().GetComponent<PlayerCrafting>();
    }
    
    public void OnPointerClick(PointerEventData data)
    {
        _playerCrafting.SetRecipe(Index);
    }
}

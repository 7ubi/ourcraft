using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class FurnaceUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private PlayerFurnace playerFurnace;
    [SerializeField] private int index;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        switch (index)
        {
            case 0:
                playerFurnace.AddMelting(eventData.button == PointerEventData.InputButton.Left ? 0 : 1);
                break;
            case 1:
                playerFurnace.AddMelter(eventData.button == PointerEventData.InputButton.Left ? 0 : 1);
                break;
            case 2:
                playerFurnace.TakeResult(eventData.button == PointerEventData.InputButton.Left ? 0 : 1);
                break;
        }
    }
}

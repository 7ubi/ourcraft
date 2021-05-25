using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class PlayerActionController : MonoBehaviour
{
    [SerializeField] private float range;
    [SerializeField] private worldCreation worldCreation;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private GameObject selectGameObject;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private DestroyBlock destroyBlock;
    
    private void Update()
    {
        var leftClick = Input.GetMouseButton(0);
        var rightClick = Input.GetMouseButtonDown(1);

        if (!leftClick)
        {
            destroyBlock.IsDestroyingBlock = false;
        }
        RaycastHit hit;
        if (leftClick || rightClick)
        {   
            if (!Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, range,
                layerMask))
            {
                destroyBlock.IsDestroyingBlock = false;
                return;
            }
            if (leftClick)
            {
                destroyBlock.IsDestroyingBlock = true;
                destroyBlock.Block = Vector3Int.FloorToInt(hit.point + transform.forward * .01f);
            }
            
            if(rightClick)
            {
                if (!inventory.CanPlaceBlock()) return;
                if (!playerController.CanPlaceBlock) return;
                worldCreation.PlaceBlock(hit.point + transform.forward * -.01f, inventory.ItemIds[inventory.Current]);
                inventory.AddItem(inventory.ItemIds[inventory.Current], -1);
            }
        }
        
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, range, layerMask))
        {
            selectGameObject.SetActive(true);
            var hitPoint = hit.point + transform.forward * -.01f;
            var pos = new Vector3((float) Math.Floor(hitPoint.x) + .5f,
                (float) Math.Floor(hitPoint.y) + .5f, (float) Math.Floor(hitPoint.z) + .5f);
            selectGameObject.transform.position = pos;
        }
        else
        {
            playerController.CanPlaceBlock = true;
            selectGameObject.SetActive(false);
        }
    }
}

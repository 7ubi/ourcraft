using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class PlayerActionController : MonoBehaviour
{
    [SerializeField] private float range;
    [SerializeField] private worldCreation worldCreation;
    [SerializeField] private GameObject underWater;
    
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private GameObject selectGameObject;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private DestroyBlock destroyBlock;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private InteractableBlocks interactableBlocks;

    [SerializeField] private Animator animator;
    
    private void Update()
    {
        if (playerInventory.InInventory) return;
        
        var leftClick = Input.GetMouseButton(0);
        var rightClick = Input.GetMouseButtonDown(1);

        RaycastHit hit;
        
        if ((rightClick || leftClick) && !animator.GetCurrentAnimatorStateInfo(0).IsName("Build"))
        {
            if (leftClick && !Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward),
                out hit, range, layerMask))
            {
                if(Input.GetMouseButtonDown(0))
                    animator.SetTrigger("Build");
            }
            else
            {
                animator.SetTrigger("Build");
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
        
        underWater.SetActive(worldCreation.GetUnderWater(transform.position));
        
        if (!leftClick)
        {
            destroyBlock.IsDestroyingBlock = false;
        }
        
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
                if (worldCreation.Blocks[worldCreation.GetBlock(hit.point + transform.forward * .01f)].isInteractable)
                {
                    var pos = hit.point + transform.forward * .01f;
                    var chunck = worldCreation.GetChunck(pos);
                    var chunckPos = chunck.transform.position;
                    var block = Vector3Int.FloorToInt(new Vector3(Mathf.FloorToInt(pos.x) - chunckPos.x, 
                        Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z) - chunckPos.z));
                    var id = worldCreation.Blocks[worldCreation.GetBlock(pos)].id;
                    if(id == BlockTypes.Furnace)
                        interactableBlocks.Furnace(chunck.furnaces[block], chunck);
                    else if(id == BlockTypes.TNT)
                        interactableBlocks.Explode(5, pos);
                }
                else
                {
                    if (inventory.ItemIds[inventory.Current] >= playerInventory.itemIndexStart) return;
                    if (!inventory.CanPlaceBlock()) return;
                    if (!playerController.CanPlaceBlock) return;

                    worldCreation.PlaceBlock(hit.point + transform.forward * -.01f,
                        inventory.ItemIds[inventory.Current]);
                    inventory.RemoveItem(inventory.Current);
                }
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerActionController : MonoBehaviour
{
    [SerializeField] private float Range;
    [SerializeField] private worldCreation _worldCreation;
    [SerializeField] private PlayerInventory _inventory;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private GameObject selectGameObject;

    [SerializeField] private PlayerController playerController;

    private void Update()
    {
        var leftClick = Input.GetMouseButtonDown(0);
        var rightClick = Input.GetMouseButtonDown(1);
        
        
        
        if (leftClick || rightClick)
        {   
            RaycastHit hit;
            if (!Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Range, layerMask)) return;
            if (leftClick)
            {
                _worldCreation.DestroyBlock(hit.point + transform.forward * .01f);
            }
            if(rightClick)
            {
                if (!_inventory.CanPlaceBlock()) return;
                if (!playerController.CanPlaceBlock) return;
                _worldCreation.PlaceBlock(hit.point + transform.forward * -.01f, _inventory.ItemIds[_inventory.Current]);
                _inventory.AddItem(_inventory.ItemIds[_inventory.Current], -1);
            }
        }
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Range, layerMask))
            {
                selectGameObject.SetActive(true);
                var hitPoint = hit.point + transform.forward * -.01f;
                var pos = new Vector3((float) Math.Floor(hitPoint.x) + .5f,
                    (float) Math.Floor(hitPoint.y) + .5f, (float) Math.Floor(hitPoint.z) + .5f);
                selectGameObject.transform.position = pos;
            }
            else
            {
                selectGameObject.SetActive(false);
            }
        }
    }
}

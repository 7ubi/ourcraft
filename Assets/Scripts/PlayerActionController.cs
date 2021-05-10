using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerActionController : MonoBehaviour
{
    [SerializeField] private float Range;
    [SerializeField] private world_creation _worldCreation;
    
    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        RaycastHit hit;
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Range))
        {
            _worldCreation.DestroyBlock(hit.point  + transform.forward * .01f, transform.position);
        }
    }
}

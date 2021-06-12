using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintMult;
    [SerializeField] private float waterMult;
    [SerializeField] private Animator animator;
    
    [Header("Camera")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float speedH = 2.0f;
    [SerializeField] private  float speedV = 2.0f;
    
    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private int drag;
    [SerializeField] private Transform groundCheck;
    
    [Header("Extra")]
    [SerializeField] private GameObject selectGameObject;
    [SerializeField] private worldCreation worldCreation;
    

    private float forward;
    private float right;
    
    private float _yaw = 0.0f;
    private float _pitch = 0.0f;
    
    private Rigidbody _rb;
    private PlayerInventory _playerInventory;
    
    public Camera Camera => _camera;
    public bool CanPlaceBlock { get; set; }
    
    public bool InPause { get; set; }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _playerInventory = GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        _rb.angularVelocity = new Vector3(0, 0, 0);
        
        if (_playerInventory.InInventory)
        {
            transform.eulerAngles = new Vector3(0.0f, _yaw, 0.0f);
            _camera.transform.eulerAngles = new Vector3(_pitch, _yaw, 0.0f);
            _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
            _rb.constraints = RigidbodyConstraints.FreezeAll;
            animator.SetBool("IsWalking", false);
            return;
        }

        if (InPause)
        {
            _rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        forward = Input.GetAxis("Vertical") * moveSpeed;
        right = Input.GetAxis("Horizontal") * moveSpeed;

        if (forward != 0 || right != 0)
        {
            animator.SetBool("IsWalking", true);
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }
        
        if (Input.GetKey(KeyCode.LeftShift))
        {
            forward *= sprintMult;
            right *= sprintMult;
        }
   
        _yaw += speedH * Input.GetAxis("Mouse X");
        _pitch -= speedV * Input.GetAxis("Mouse Y");

        if (_pitch > 90)
            _pitch = 90;
        if (_pitch < -90)
            _pitch = -90;

        transform.eulerAngles = new Vector3(0.0f, _yaw, 0.0f);
        _camera.transform.eulerAngles = new Vector3(_pitch, _yaw, 0.0f);

        if (worldCreation.GetUnderWater(transform.position + new Vector3(0, -0.5f, 0)))
        {
            forward *= waterMult;
            right *= waterMult;
        }


        if (worldCreation.GetUnderWater(transform.position))
        {
            _rb.drag = drag;
            if (Input.GetKey(KeyCode.Space))
            {
                _rb.velocity = new Vector3(_rb.velocity.x, jumpForce, _rb.velocity.z);
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                _rb.velocity = new Vector3(_rb.velocity.x, -jumpForce, _rb.velocity.z);
            }
            
        }
        else
        {
            _rb.drag = 0;
            if (Input.GetKey(KeyCode.Space) && IsGrounded())
                _rb.velocity = new Vector3(_rb.velocity.x, jumpForce, _rb.velocity.z);
        }
        
        if (_playerInventory.InInventory) return;
        var velocity = _rb.velocity;
        var yVel = velocity.y;
        var transform1 = transform;
        velocity = transform1.forward * forward + transform1.right * right;
        velocity = new Vector3(velocity.x, yVel, velocity.z);
        _rb.velocity = velocity;
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(groundCheck.position, -Vector3.up, .1f);
    }

    public void LoadData(Vector3 pos, Quaternion rotation)
    {
        transform.position = pos;
        _camera.transform.rotation = rotation;
        transform.eulerAngles = new Vector3(0, _camera.transform.eulerAngles.y, 0);
    }

    public void SetPos()
    {
        var position = transform.position;
        for (var y = worldCreation.MAXHeight - 1; y >= 0; y--)
        {
            if (worldCreation.GetBlock(new Vector3(position.x, y, position.z)) == 0) continue;
            position = new Vector3(position.x, y + 4, position.z);
            transform.position = position;
            return;
        }
    }

    public void Pause()
    {
        _rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == selectGameObject)
        {
            CanPlaceBlock = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == selectGameObject)
        {
            CanPlaceBlock = true;
        }
    }
}

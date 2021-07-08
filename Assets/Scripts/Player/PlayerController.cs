using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private GameObject head;
    
    public float Yaw { get; private set; } = 0.0f;

    public float Pitch { get; private set; } = 0.0f;
    public int Orientation { get; set; }
    
    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private int drag;
    [SerializeField] private Transform groundCheck;
    
    [Header("Extra")]
    [SerializeField] private GameObject selectGameObject;
    [SerializeField] private worldCreation worldCreation;
    

    private float _forward;
    private float _right;

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
            transform.eulerAngles = new Vector3(0.0f, Yaw, 0.0f);
            _camera.transform.eulerAngles = new Vector3(Pitch, Yaw, 0.0f);
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

        _forward = Input.GetAxis("Vertical") * moveSpeed;
        _right = Input.GetAxis("Horizontal") * moveSpeed;

        if (_forward != 0 || _right != 0)
        {
            animator.SetBool("IsWalking", true);
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }
        
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _forward *= sprintMult;
            _right *= sprintMult;
        }
   
        Yaw += speedH * Input.GetAxis("Mouse X");
        Pitch -= speedV * Input.GetAxis("Mouse Y");

        if (Pitch > 90)
            Pitch = 90;
        if (Pitch < -90)
            Pitch = -90;

        var XZDirection = transform.forward;
        XZDirection.y = 0;
        
        if (Vector3.Angle(XZDirection, Vector3.forward) <= 45)
            Orientation = 0;
        else if (Vector3.Angle(XZDirection, Vector3.right) <= 45)
            Orientation = 5;
        else if (Vector3.Angle(XZDirection, Vector3.back) <= 45)
            Orientation = 1;
        else
            Orientation = 4;
        
        var transform2 = _camera.transform;

        var headPos = head.transform.position;

        transform.eulerAngles = new Vector3(0.0f, Yaw, 0.0f);
        transform2.eulerAngles = new Vector3(transform2.eulerAngles.x, Yaw, 0.0f);
        head.transform.eulerAngles = new Vector3(Pitch, head.transform.eulerAngles.y, 0);
        
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            foreach (var chunk in worldCreation.chunksInRange)
            {
                if (Vector3.Distance(chunk.transform.position + 
                        new Vector3(worldCreation.Size / 2f, transform2.position.y, worldCreation.Size / 2f),
                    transform2.position) < worldCreation.Size * 2)
                {
                    chunk.gameObject.SetActive(true);
                }
                else
                    chunk.gameObject.SetActive(Vector3.Angle(chunk.transform.position + 
                     new Vector3(worldCreation.Size / 2f, 0, worldCreation.Size / 2f)
                     - headPos, transform2.forward) < 90.0f);
            }
        }
        
        if (worldCreation.GetUnderWater(transform.position + new Vector3(0, -0.5f, 0)))
        {
            _forward *= waterMult;
            _right *= waterMult;
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
        velocity = transform1.forward * _forward + transform1.right * _right;
        velocity = new Vector3(velocity.x, yVel, velocity.z);
        _rb.velocity = velocity;
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(groundCheck.position, -Vector3.up, .1f);
    }

    public void LoadData(Vector3 pos, float yaw, float pitch)
    {
        transform.position = pos;
        Yaw = yaw;
        Pitch = pitch;
    }

    public void SetPos()
    {
        var position = transform.position;
        for (var y = worldCreation.MAXHeight - 1; y >= 0; y--)
        {
            if (worldCreation.GetBlock(new Vector3(position.x, y, position.z)) == 0) continue;
            position = new Vector3(position.x, y + 3, position.z);
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

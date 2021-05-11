using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private Camera _camera;
    [SerializeField] private float speedH = 2.0f;
    [SerializeField] private  float speedV = 2.0f;
    [SerializeField] private float jumpForce;

    private float forward;
    private float right;
    
    private float _yaw = 0.0f;
    private float _pitch = 0.0f;
    
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        forward = Input.GetAxis("Vertical") * moveSpeed;
        right = Input.GetAxis("Horizontal") * moveSpeed;

        var mouseRight = Input.GetAxis("Mouse X");
        var mouseUp = Input.GetAxis("Mouse Y");
        
   
        _yaw += speedH * Input.GetAxis("Mouse X");
        _pitch -= speedV * Input.GetAxis("Mouse Y");

        if (_pitch > 90)
            _pitch = 90;
        if (_pitch < -90)
            _pitch = -90;

        transform.eulerAngles = new Vector3(0.0f, _yaw, 0.0f);
        _camera.transform.eulerAngles = new Vector3(_pitch, _yaw, 0.0f);

        if (Input.GetKey(KeyCode.Space) && isGrounded())
            _rb.velocity = new Vector3(_rb.velocity.x, jumpForce, _rb.velocity.z);
    }

    private void LateUpdate()
    {
        var velocity = _rb.velocity;
        var yVel = velocity.y;
        velocity = transform.forward * forward + transform.right * right;
        velocity = new Vector3(velocity.x, yVel, velocity.z);
        _rb.velocity = velocity;
    }

    bool isGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, 1.25f + .1f);
    }
}

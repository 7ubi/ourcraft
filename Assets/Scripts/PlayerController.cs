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
        var forward = Input.GetAxis("Vertical") * moveSpeed;
        var right = Input.GetAxis("Horizontal") * moveSpeed;
        transform.position += transform.forward * (forward * Time.deltaTime) + transform.right * (right * Time.deltaTime);

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

        if (Input.GetKey(KeyCode.Space) && _rb.velocity.y == 0)
            _rb.velocity = new Vector3(_rb.velocity.x, jumpForce, _rb.velocity.z);
    }
}

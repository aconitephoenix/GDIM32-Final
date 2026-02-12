using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float maxLookAngle = 80f;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOV = 70f;
    [SerializeField] private float fovTransitionSpeed = 10f;
    
    private CharacterController characterController;
    private Camera playerCamera;
    private Vector3 velocity;
    private bool isGrounded;
    private float verticalRotation = 0f;
    private bool isSprinting = false;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // If camera transform not assigned, try to find it
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // Get camera component
        playerCamera = cameraTransform.GetComponent<Camera>();
        if (playerCamera != null)
        {
            normalFOV = playerCamera.fieldOfView;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleFOV();
    }
    
    private void HandleMovement()
    {
        // Check if grounded
        isGrounded = characterController.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        // Get input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        // Check if sprinting
        isSprinting = Input.GetKey(KeyCode.LeftShift) && isGrounded && moveZ > 0;
        
        // Calculate current speed
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
        
        // Calculate movement direction
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        characterController.Move(move * currentSpeed * Time.deltaTime);
        
        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
        
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    
    private void HandleMouseLook()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Rotate player horizontally
        transform.Rotate(Vector3.up * mouseX);
        
        // Rotate camera vertically
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }
    
    private void HandleFOV()
    {
        if (playerCamera == null) return;
        
        // Smoothly transition FOV based on sprint state
        float targetFOV = isSprinting ? sprintFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovTransitionSpeed * Time.deltaTime);
    }
}

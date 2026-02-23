using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;    
    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float maxLookAngle = 80f;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOV = 70f;
    [SerializeField] private float fovTransitionSpeed = 10f;
    [SerializeField] private float sprintDuration = 2f;
    [SerializeField] private Image SprintBar;
    [SerializeField] private float alphaBlinkSpeed = 2f;
    public float Sprint = 1f;
    private bool CanSprint;

    [SerializeField] private Rigidbody rb;
    private Camera playerCamera;
    private Vector3 velocity;
    private bool isGrounded;
    private float verticalRotation = 0f;
    private bool isSprinting = false;

    // Start is called before the first frame update
    void Start()
    {
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        cameraTransform = Camera.main.transform;

        // Get camera component
        playerCamera = cameraTransform.GetComponent<Camera>();
        normalFOV = playerCamera.fieldOfView;

    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleSprint();
    }
    
    private void HandleMovement()
    {
        // Check if grounded
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        // Get input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        // Check if sprinting
        isSprinting = Input.GetKey(KeyCode.LeftShift) && isGrounded && CanSprint;
        
        // Calculate current speed
        float currentSpeed;
        if (isSprinting)
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = moveSpeed;
        }

        //rb player movement
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        rb.MovePosition(rb.position + move * currentSpeed * Time.deltaTime);


        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            //rb force jump
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        }
        
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
    
    private void HandleSprint()
    {        
        // lerp to transition FOV based on sprint state
        float targetFOV;
        
        if (isSprinting)
        {
            targetFOV = sprintFOV;
            // if sprinting decrease sprint bar
            if (SprintBar.rectTransform.localScale.x >= 0f)
            {
                Sprint -= 0.1f * sprintDuration * Time.deltaTime;
            }
            
        }
        else
        {
            targetFOV = normalFOV;
            // if sprinting increase sprint bar
            if (SprintBar.rectTransform.localScale.x <= 1f)
            {
                Sprint += 0.1f * sprintDuration * Time.deltaTime;
            }

            
        }

        // sprintbar coloring and sprint availability
        if (Sprint <= 0f && CanSprint == true)
        {
            SprintBar.color = Color.red;
            CanSprint = false;
            //Debug.Log("Out of sprint!");
        }
        if (Sprint >= 1f && CanSprint == false)
        {
            SprintBar.color = Color.cyan;
            CanSprint = true;
            //Debug.Log("Sprint ready!");
        }

        // Pulsing alpha when sprint is unavailable
        if (!CanSprint)
        {
            float alpha = Mathf.PingPong(Time.time * alphaBlinkSpeed, 1f);
            Color sprintBarColor = SprintBar.color;
            sprintBarColor.a = alpha;
            SprintBar.color = sprintBarColor;
        }
        else
        {
            Color sprintBarColor = SprintBar.color;
            sprintBarColor.a = 1f;
            SprintBar.color = sprintBarColor;
        }

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovTransitionSpeed * Time.deltaTime);
        SprintBar.rectTransform.localScale = new Vector3(Sprint, 1f, 1f);

    }
}
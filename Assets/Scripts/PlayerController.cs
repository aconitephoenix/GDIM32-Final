using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [Header("Sprint Settings")]
    [SerializeField] private float sprintDuration = 2f;
    [SerializeField] private Image SprintBar;
    [SerializeField] private Image SprintBarBackground;
    [SerializeField] private float alphaBlinkSpeed = 2f;
    public float Sprint = 1f;
    private bool CanSprint;
    private bool isSprinting = false;
    private bool isJumping = true;

    [Header("Raycast Settings")]
    [SerializeField] private float _lineofSightMaxDist;
    [SerializeField] private Vector3 _raycastStartOffset;
    private bool _lookingAtInteractable = false;

    private string _playerTag = "Player";
    private string _npcTag = "NPC";
    private string _interactableTag = "Interactable";
    
    [SerializeField] private Rigidbody rb;
    private Camera playerCamera;
    private Vector3 velocity;
    private bool isGrounded;
    private float verticalRotation = 0f;

    // Variables for Gizmo drawing
    private Vector3 _raycastHitLocation;

    public delegate void StringDelegate(string str);
    public event StringDelegate InteractableDetected;

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
    
    // Raycasting Methods
    // Vector setting Ray start position to camera's world space position
    private Vector3 _raycastStart {
        get {
            return cameraTransform.position;
        }
    }

    // Vector pointing out from camera
    private Vector3 _raycastDir
    {
        get {
            return (cameraTransform.forward).normalized;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleSprint();
        // temp added this method here for testing -jess
        LookingAtInteractable();
    }

    private void FixedUpdate()
    {
        HandleJump();
    }

    private void HandleMovement()
    {

        // Get input
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        
        // Check if sprinting
        isSprinting = Input.GetKey(KeyCode.LeftShift) && CanSprint && (moveX != 0 || moveZ != 0);
        
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
        rb.MovePosition(rb.position + move.normalized * currentSpeed * Time.deltaTime);

        
        
    }

    private void HandleJump()
    {
        // Check if grounded
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
        if (isGrounded)
        {
            isJumping = false;
        }
        // Jump
        if (Input.GetKey(KeyCode.Space) && isGrounded && !isJumping && rb.velocity.y == 0)
        {
            //rb force jump
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumping = true;
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
            SprintBar.color = new Color(0.678f, 0.996f, 1f); // #ADFEFF
            CanSprint = true;
            //Debug.Log("Sprint ready!");
        }

        // Handle alpha fade based on sprint availability for SprintBar
        Color sprintBarColor = SprintBar.color;
        Color sprintBarBackgroundColor = SprintBarBackground.color;

        if (Sprint >= 1f)
        {
            // Fade to alpha 0 when sprint is full
            sprintBarColor.a = Mathf.Lerp(sprintBarColor.a, 0f, fovTransitionSpeed * Time.deltaTime);
            // Fade to alpha 0 when sprint is full
            sprintBarBackgroundColor.a = Mathf.Lerp(sprintBarBackgroundColor.a, 0f, fovTransitionSpeed * Time.deltaTime);
        }
        else if (!CanSprint)
        {
            // Pulsing alpha when sprint is unavailable
            float alpha = Mathf.PingPong(Time.time * alphaBlinkSpeed, .5f);
            sprintBarColor.a = alpha;
        }
        else
        {
            // Normal alpha when sprint is available but not full
            sprintBarColor.a = .5f;
            // Normal alpha when sprint is available but not full
            sprintBarBackgroundColor.a = .5f;
        }
        SprintBar.color = sprintBarColor;
        SprintBarBackground.color = sprintBarBackgroundColor;

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovTransitionSpeed * Time.deltaTime);
        SprintBar.rectTransform.localScale = new Vector3(Sprint, 1f, 1f);

    }

    // Check if player is looking at something they can interact with
    private bool LookingAtInteractable()
    { 
        RaycastHit hitInfo;

        // Firing raycast out from camera
        if (Physics.Raycast(_raycastStart, _raycastDir, out hitInfo, _lineofSightMaxDist))
        {
            _raycastHitLocation = hitInfo.point;
            if (hitInfo.collider.gameObject.tag.Equals(_interactableTag) || hitInfo.collider.gameObject.tag.Equals(_npcTag))
            {
                _lookingAtInteractable = true;
            }
            // will relocate this later in a diff method probably -jess
            InteractableDetected?.Invoke(hitInfo.collider.gameObject.tag);
        } else
        {
            InteractableDetected?.Invoke("Untagged");
            _lookingAtInteractable = false;
        }

        return _lookingAtInteractable;
    }

    private void OnDrawGizmos()
    {
        // Don't draw gizmos until game is running
        if (!Application.isPlaying) return;

        // Draw Ray from camera
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_raycastStart, _raycastDir * _lineofSightMaxDist);
        Gizmos.DrawSphere(_raycastHitLocation, 0.1f);
    }
}
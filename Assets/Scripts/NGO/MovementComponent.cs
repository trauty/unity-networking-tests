using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class MovementComponent : NetworkBehaviour
{
    [Header("Setup")] 
    [SerializeField] private Transform playerMesh;
    [SerializeField] private GameObject glasses;

    [Header("Camera Movement")]
    [SerializeField] private Transform playerCameraPosition;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 1.0f;

    [Header("Movement")]
    [SerializeField] private float movementSpeed = 10.0f;
    [SerializeField] private float groundDrag = 6.0f;
    [SerializeField] private float airDrag = 2.0f;
    [SerializeField] private float gravityMultiplier = 3.0f;

    [Header("Jumping")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float jumpHeight = 10.0f;
    [SerializeField] private LayerMask groundLayer;

    private MainInput input;
    private Rigidbody rb;

    private Vector3 moveDir = Vector3.zero;
    private float yRot = 0.0f;

    private bool isGrounded = false;

    private void Awake()
    {
        input = new MainInput();
        TryGetComponent(out rb);
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Jump.performed += OnJumpPressed;
        playerCamera.gameObject.SetActive(true);
        glasses.SetActive(false);
    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.Jump.performed -= OnJumpPressed;
        playerCamera.gameObject.SetActive(false);
        glasses.SetActive(true);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        yRot = playerMesh.localRotation.y;

        rb.linearDamping = groundDrag;
    }

    void Update()
    {
        playerCamera.transform.SetPositionAndRotation(playerCameraPosition.position, playerCameraPosition.rotation);

        Vector2 lookInput = 0.1f * mouseSensitivity * input.Player.Look.ReadValue<Vector2>();
        yRot += lookInput.x;

        playerMesh.localEulerAngles = new Vector3(0.0f, yRot, 0.0f);
        playerCameraPosition.transform.localEulerAngles += new Vector3(-lookInput.y, 0.0f, 0.0f);
        playerCameraPosition.transform.localEulerAngles = new Vector3(playerCameraPosition.transform.localEulerAngles.x, yRot, 0.0f);

        Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
        moveDir = playerMesh.forward * moveInput.y + playerMesh.right * moveInput.x;

        isGrounded = Physics.CheckSphere(groundCheck.position, 0.05f, groundLayer);
        rb.linearDamping = isGrounded ? groundDrag : airDrag;
    }

    private void FixedUpdate()
    {
        rb.AddForce(moveDir.normalized * movementSpeed, ForceMode.Acceleration);
        rb.AddForce(Physics.gravity * gravityMultiplier);
    }

    private void OnJumpPressed(InputAction.CallbackContext obj)
    {
        if (isGrounded)
        {
            rb.AddForce(rb.mass * Mathf.Sqrt(2.0f * -Physics.gravity.y * gravityMultiplier * jumpHeight) * Vector3.up, ForceMode.Impulse);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, 0.05f);
    }
}

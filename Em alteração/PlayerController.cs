using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Input Settings")]
    public InputActionAsset actions;

    [Header("Ground")]
    public Transform groundCheck;
    public float groundMask = 0.4f;
    [HideInInspector] public bool isGrounded;
    public float currentSpeed;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Movement Control")]
    public float deceleration = 20f;

    [Header("Camera Follow Settings")]
    public Camera specificCamera; // câmera específica para seguir

    private InputAction m_moveAction;
    private InputAction m_lookAction;
    private InputAction m_jumpAction;
    private Vector2 m_moveInput;
    private Vector2 m_lookInput;
    private Rigidbody rb;
    private Camera cam;
    private Camera lastValidCam;
    private bool wasMovingLastFrame = false; // para detectar quando para/começa a andar
    private Vector3 lastMovementDirection;   // última direção de movimento

    // >>> NOVO: referencia "travada" da câmera quando começa a andar na câmera fixa
    private Vector3 lockedForward;
    private Vector3 lockedRight;

    private void OnEnable()
    {
        actions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        actions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        m_moveAction = actions.FindAction("Move");
        m_lookAction = actions.FindAction("Look");
        m_jumpAction = actions.FindAction("Jump");
        rb = GetComponent<Rigidbody>();

        // Prioriza câmera específica se definida
        if (specificCamera != null)
            cam = specificCamera;
        else
            cam = Camera.main;

        lastValidCam = cam;

        if (cam == null)
            Debug.LogError("Nenhuma câmera encontrada!");
    }

    private void Update()
    {
        m_moveInput = m_moveAction.ReadValue<Vector2>();
        m_lookInput = m_lookAction.ReadValue<Vector2>();

        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundMask,
            LayerMask.GetMask("Terreno")
        );

        if (m_jumpAction.WasPressedThisFrame() && isGrounded)
            jump();

        UpdateCameraReference();
    }

    private void UpdateCameraReference()
    {
        // Se tem câmera específica definida, APENAS usa ela e ignora outras
        if (specificCamera != null)
        {
            cam = specificCamera;
            return;
        }

        // Só se não tiver câmera específica, usa Camera.main com backup
        Camera newCam = Camera.main;

        if (newCam != null)
        {
            cam = newCam;
            lastValidCam = newCam;
        }
        else if (cam == null && lastValidCam != null)
        {
            cam = lastValidCam;
        }
    }

    public void jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        UpdateCameraReference();
        walking();
        Rotating();
    }

    private void walking()
    {
        if (cam == null)
        {
            Debug.LogWarning("Câmera é null!");
            return;
        }

        bool hasInput = m_moveInput.magnitude > 0.01f;
        bool justStartedMoving = hasInput && !wasMovingLastFrame;

        if (hasInput)
        {
            if (cam.name == "WalkingCamera")
            {
                
                Vector3 forward = cam.transform.forward;
                Vector3 right = cam.transform.right;
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();

                Vector3 inputDirection = (m_moveInput.x * right + m_moveInput.y * forward).normalized;

                Vector3 targetVelocity = inputDirection * moveSpeed;
                targetVelocity.y = rb.linearVelocity.y;
                rb.linearVelocity = targetVelocity;

                if (inputDirection.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
                    rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime));
                }
            }
            else
            {
                
                if (justStartedMoving)
                {
                    lockedForward = cam.transform.forward;
                    lockedRight = cam.transform.right;
                    lockedForward.y = 0;
                    lockedRight.y = 0;
                    lockedForward.Normalize();
                    lockedRight.Normalize();
                }

                // 2) Usa input atual + referência travada para permitir diagonais
                Vector3 inputDirection = (m_moveInput.x * lockedRight + m_moveInput.y * lockedForward).normalized;

                // Move na direção calculada
                Vector3 targetVelocity = inputDirection * moveSpeed;
                targetVelocity.y = rb.linearVelocity.y;
                rb.linearVelocity = targetVelocity;

                // Rotaciona suavemente para direção de movimento
                if (inputDirection.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(inputDirection, Vector3.up);
                    rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime));
                }
            }
        }
        else
        {
            // Para o movimento aplicando desaceleração
            Vector3 currentVelocity = rb.linearVelocity;
            Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector3(horizontalVelocity.x, currentVelocity.y, horizontalVelocity.z);

            // Quando parar completamente, libera referência da câmera
            lockedForward = Vector3.zero;
            lockedRight = Vector3.zero;
        }

        wasMovingLastFrame = hasInput;

        // Velocidade atual (somente horizontal)
        currentSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
    }


 
}

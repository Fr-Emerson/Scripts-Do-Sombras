using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMouse : MonoBehaviour
{
    [Header("Input Settings")]
    public InputActionAsset actions;
    private InputAction lookAction;

    [Header("Camera Settings")]
    public float rotationSpeed = 200f;
    [Range(0.1f, 5.0f)]
    public float mouseSensitivity = 1.0f; 
    public Vector2 CameraAngleDefault = new Vector2(40f, 0f);
    public bool UseLimits = false;
    public Vector2 CameraAngleLimits = new Vector2(0, 0);

    [Header("Smooth Movement")]
    [Range(1f, 20f)]
    public float smoothSpeed = 10f;
    public bool useSmoothMovement = true;

    [Header("Player Follow Settings")]
    public Transform playerTarget;
    public Vector3 offset = new Vector3(0, 2, -5);
    public float followSpeed = 5f;
    public bool followPlayer = true;

    [Header("Distance Control")]
    public float minDistance = 2f;
    public float maxDistance = 10f;
    public float currentDistance = 5f;

    // Variáveis privadas
    private float currentYRotation = 0f;
    private float targetYRotation = 0f;
    private Vector3 targetPosition;

    void Start()
    {
        lookAction = actions.FindAction("Look");
        lookAction.Enable();

        currentYRotation = CameraAngleDefault.y;
        targetYRotation = currentYRotation;

        if (playerTarget == null && followPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTarget = player.transform;
        }

        currentDistance = offset.magnitude;
    }

    void Update()
    {
        HandleMouseInput();
        UpdateCameraPosition();
        UpdateCameraRotation();
    }

    void HandleMouseInput()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        lookInput *= mouseSensitivity;

        targetYRotation += lookInput.x * rotationSpeed * Time.deltaTime;

        if (UseLimits)
        {
            targetYRotation = math.clamp(targetYRotation, CameraAngleLimits.x, CameraAngleLimits.y);
        }
    }

    void UpdateCameraPosition()
    {
        if (!followPlayer || playerTarget == null) return;

        Quaternion rotation = Quaternion.Euler(CameraAngleDefault.x, targetYRotation, 0);
        Vector3 rotatedOffset = rotation * new Vector3(0, offset.y, -currentDistance);

        targetPosition = playerTarget.position + rotatedOffset;

        CheckForObstacles();

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    void UpdateCameraRotation()
    {
        Quaternion targetRotation = Quaternion.Euler(CameraAngleDefault.x, targetYRotation, 0);

        if (useSmoothMovement)
        {
            currentYRotation = Mathf.Lerp(currentYRotation, targetYRotation, smoothSpeed * Time.deltaTime);
        }
        else
        {
            currentYRotation = targetYRotation;
        }

        if (followPlayer && playerTarget != null)
        {
            // Sempre olha pro player
            transform.LookAt(playerTarget.position + Vector3.up * offset.y * 0.5f);
        }
        else
        {
            // Caso não siga player
            transform.rotation = useSmoothMovement
                ? Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime)
                : targetRotation;
        }
    }

    void CheckForObstacles()
    {
        if (!followPlayer || playerTarget == null) return;

        Vector3 directionFromPlayer = (targetPosition - playerTarget.position).normalized;
        float distanceToTarget = Vector3.Distance(playerTarget.position, targetPosition);

        RaycastHit hit;
        if (Physics.Raycast(playerTarget.position, directionFromPlayer, out hit, distanceToTarget))
        {
            targetPosition = hit.point - directionFromPlayer * 0.5f;
            currentDistance = Mathf.Max(Vector3.Distance(playerTarget.position, targetPosition), minDistance);
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, offset.magnitude, Time.deltaTime);
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }
    }

    public void SetSensitivity(float newSensitivity)
    {
        mouseSensitivity = Mathf.Clamp(newSensitivity, 0.1f, 5.0f);
    }

    public void ToggleSmoothMovement()
    {
        useSmoothMovement = !useSmoothMovement;
    }

    public void ResetCamera()
    {
        currentYRotation = CameraAngleDefault.y;
        targetYRotation = currentYRotation;
        currentDistance = offset.magnitude;
    }

    void OnDisable()
    {
        lookAction?.Disable();
    }
}

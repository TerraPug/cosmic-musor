using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpacePlayerController : MonoBehaviour
{
    [Header("Flight")]
    [SerializeField] private float acceleration = 18f;
    [SerializeField] private float maxSpeed = 9f;
    [SerializeField] private float mouseSensitivity = 2.2f;

    private Rigidbody body;
    private float pitch;
    private float yaw;
    private Vector3 movementInput;

    public Vector3 Velocity => body != null ? body.linearVelocity : Vector3.zero;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.linearDamping = 1.4f;
        body.angularDamping = 6f;
        body.freezeRotation = true;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Vector3 angles = transform.eulerAngles;
        pitch = angles.x;
        yaw = angles.y;
    }
    private void Update()
    {
        movementInput = Vector3.zero;
        if (!Application.isFocused) return;
        if (SpaceGameInput.EscapePressed)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        if (SpaceGameInput.MousePressed(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        movementInput = SpaceGameInput.Movement;
        Vector2 look = SpaceGameInput.Look;
        yaw += look.x * mouseSensitivity;
        pitch -= look.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -85f, 85f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void FixedUpdate()
    {
        body.AddForce(transform.TransformDirection(movementInput) * acceleration, ForceMode.Acceleration);

        if (body.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            body.linearVelocity = body.linearVelocity.normalized * maxSpeed;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) ReleaseControl();
    }

    private void OnDisable()
    {
        ReleaseControl();
    }

    private void ReleaseControl()
    {
        movementInput = Vector3.zero;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}

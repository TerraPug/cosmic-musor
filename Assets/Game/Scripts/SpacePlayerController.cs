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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -85f, 85f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void FixedUpdate()
    {
        float vertical = 0f;
        if (Input.GetKey(KeyCode.Space)) vertical += 1f;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C)) vertical -= 1f;

        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), vertical, Input.GetAxisRaw("Vertical"));
        input = Vector3.ClampMagnitude(input, 1f);
        body.AddForce(transform.TransformDirection(input) * acceleration, ForceMode.Acceleration);

        if (body.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            body.linearVelocity = body.linearVelocity.normalized * maxSpeed;
        }
    }
}
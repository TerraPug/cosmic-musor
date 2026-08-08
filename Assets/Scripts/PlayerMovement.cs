using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 6.0f;
    public float runSpeed = 10.0f;
    public float jumpHeight = 2.0f;
    public float gravity = -9.81f;

    public Transform playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 85.0f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (playerCamera != null)
        {
            rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float speed = isRunning ? runSpeed : walkSpeed;

        float curSpeedX = speed * Input.GetAxis("Vertical");
        float curSpeedY = speed * Input.GetAxis("Horizontal");
        float movementDirectionY = moveDirection.y;
        
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (characterController.isGrounded)
        {
            moveDirection.y = -0.5f; 

            if (Input.GetButtonDown("Jump"))
            {
                moveDirection.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            moveDirection.y = movementDirectionY + (gravity * Time.deltaTime);
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }
}

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public Transform playerCamera;
    public Transform groundChecker;
    public LayerMask groundMask;
    public float groundRadious = .4f;
    public float gravity = -9.8f;
    public float jumpForce = 3f;
    
    Vector3 velocity;
    public float speed = 6f;
    float smoothRotationForce = .1f;
    float smoothRotationVelocity;
    bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundChecker.position, groundRadious, groundMask);

        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");
        
        var direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= .1f)
        {
            var targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
            var desiredAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref smoothRotationVelocity, smoothRotationForce);
            transform.rotation = Quaternion.Euler(0, desiredAngle, 0);

            var unit = vertical < 0 ? Vector3.back : Vector3.forward;
            var movDirection = Quaternion.Euler(0, desiredAngle, 0) * unit;
            controller.Move(movDirection * speed * Time.deltaTime);
        }

        if(Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpForce * -2 * gravity);

        velocity.y += gravity * Time.deltaTime;
        
        controller.Move(velocity * Time.deltaTime);
    }
}

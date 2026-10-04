using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    public float groundCheckDistance = 0.2f;

    private CharacterController controller;
    private Vector3 velocity;
    private Animator animator;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Transform cam = Camera.main.transform;

        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * z + right * x;

        if (move.sqrMagnitude > 0.01f)
        {
            controller.Move(move.normalized * speed * Time.deltaTime);

            if (z > 0.1f)
                transform.forward = forward;
        }

        bool grounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            controller.height / 2f + groundCheckDistance
        );

        if (grounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (animator != null)
        {
            animator.SetFloat("InputX", x);
            animator.SetFloat("InputY", z);
            animator.SetFloat("Blend", move.magnitude);
            animator.SetBool("IsInAir", !grounded);
        }
    }
}


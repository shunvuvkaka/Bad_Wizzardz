
using NUnit.Framework.Internal.Filters;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    public float groundDrag;
    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update() 
    {
        // Ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();

        // Handle drag

        if (grounded)
        {
            rb.linearDamping = groundDrag;
        } else 
        {
            rb.linearDamping = 0;
        }
    }
    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput() 
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }
    private void MovePlayer() 
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }
}

using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody playerRB;

    public Transform playerTransform;   // player object
    public Transform cameraTransform;   // main camera

    public float shiftSpeed;

    private Vector2 horizontalMovement;
    private float verticalMovement;
    public float velocityScaling = 10f;
    public float jumpVelocity = 9.81f;

    private bool isGrounded;

    private bool isInvert = false;
    private readonly KeyCode invertKey = KeyCode.Q;
    private int direction = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRB = GetComponent<Rigidbody>();
        playerTransform = GetComponent<Transform>();
        cameraTransform = Camera.main.transform;
        horizontalMovement = Vector2.zero;
    }

    // Update is called once per frame
    private void Update()
    {
        horizontalMovement = CalculateHorizontalMovementVector();
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
        verticalMovement = CalculateVerticalMovementVector();
    }

    // FixedUpdate is called every fixed amount of DeltaTime 
    // Default: 20ms
    void FixedUpdate()
    {
        if(Input.GetKey(invertKey) && !isInvert)
        {
            playerTransform.Rotate(0f, 180f, 0f);
            direction = -1;
            isInvert = true;
        }
        else if (!Input.GetKey(invertKey) && isInvert)
        {
            playerTransform.Rotate(0f, 180f, 0f);
            direction = 1;
            isInvert = false;
        }    

        
        Vector3 velocity = playerRB.linearVelocity;

        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();
       
        Vector3 movement = direction * forward * horizontalMovement.y + direction * right * horizontalMovement.x;

        velocity.x = movement.x;
        velocity.z = movement.z;
        velocity.y += verticalMovement;
        
        playerRB.linearVelocity = velocity;
    }

    /* 
     * Function Calculates player movement vector in the x and z axis 
     * Input: None 
     * Ouput: 2d vector representing the velocity of the player in the x and z axis 
     */
    private Vector2 CalculateHorizontalMovementVector()
    {
        float xAxis = Input.GetAxisRaw("Horizontal");
        float zAxis = Input.GetAxisRaw("Vertical");

        bool shiftFlag = Input.GetKey(KeyCode.LeftShift);
        float shiftScaling = shiftFlag ? shiftSpeed : 1f;

        Vector2 res = new Vector2(xAxis, zAxis);

        res.Normalize();
        res *= velocityScaling * shiftScaling;
        

        return res;
    }

    /*
     * Function calculates the vertical velocity of the player
     * Input: None
     * Output: float velocity
     */
    private float CalculateVerticalMovementVector()
    {
        if (Input.GetKey(KeyCode.Space) && isGrounded) { return jumpVelocity; }

        return 0f;
    }
}
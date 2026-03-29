using System;
using System.Security.Cryptography;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    //Player
    private Rigidbody playerRB;
    public Transform playerTransform;   // player object
    public Transform cameraTransform;   // main camera

    //Regular Movement
    private Vector2 horizontalMovement;
    private float verticalMovement;
    private readonly float velocityScaling = 160f; //tweak for difference in speed
    private readonly float jumpVelocity = 9.81f * 1.5f;
    private readonly float acceleration = 0.25f; //tweak for difference in the "weight" of key presses on velocity and also speed 
    private bool isGrounded;

    //Inverted Stuff
    private bool isInvert = false;
    private readonly KeyCode invertKey = KeyCode.Q;
    private int direction = 1;

    //Climb
    private bool isLatched = false;

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
        if (Input.GetKey(invertKey) && !isInvert)
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

        if (!isGrounded && Input.GetKey(KeyCode.Space) && !isLatched)
        {
            TryGrabbingClimbPoint();
        }

        if (isLatched && Input.GetKey(KeyCode.Space))
        {
            JumpOffClimbPoint();
            isLatched = false;
        }

        playerRB.constraints = isLatched ? RigidbodyConstraints.FreezePosition : RigidbodyConstraints.None;     //Player is unable to move while latched



        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 movement = direction * forward * horizontalMovement.y + direction * right * horizontalMovement.x;


        Vector3 desiredVelocity = movement;
        Vector3 currentVelocity = playerRB.linearVelocity;

        Vector3 velocityDiff = desiredVelocity - new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        if (!isGrounded)
        {
            playerRB.AddForce(Physics.gravity * 2.25f, ForceMode.Acceleration);
        }

        Vector3 force = velocityDiff * acceleration;
        playerRB.AddForce(force, ForceMode.Acceleration);

        if (verticalMovement > 0f)
        {
            playerRB.AddForce(Vector3.up * jumpVelocity, ForceMode.Impulse);
        }
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
        float shiftScaling = shiftFlag ? 2f : 1f;

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


    /*
     * Function Attmepts To Latch Onto A Climbing Point
     * Input: None
     * Output: Latched Or Not
     */
    private bool TryGrabbingClimbPoint()
    {

        RaycastHit hit;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 3f))
        {
            //ClimbPoint climb = hit.collider.GetComponentInParent<ClimbPoint>();

            //if (climb != null)
            //{
            //   isLatched = true;
            //}
        }
        return isLatched;
    }

    private void JumpOffClimbPoint()
    {
        Vector3 JumpDir = cameraTransform.forward;

        JumpDir *= 10f;

        playerRB.AddForce(JumpDir, ForceMode.Impulse);
    }
}
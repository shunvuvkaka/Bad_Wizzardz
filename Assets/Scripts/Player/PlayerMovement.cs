using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour, IDamageable
{
    private Rigidbody playerRB;

    public Transform playerTransform;   // player object
    public Transform cameraTransform;   // main camera
    public PlayerCamera cam;

    public float shiftSpeed;

    private Vector2 horizontalMovement;
    private float verticalMovement;
    public float velocityScaling = 10f;
    public float jumpVelocity = 9.81f;
    public float airFriction;
    public float standardFriction;
    public float fallingGravity;
    public float jumpDist;
    public float jumpBuffer;
    public float coyoteTime;
    public float airMovement;

    [SerializeField] private bool isGrounded;
    private bool prevGrounded;
    private bool canJump;
    private bool canMove = true;
    private Vector2 prevMovement;

    private bool isInvert = false;
    private readonly KeyCode invertKey = KeyCode.Q;
    private int direction = 1;

    public static PlayerMovement Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
        playerRB = GetComponent<Rigidbody>();
        playerTransform = GetComponent<Transform>();
        cameraTransform = Camera.main.transform;
        horizontalMovement = Vector2.zero;
    }

    // Update is called once per frame
    private void Update()
    {
        horizontalMovement = CalculateHorizontalMovementVector();   
        isGrounded = Physics.Raycast(transform.position, Vector3.down, jumpDist);

        MyInput();

        if (prevGrounded != isGrounded && prevGrounded == true)
        {
            StartCoroutine(CoyoteTime());
        }

        prevGrounded = isGrounded;

        verticalMovement = CalculateVerticalMovementVector(canJump);

        Debug.DrawRay(transform.position, -transform.up * jumpDist, Color.blanchedAlmond);
        
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

        if (playerRB.linearVelocity.y < 0)
        {
            playerRB.AddForce(Physics.gravity * fallingGravity, ForceMode.Acceleration);
        }
        else
        {
            playerRB.AddForce(Physics.gravity, ForceMode.Acceleration);
        }

        if (isGrounded)
        {
            playerRB.linearDamping = standardFriction;
        }
        else
        {
            playerRB.linearDamping = airFriction;
        }    

        
        Vector3 velocity = playerRB.linearVelocity;

        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();
       
        Vector3 movement = direction * horizontalMovement.y * forward + direction * horizontalMovement.x * right;

        velocity.x = movement.x;
        velocity.z = movement.z;
        velocity.y = verticalMovement;
        
        playerRB.AddForce(velocity, ForceMode.VelocityChange);
    }

    /* 
     * Function Calculates player movement vector in the x and z axis 
     * Input: None 
     * Ouput: 2d vector representing the velocity of the player in the x and z axis 
     */
    private Vector2 CalculateHorizontalMovementVector()
    {
        if (!canMove)
        {
            return prevMovement *= isGrounded ? 1 : airMovement;
        }

        float xAxis = Input.GetAxisRaw("Horizontal");
        float zAxis = Input.GetAxisRaw("Vertical");

        bool shiftFlag = Input.GetKey(KeyCode.LeftShift);
        float shiftScaling = shiftFlag ? shiftSpeed : 1f;

        Vector2 res = new Vector2(xAxis, zAxis);

        res.Normalize();
        res *= velocityScaling * shiftScaling;

        if (!isGrounded)
            res *= airMovement;
        
        prevMovement = res;

        return res;
    }

    /*
     * Function calculates the vertical velocity of the player
     * Input: None
     * Output: float velocity
     */
    private float CalculateVerticalMovementVector(bool grounded)
    {
        if (!canMove)
            return 0f;
            
        if (Input.GetKey(KeyCode.Space) && grounded) { return jumpVelocity; }

        if (Input.GetKey(KeyCode.Space) && !grounded)
            StartCoroutine(JumpBuffer());

        return 0f;
    }

    IEnumerator JumpBuffer()
    {
        float time = jumpBuffer;

        while (time > 0)
        {
            yield return null;

            time -= Time.deltaTime;

            if (isGrounded)
            {
                verticalMovement = jumpVelocity;
            }
        } 
    }
    IEnumerator CoyoteTime()
    {
        float time = coyoteTime;

        while (time > 0)
        {
            yield return null;

            time -= Time.deltaTime;

            canJump = true;

            if (Input.GetKey(KeyCode.Space))
                break;
        }

        canJump = isGrounded; 
    }

    void SetCursorToCenter()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Mouse.current.WarpCursorPosition(screenCenter);
    }

    public void MyInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && GameUI.Instance.currentState == GameUI.UIState.NotCasting)
        {
            Time.timeScale = 0.5f;
            DrawGlyph.Instance.casting = true;
            cam.casting = true;
            canMove = false;
            Cursor.lockState = CursorLockMode.None;
            SetCursorToCenter();
            GameUI.Instance.currentState = GameUI.UIState.Casting;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl) && GameUI.Instance.currentState == GameUI.UIState.Casting)
        {
            SetCursorToCenter();
            Time.timeScale = 1f;
            DrawGlyph.Instance.casting = false;
            cam.casting = false;
            canMove = true;
            GameUI.Instance.currentState = GameUI.UIState.NotCasting;
        }

        if (Input.GetKeyDown(KeyCode.F) && (GameUI.Instance.currentState == GameUI.UIState.NotCasting || GameUI.Instance.currentState == GameUI.UIState.Viewing))
        {
            ToggleObject();
        }
        
        if (Input.GetKeyDown(KeyCode.Tab) && (GameUI.Instance.currentState == GameUI.UIState.NotCasting || GameUI.Instance.currentState == GameUI.UIState.Paused))
        {
            TogglePause();
        }
    }

    void ToggleObject()
    {
        ObjectUI.Instance.SelectObject(ObjectUI.Instance.baseImage);

        PlayerAnimation.Instance.selecting = !PlayerAnimation.Instance.selecting;
        PlayerAnimation.Instance.Object(PlayerAnimation.Instance.selecting);

        if (PlayerAnimation.Instance.selecting)
            GameUI.Instance.currentState = GameUI.UIState.Viewing;
        else
            GameUI.Instance.currentState = GameUI.UIState.NotCasting;

        cam.casting = PlayerAnimation.Instance.selecting;

        canMove = !canMove;
    }

    public void TogglePause()
    {
        canMove = !canMove;
        cam.casting = !cam.casting;

        if (GameUI.Instance.currentState == GameUI.UIState.Paused)
            GameUI.Instance.currentState = GameUI.UIState.NotCasting;
        else
            GameUI.Instance.currentState = GameUI.UIState.Paused;
    }

    public void Damage(float damage)
    {
        PlayerStats.Instance.Health -= damage;

        if (PlayerStats.Instance.Health < 0)
            PlayerAnimation.Instance.Dead();
    }
}
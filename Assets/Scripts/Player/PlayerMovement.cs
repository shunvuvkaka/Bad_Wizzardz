using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour, IDamageable
{
    // ===== STATE =====
    public enum PlayerState
    {
        Normal,
        Climbing,
        Casting,
        Viewing,
        Paused
    }

    private PlayerState currentState = PlayerState.Normal;

    // ===== REFERENCES =====
    public Transform cameraTransform;
    public Transform playerTransform;
    public PlayerCamera cam;
    private Rigidbody playerRB;
    public static PlayerMovement Instance;
    public DrawGlyph Glyph;

    public float shiftSpeed;

    private Vector2 horizontalMovement;
    private float verticalMovement;
    public float velocityScaling = 160f;
    public float jumpVelocity = 9.81f;
    public float airFriction;
    public float standardFriction;
    public float fallingGravity;
    public float jumpDist;
    public float jumpBuffer;
    public int jumpPause;
    public float airMovement;
    public float walkAudioSpeed;
    private float horizontalinput;
    private float verticalinput;
    public float moveSpeed;
    public float InputTimeDelay = 1;

    [SerializeField] private bool isGrounded;
    private bool prevGrounded;
    private bool canJump;
    private bool control;
    private bool canMove = true;
    private Vector2 prevMovement;
    private readonly float acceleration = 0.25f; //tweak for difference in the "weight" of key presses on velocity and also speed 

    public float playerHeight;
    public LayerMask whatIsGround;

    private bool isInvert = false;
    private readonly KeyCode invertKey = KeyCode.Q;
    private int direction = 1;
    private int currentPuase = -1;

    // ===== JUMP =====
    public float jumpForce = 8f;
    public float coyoteTime = 0.15f;
    private float coyoteTimer;

    // ===== GROUND =====
    public float groundCheckDistance = 1.1f;
    public LayerMask groundLayer;

    // ===== CLIMB =====
    public float climbRange = 3f;

    // ===== INPUT =====
    private Vector2 input;


    public Vector2 res;
    private bool shiftFlag;
    Vector3 moveDirection;

    void Awake()
    {
        playerRB = GetComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;
        Instance = this;
    }

    void Update()
    {
        HandleInput();

        if (currentState == PlayerState.Paused)
            return;

        // ===== MOVEMENT INPUT =====
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        // ===== GROUND CHECK =====
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        // ===== JUMP / CLIMB INPUT =====
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentState == PlayerState.Climbing)
            {
                ExitClimb();
            }
            else if (coyoteTimer > 0f && currentState == PlayerState.Normal)
            {
                Jump();
            }
            else if (!isGrounded && currentState == PlayerState.Normal)
            {
                TryClimb();
            }
        }

    }

    void FixedUpdate()
    {
        if (currentState != PlayerState.Normal)
        {
            playerRB.linearVelocity = new Vector3(0f, playerRB.linearVelocity.y, 0f);
            return;
        }

        Move();
    }

    void SetCursorToCenter() 
    {
        if (control == true)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }


        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        control = true;
    }

    // ===== MOVEMENT =====
    void Move()
    {
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        horizontalinput = Input.GetAxisRaw("Horizontal");
        verticalinput = Input.GetAxisRaw("Vertical");

        moveDirection = playerTransform.forward * verticalinput + playerTransform.right * horizontalinput;

        playerRB.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);



        Vector3 moveDir = forward * input.y + right * input.x;
        Vector3 desiredVelocity = moveDir * velocityScaling;

        Vector3 currentVelocity = playerRB.linearVelocity;
        Vector3 currentHorizontal = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        Vector3 velocityDiff = desiredVelocity - currentHorizontal;

        if (verticalMovement > 0f && currentPuase < 0)
        {
            playerRB.linearVelocity += Vector3.up * verticalMovement;
            isGrounded = false;
            Debug.Log("jump force applied");
            currentPuase = jumpPause;
        }

        if (!isGrounded)
        {
            playerRB.linearDamping = airFriction;
            playerRB.AddForce(Physics.gravity * fallingGravity, ForceMode.Acceleration);
        }
        else
        {
            playerRB.linearDamping = standardFriction;
        }


        currentPuase --;

        Vector3 force = velocityDiff * acceleration;
        playerRB.AddForce(force, ForceMode.Acceleration);
    }

    // ===== JUMP =====
    void Jump()
    {
        if (!canMove)
        {
            if (prevMovement != Vector2.zero && isGrounded)
            {
                PlayerAudio.Instance.isWalking = true;
            }
            else
            {
                PlayerAudio.Instance.isWalking = false;
            }

          prevMovement *= isGrounded ? 1 : airMovement;
        }
    }

    // ===== CLIMB =====
    void TryClimb()
    {
        RaycastHit hit;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, climbRange))
        {
            //ClimbPoint climb = hit.collider.GetComponentInParent<ClimbPoint>();

            //if (climb != null)
            //{
            //    EnterClimb(climb);
            //}
        }


        //void EnterClimb(ClimbPoint climb)
        //{
        //    currentState = PlayerState.Climbing;

        if (!isGrounded)
            res *= airMovement;

        if (res != Vector2.zero && isGrounded)
        {
            PlayerAudio.Instance.isWalking = true;
            PlayerAudio.Instance.walkSpeed = shiftFlag ? walkAudioSpeed / shiftSpeed : walkAudioSpeed;
        }
        else
        {
            PlayerAudio.Instance.isWalking = false;
        }

        prevMovement = res;
    }
    //    transform.position = climb.snapPosition;
    //    transform.forward = -climb.climbNormal;
    //}

    void ExitClimb()
    {
        currentState = PlayerState.Normal;

        playerRB.useGravity = true;

        Vector3 jumpDir = (cameraTransform.forward + Vector3.up).normalized;
        playerRB.AddForce(jumpDir * jumpForce, ForceMode.Impulse);
    }

    // ===== INPUT SYSTEM =====
    void HandleInput()
    {
        // CASTING
        if (Input.GetKeyDown(KeyCode.LeftControl) && currentState == PlayerState.Normal)
            EnterCasting();

        if (Input.GetKeyUp(KeyCode.LeftControl) && currentState == PlayerState.Casting)
            ExitCasting();

        // OBJECT VIEW
        if (Input.GetKeyDown(KeyCode.F) &&
            (currentState == PlayerState.Normal || currentState == PlayerState.Viewing))
            ToggleObject();
      
     

        // PAUSE
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (currentState == PlayerState.Normal)
            {
                TogglePause(); 
            }
            else if (currentState == PlayerState.Paused)
            {
                EndPause();
            }
        }
            
     
    }   

    // ===== CASTING =====
    void EnterCasting()
    {
        currentState = PlayerState.Casting; 

        Time.timeScale = 0.5f;
        DrawGlyph.Instance.casting = true;
        cam.casting = true;

        Cursor.lockState = CursorLockMode.None;
        SetCursorToCenter();
    }

    void ExitCasting()
    {
        currentState = PlayerState.Normal;
    }
    public void MyInput()
    {
        if (GameUI.Instance.currentState == GameUI.UIState.Dead)
        {
            return;
        }
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

        SetCursorToCenter();
    }

    // ===== OBJECT =====
    void ToggleObject()
    {
        bool selecting = !PlayerAnimation.Instance.selecting;

        PlayerAnimation.Instance.selecting = selecting;
        PlayerAnimation.Instance.Object(selecting);

        ObjectUI.Instance.SelectObject(ObjectUI.Instance.baseImage);

    
        cam.casting = selecting;

        currentState = selecting ? PlayerState.Viewing : PlayerState.Normal;

        InputTimeDelay *= -1;

        if (InputTimeDelay == -1)
        {
            Invoke(nameof(Pause), 0.8f);
        }
        else 
        {
            Time.timeScale = 1f;
        }

    }

    // ===== PAUSE =====
    public void TogglePause()
    {
        canMove = !canMove;
       cam.casting = false;

       if (GameUI.Instance.currentState == GameUI.UIState.Paused || GameUI.Instance.currentState == GameUI.UIState.Settings)
           GameUI.Instance.currentState = GameUI.UIState.NotCasting;
       else 
       {
          currentState = PlayerState.Paused;
            Pause();
      }
      

        cam.casting = (currentState == PlayerState.Paused);

        Glyph.CanCast = false;
    }

    public void EndPause() 
    {
        SetCursorToCenter();
        canMove = true;
        //Glyph.CanCast = true;

        if (GameUI.Instance.currentState == GameUI.UIState.Paused || GameUI.Instance.currentState == GameUI.UIState.Settings)
            GameUI.Instance.currentState = GameUI.UIState.NotCasting;

            currentState = PlayerState.Normal;
            Time.timeScale = 1f;
      

        cam.casting = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true; 
    }

    // ===== DAMAGE =====
    public void Damage(float damage)
    {
        PlayerStats.Instance.Health -= damage;

        PlayerAnimation.Instance.Hit();

        if (PlayerStats.Instance.Health < 0)
        {
            PlayerAnimation.Instance.Dead();
        }
    }
    public void Pause() 
    {
        Time.timeScale = 0f;
    }
}
using UnityEngine.InputSystem;
using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    #region Input Action References
    public InputActionReference movementAction;
    public InputActionReference jumpAction;
    public InputActionReference bounceAction;
    private Rigidbody2D rb;
    #endregion

    #region Variables
    public Vector2 startPosition = new Vector2(-14, 0);
    public CheckpointManager checkpointManager;
    public int maxHealth = 5;
    public int healthCount;

    //Movement Variables
    public float moveSpeed = 14f;
    public float acceleration = 5f;
    public float deceleration = 20f;
    public float velPower = 0.9f;
    public bool facingRight;

    //Jump Variables
    public float jumpForce = 5f;
    public float jumpSpeedMultiplier = 3.2f;
    private bool isJumping = false;
    public float jumpCoyoteTime = 0.15f;
    public float jumpBufferTime = 0.1f;
    public float jumpCutMultiplier = 0.1f;
    private float gravityScale;
    public float fallGravityMultiplier = 2.5f;
    private float lastGroundedTime = 0f;
    private float lastJumpTime = 0f;
    
    public float jumpMoveSpeed = 8f;
    public float jumpAcceleration = 20f;
    public float jumpDeceleration = 0.1f;
    public float jumpVelPower = 1f;
    public int shortJumpTimeThreshold = 45;
    public float shortJumpReduction = 1.05f;

    //Bounce Variables
    public int bouncesRemaining = 0;
    public int maxBounces = 7;
    public bool isBouncing;

    public float bounceMomentum = 1700f;
    public float bounceMomentumDecayRate = 120f;
    public float currentBounceMomentum = 0f;

    public float bounceMass = 9f;
    public float regularMass = 1f;
    public float bounceDrag = 2.5f;
    public float regularDrag = 0.5f;
    private Vector2 bounceDirection;
    public float bounceMoveMultiplier = 300f;
    public float bounceGravityMultiplier = 2f;
    public float bounceFallGravityMultiplier = 5f;
    private float bounceHorizontalInfluence;
    private float bounceVerticalInfluence;
    public float horizontalInfluenceMultiplier = 0.4f;
    public float verticalInfluenceMultiplier = 2f;
    public float extraMomentumBuildRate = 20f;
    public float extraMomentum;
    public float afterBounceAirTime = 0.1f;
    public float currentAfterBounceAirTime = 0f;
    public float afterBounceAirTimeGravityMultiplier = 0.02f;
    public float verticleBounceMomentumLoss = 5f;
    public float horizontalBounceMomentumLoss = 5f;
    public float leftoverMomentumLossRate = 1.5f;
    private float jumpPressTime = 0f;
    #endregion

    //Inputs
    private Vector2 moveInput;
    private bool jumpWasReleased;
    private bool jumpIsPressedInput;
    private bool bounceInput;

    //Ground Check
    public LayerMask groundLayer;
    private bool isGrounded;
    private Vector2 groundCheckBoxSize = new Vector2(0.7f, 0.1f);

    //Collider References
    private CircleCollider2D bounceModeCollider;
    private PolygonCollider2D regularModeCollider;

    //Sprites
    private SpriteRenderer spriteRenderer;
    public Sprite bounceMode;
    public Sprite normalMode;


    //Testing variables
    public InputActionReference cheatmodeAction;
    public float timescale = 1f;
    public bool spawnAtStart = true;
    public bool cheatmode = false;


    public float iFrames = 1f;
    public float iFrameTimer;
    // Start is called before the first frame update
    void Awake()
    {
        facingRight = true;
        healthCount = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bounceModeCollider = GetComponent<CircleCollider2D>();
        regularModeCollider = GetComponent<PolygonCollider2D>();
        iFrameTimer = 0f;
        currentBounceMomentum = 0f;
        extraMomentum = 0f;
        bounceDirection = Vector2.zero;
        currentAfterBounceAirTime = 0f;
        jumpWasReleased = true;
        spriteRenderer.sprite = normalMode;
        isBouncing = false;
        isJumping = false;
        gravityScale = rb.gravityScale;
        bounceModeCollider.enabled = isBouncing;
        regularModeCollider.enabled = !isBouncing;
        if (spawnAtStart)
        {
            transform.position = startPosition;
        }
    }
    private void OnEnable()
    {
        movementAction.action.Enable();
        jumpAction.action.Enable();
        bounceAction.action.Enable();
        cheatmodeAction.action.Enable();
    }
    private void OnDisable()
    {
        movementAction.action.Disable();
        jumpAction.action.Disable();
        bounceAction.action.Disable();
        cheatmodeAction.action.Disable();
    }
    private void Update()
    {
        #region Inputs
        moveInput = movementAction.action.ReadValue<Vector2>();
        jumpIsPressedInput = jumpAction.action.IsPressed();
        bounceInput = bounceAction.action.WasPressedThisFrame();
        #endregion
        CheckGrounded();
        #region Bounce Influence Input
        if (jumpIsPressedInput)
        {
            jumpPressTime++;
        }
        else if (jumpWasReleased)
        {
            jumpPressTime = 0;
        }
        if (bounceInput)
        {
            if (bouncesRemaining >= 0)
            {
                isBouncing = !isBouncing;
            }
        }
        if (isBouncing)
        {
            bounceHorizontalInfluence = Mathf.Round(moveInput.x);
            bounceVerticalInfluence = Mathf.Round(moveInput.y);
        }
        #endregion
        if (cheatmodeAction.action.WasPressedThisFrame())
        {
            cheatmode = !cheatmode;
        }
        if (moveInput.x < 0)
        {
            facingRight = false;
        }
        if (moveInput.x > 0)
        {
            facingRight = true;
        }
        if (bounceDirection.x < 0)
        {
            facingRight = false;
        }
        if (bounceDirection.x > 0)
        {
            facingRight = true;
        }
        spriteRenderer.flipX = !facingRight;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        Time.timeScale = timescale;
        TickTimers();
        Move();
        ToggleBounceMode();
        #region Jump Logic
        if (jumpWasReleased && jumpIsPressedInput && lastGroundedTime > 0 && lastJumpTime < 0 && !isBouncing && !isJumping)
        {
            rb.velocity = new Vector2(rb.velocity.x/2f, rb.velocity.y);
            Jump();
            jumpWasReleased = false;
        }
        if (!jumpIsPressedInput && isJumping && jumpPressTime > 10 && jumpPressTime < shortJumpTimeThreshold && !isBouncing)
        {
            rb.velocity = new Vector2 (rb.velocity.x, rb.velocity.y / shortJumpReduction);
        }
        //If player releases jump button while in the middle of a jump, reduce y velocity to cut jump short
        #endregion
        #region Gravity Fall Scale
        if (rb.velocity.y <= 0 && !isBouncing && currentAfterBounceAirTime <= 0)
        {
            rb.gravityScale = gravityScale * fallGravityMultiplier;
        }
        else if (!isBouncing)
        {
            if (currentAfterBounceAirTime > 0)
            {
                rb.gravityScale = gravityScale * afterBounceAirTimeGravityMultiplier;
            }
            else
            {
                rb.gravityScale = gravityScale;
            }
        }
        //if in bouncemode, player is going downwards increase gravity only if bounce direction isnt downwards and the momentum is under a certain threshold
        if (rb.velocity.y < 0 && isBouncing)
        {
            if (bounceDirection.y < 0 && currentBounceMomentum > 1000)
            {
                rb.gravityScale = gravityScale * bounceGravityMultiplier * 0.001f;
            }
            else
            {
                rb.gravityScale = gravityScale * bounceFallGravityMultiplier;
            }
            
        }
        else if (isBouncing)
        {
            rb.gravityScale = gravityScale * bounceGravityMultiplier;
        }
        #endregion
        #region Additional Bounce Force
        if (isBouncing)
        {
            Bounce();
        }
        #endregion
    }
    private void TickTimers()
    {
        lastGroundedTime -= Time.deltaTime;
        lastJumpTime -= Time.deltaTime;
        iFrameTimer -= Time.deltaTime;
        if (currentAfterBounceAirTime > 0)
        {
            currentAfterBounceAirTime -= Time.deltaTime;
        }
        else
        {
            currentAfterBounceAirTime = 0f;
        }
    }
    private void Move()
    {
        if (isBouncing)
        {
            rb.AddForce(Mathf.Round(moveInput.x) * Vector2.right * bounceMoveMultiplier);
        }
        else if (isJumping)
        {
            //Calculates speed the player wants to go based on movement input
            float targetSpeed = moveInput.x * jumpMoveSpeed;
            //Calculate the difference between the player's desired speed and the actual speed
            float speedDif = targetSpeed - rb.velocity.x;
            //Checks if targetspeed is close to zero if so decelrate otherwise accelerate
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? jumpAcceleration : jumpDeceleration;
            float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, jumpVelPower) * Mathf.Sign(speedDif);
            // Apply physics-based movement using Rigidbody
            rb.AddForce(new Vector2(movement, 0f));
        }
        else
        {
            //Calculates speed the player wants to go based on movement input
            float targetSpeed = moveInput.x * moveSpeed;
            //Calculate the difference between the player's desired speed and the actual speed
            float speedDif = targetSpeed - rb.velocity.x;
            //Checks if targetspeed is close to zero if so decelrate otherwise accelerate
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
            // Apply physics-based movement using Rigidbody
            rb.AddForce(new Vector2(movement, 0f));
        }
        if (!isGrounded && !isBouncing)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);
        }
    }
    private void CheckGrounded()
    {
        Vector2 checkBoxPosition = transform.position + new Vector3(0f, -groundCheckBoxSize.y - 0.85f, 0f);
        isGrounded = Physics2D.OverlapBox(checkBoxPosition, groundCheckBoxSize, 0f, groundLayer);
        if (isBouncing)
        {
            isGrounded = false;
        }
        if (isGrounded)
        {

            lastGroundedTime = jumpCoyoteTime;
            if (!isJumping && !jumpIsPressedInput)
            {
                jumpWasReleased = !jumpIsPressedInput;
            }
        }
        if (isGrounded && !isBouncing)
        {
            bouncesRemaining = maxBounces;
        }
        if (rb.velocity.y <= 0)
        {
            isJumping = false;
        }
        
    }
    private void Jump()
    {
        //Zero out y velocity for coyote time jumps to feel normal
        rb.velocity = new Vector2(rb.velocity.x, 0);
        lastJumpTime = jumpBufferTime;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpSpeedMultiplier);
        isJumping = true;
    }
    private void ToggleBounceMode()
    {
        //Toggles Bounce properties such as collider type and gravity multiplier
        bounceModeCollider.enabled = isBouncing;
        regularModeCollider.enabled = !isBouncing;
        //Add slight upwards force to prevent getting stuck to floor when leaving bouncemode
        if (bouncesRemaining == 0  && isBouncing)
        {
            rb.AddForce(Vector2.up, ForceMode2D.Impulse);
            isBouncing = false;
        }
        if (isBouncing && bouncesRemaining > 0)
        {
            spriteRenderer.sprite = bounceMode;
            rb.mass = bounceMass;
            rb.drag = bounceDrag;
        }
        else
        {
            isBouncing = false;
            currentBounceMomentum = 0f;
            extraMomentum = 0f;
            bounceDirection = Vector2.zero;
            spriteRenderer.sprite = normalMode;
            rb.mass = regularMass;
            rb.drag = regularDrag;
        }
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts.Length != 0)
        {
            if (collision.collider.tag == "HardHazard")
            {
                if (!cheatmode)
                {
                    healthCount--;
                }
                Respawn();
            }
            else if (collision.collider.tag == "SoftHazard" )
            {
                if (!isBouncing && iFrameTimer <= 0f)
                {
                    healthCount--;
                    iFrameTimer = iFrames;
                }
            }
            else
            {
                Vector2 norm1 = new Vector2(Mathf.RoundToInt(collision.contacts[0].normal.x), Mathf.RoundToInt(collision.contacts[0].normal.y)).normalized;
                if (isBouncing)
                {
                    float leftoverMomentum = currentBounceMomentum - bounceMomentum/leftoverMomentumLossRate;
                    currentBounceMomentum = bounceMomentum + extraMomentum;
                    if (leftoverMomentum > 0)
                    {
                        currentBounceMomentum += leftoverMomentum;
                    }
                    extraMomentum = 0f;
                    bouncesRemaining--;
                    //Naturalize velocity and gravity scale to make consistent bounces
                    if (Mathf.Abs(norm1.x) > 0)
                    {
                        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / verticleBounceMomentumLoss);
                    }
                    else
                    {
                        rb.velocity = new Vector2( rb.velocity.x / horizontalBounceMomentumLoss, rb.velocity.y);
                    }
                    
                    rb.gravityScale = gravityScale;
                    if (collision.contacts.Length > 1)
                    {
                        Vector2 norm2 = new Vector2(Mathf.RoundToInt(collision.contacts[1].normal.x), Mathf.RoundToInt(collision.contacts[1].normal.y)).normalized;
                        calculateBounceDirection(norm1 + norm2);
                    }
                    else
                    {
                        calculateBounceDirection(norm1);
                    }
                    //Debug.Log("Collisions : " + collision.contacts.Length + "|| Normal : " + collision.contacts[0].normal + "|| Bounce Direction : " + bounceDirection + "|| Bounce Direction Influence : " + bounceDirectionalInfluence);
                }
            }
        }  
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        Vector2 norm1 = new Vector2(Mathf.RoundToInt(collision.contacts[0].normal.x), Mathf.RoundToInt(collision.contacts[0].normal.y)).normalized;
        //Fixes sticking occured when in contact with 2 surfaces consecutively
        if (isBouncing)
        {
            if (collision.contacts.Length > 1)
            {
                Vector2 norm2 = new Vector2(Mathf.RoundToInt(collision.contacts[1].normal.x), Mathf.RoundToInt(collision.contacts[1].normal.y)).normalized;
                calculateBounceDirection(norm1 + norm1);
            }
            else
            {
                calculateBounceDirection(norm1);
            }
            //Added to potentially fix a bug where it seems like if the player bounces at 0 bounces remaining and enters bounce mode at the same time, player does not recieve any momentum
            if (currentBounceMomentum == 0f)
            {
                currentBounceMomentum = bounceMomentum;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Heal")
        {
            healthCount = maxHealth;
        }
        if (collision.tag == "SoftHazard")
        {
            if (!isBouncing && iFrameTimer <= 0f)
            {
                healthCount--;
                iFrameTimer = iFrames;
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Hazard" && iFrameTimer <= 0f)
        {
            healthCount--;
            iFrameTimer = iFrames;
        }
    }
    private void calculateBounceDirection(Vector2 normal)
    {
        //Make consistent normal vectors
        //Checks collision from cardinal directions
        bounceDirection = Vector2.zero;
        if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))// Collision is left or right
        {
            if (normal == new Vector2(-1, 0))
            {
                //collision on right
                if (bounceVerticalInfluence == 0)
                {
                    if (rb.velocity.y < -1f)
                    {
                        bounceVerticalInfluence = -1f;
                    }
                    else
                    {
                        bounceVerticalInfluence = 1f;
                    }
                }
                bounceDirection = new Vector2(-1f, bounceVerticalInfluence * verticalInfluenceMultiplier).normalized;            
                bounceHorizontalInfluence = -1f;
            }
            else if (normal == new Vector2(1, 0))
            {
                //collision on left
                if (bounceVerticalInfluence == 0)
                {
                    if (rb.velocity.y < -1f)
                    {
                        bounceVerticalInfluence = -1f;
                    }
                    else
                    {
                        bounceVerticalInfluence = 1f;
                    }
                }
                bounceDirection = new Vector2(1f, bounceVerticalInfluence * verticalInfluenceMultiplier).normalized;
                bounceHorizontalInfluence = 1f;
            }
        }
        else // Collision is up or down
        {
            if (normal == new Vector2(0, -1))
            {
                //collision on up
                if (bounceHorizontalInfluence == 0f)
                {
                    if (rb.velocity.x > 0.5f)
                    {
                        bounceHorizontalInfluence = 1f;
                    }
                    else if (rb.velocity.x < -0.5f)
                    {
                        bounceHorizontalInfluence = -1f;
                    }
                }
                bounceDirection = new Vector2(bounceHorizontalInfluence * horizontalInfluenceMultiplier * 2f, -1f).normalized;
            }
            else if (normal == new Vector2(0, 1))
            {
                //collision on down
                if (bounceHorizontalInfluence == 0f)
                {
                    if (rb.velocity.x > 0.5f)
                    {
                        bounceHorizontalInfluence = 1f;
                    }
                    else if (rb.velocity.x < -0.5f)
                    {
                        bounceHorizontalInfluence = -0.5f;
                    }
                }
                bounceDirection = new Vector2(bounceHorizontalInfluence * horizontalInfluenceMultiplier, 1f).normalized;
            }
            else
            {
                //Diagonal collisions
                bounceDirection = normal/1.5f;
            }
        }
        bounceDirection *= 1.1f;
    }
    private void Bounce()
    {
        rb.AddForce(bounceDirection * currentBounceMomentum);
        if (currentBounceMomentum - bounceMomentumDecayRate > 0)
        {
            currentBounceMomentum -= bounceMomentumDecayRate;
        }
        else
        {
            currentBounceMomentum = 0f;
            extraMomentum += extraMomentumBuildRate;
        }
    }
    private void Respawn()
    {
        isBouncing = false;
        bouncesRemaining = maxBounces;
        afterBounceAirTime = 0f;
        rb.velocity = Vector2.zero;
        ToggleBounceMode();
        CheckGrounded();
        if (checkpointManager.lastReachedCheckpoint != -1)
        {
            transform.position = checkpointManager.checkpoints[checkpointManager.lastReachedCheckpoint].transform.position;
        }
        else 
        {
            transform.position = startPosition;
        }

    }
}

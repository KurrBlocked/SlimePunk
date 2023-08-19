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


    //Jump Variables
    public float jumpForce = 5.5f;
    public float jumpSpeedMultiplier = 3.2f;
    private bool isJumping = false;
    public float jumpCoyoteTime = 0.15f;
    public float jumpBufferTime = 0.1f;
    public float jumpCutMultiplier = 0.1f;
    private float gravityScale;
    public float fallGravityMultiplier = 3f;
    private float lastGroundedTime = 0f;
    private float lastJumpTime = 0f;
    private float airTimeFactor = 1f;
    public float airTimeThreshold = 0.5f;
    public float airTimeLossRate = 0.1f;

    //Bounce Variables
    public int bouncesRemaining = 0;
    public int maxBounces = 6;
    public bool isBouncing;
    public float bounceDelayTime = 0f;
    public float bouncePauseAmount = 0f;

    public float bounceMomentum = 1550f;
    public float bounceMomentumDecayRate = 75f;
    private float currentBounceMomentum = 0f;

    public float bounceMass = 9f;
    public float regularMass = 1f;
    public float bounceDrag = 2.2f;
    public float regularDrag = 0.5f;
    private Vector2 bounceDirection;
    public float bounceMoveMultiplier = 150f;
    public float bounceGravityMultiplier = 2f;
    public float bounceFallGravityMultiplier = 3f;
    public float bounceVelocityMultiplier = 1.01f;
    private Vector2 bounceDirectionalInfluence;
    public float horizontalInfluenceMultiplier = 0.4f;
    public float verticalInfluenceMultiplier = 2.5f;
    public float extraMomentumBuildRate = 10f;
    public float extraMomentum;
    public float diagonalBounceDirectionDampener = 1.4f;
    public float singularBounceDirectionDampener = 1.4f;
    public float afterBounceAirTime = 0.1f;
    public float currentAfterBounceAirTime = 0f;
    public float afterBounceAirTimeGravityMultiplier = 0.8f;
    public float velocityClampingMomentumThreshold = 2000f;
    public float maxBounceVelocity = 15f;
    public float carryOverMomentumReductionRate = 0.3f;
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
    private CircleCollider2D circleCollider;
    private BoxCollider2D boxCollider;

    //Sprites
    private SpriteRenderer spriteRenderer;
    public Sprite bounceMode;
    public Sprite normalMode;


    //Testing variables
    public InputActionReference cheatmodeAction;
    public float timescale = 1f;
    public bool spawnAtStart = true;
    public bool cheatmode = false;

    // Start is called before the first frame update
    void Awake()
    {
        healthCount = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        currentBounceMomentum = 0f;
        extraMomentum = 0f;
        bounceDirection = Vector2.zero;
        bounceDelayTime = 0f;
        currentAfterBounceAirTime = 0f;
        jumpWasReleased = true;
        spriteRenderer.sprite = normalMode;
        isBouncing = false;
        isJumping = false;
        gravityScale = rb.gravityScale;
        circleCollider.enabled = isBouncing;
        boxCollider.enabled = !isBouncing;
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
        if (bounceInput)
        {
            if (bouncesRemaining >= 0)
            {
                isBouncing = !isBouncing;
                //If player leaves bouncemode prematurely, set slowfall timer
                if (!isBouncing)
                {
                    currentAfterBounceAirTime = afterBounceAirTime;
                }
            }
        }
        if (isBouncing)
        {
            bounceDirectionalInfluence = new Vector2(Mathf.Round(moveInput.x), Mathf.Round(moveInput.y));
        }
        #endregion
        if (cheatmodeAction.action.WasPressedThisFrame())
        {
            cheatmode = !cheatmode;
        }
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
            Jump();
            jumpWasReleased = false;
            
        }
        //If player releases jump button while in the middle of a jump, reduce y velocity to cut jump short
        if (rb.velocity.y > 0 && !jumpIsPressedInput && !isBouncing)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
        }
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
            //Add a small freeze between a bounce
            if (bounceDelayTime > 0)
            {
                rb.velocity = Vector2.zero;
                rb.constraints = (RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation);
            }
            else
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                Bounce();
                
            }
            
        }
        #endregion
    }
    private void TickTimers()
    {
        lastGroundedTime -= Time.deltaTime;
        lastJumpTime -= Time.deltaTime;
        if (bounceDelayTime > 0)
        {
            bounceDelayTime -= Time.deltaTime;
        }
        else
        {
            bounceDelayTime = 0f;
        }
        if (currentAfterBounceAirTime > 0)
        {
            currentAfterBounceAirTime -= Time.deltaTime;
        }
        else
        {
            currentAfterBounceAirTime = 0f;
        }

        //Reduce amount of horizontal movement possible for the player the longer they are in the air for
        if (!isGrounded && !isBouncing && airTimeFactor > airTimeThreshold)
        {
            airTimeFactor -= (Time.deltaTime * airTimeLossRate);
        }
        else if (isGrounded)
        {
            airTimeFactor = 1f;
        }

    }
    private void Move()
    {
        if (isBouncing)
        {
            rb.AddForce(Mathf.Round(moveInput.x) * Vector2.right * bounceMoveMultiplier);
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
            rb.velocity = new Vector2(rb.velocity.x * airTimeFactor, rb.velocity.y);
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
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpSpeedMultiplier);
        isJumping = true;
    }
    private void ToggleBounceMode()
    {
        //Toggles Bounce properties such as collider type and gravity multiplier
        circleCollider.enabled = isBouncing;
        boxCollider.enabled = !isBouncing;
        //Add slight upwards force to prevent getting stuck to floor when leaving bouncemode
        if (bouncesRemaining == 0  && isBouncing)
        {
            rb.AddForce(Vector2.up, ForceMode2D.Impulse);
        }else if (isBouncing && bouncesRemaining > 0)
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
            if (collision.collider.tag == "Hazard")
            {
                if (!cheatmode)
                {
                    healthCount--;
                }
                Respawn();
            }
            else
            {
                Vector2 norm1 = new Vector2(Mathf.RoundToInt(collision.contacts[0].normal.x), Mathf.RoundToInt(collision.contacts[0].normal.y)).normalized;
                if (isBouncing)
                {
                    bounceDelayTime = bouncePauseAmount;
                    currentBounceMomentum = bounceMomentum + extraMomentum + Mathf.Round(currentBounceMomentum * carryOverMomentumReductionRate);
                    Debug.Log(currentBounceMomentum);
                    extraMomentum = 0f;
                    bouncesRemaining--;
                    //Naturalize velocity and gravity scale to make consistent bounces
                    rb.velocity = Vector2.zero;
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
                /*if (!isGrounded && !isBouncing)
                {
                    if (norm1 == new Vector2(0, 1))
                    {
                        rb.velocity = Vector2.zero;
                        Debug.Log("ground");
                    }
                }*/
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
        /*if (!isGrounded && !isBouncing)
        {
            if (norm1 == new Vector2(0, 1))
            {
                rb.velocity = Vector2.zero;
                Debug.Log("ground");
            }
        }*/
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Heal")
        {
            healthCount = maxHealth;
        }
    }
    private void calculateBounceDirection(Vector2 normal)
    {
        //Make consistent normal vectors
        //normal = new Vector2 (Mathf.RoundToInt(collision.contacts[0].normal.x), Mathf.RoundToInt(collision.contacts[0].normal.y)).normalized;
        //Checks collision from cardinal directions
        if (normal == new Vector2(0, -1))
        {
            //collision on up
            bounceDirection = new Vector2(bounceDirectionalInfluence.x * horizontalInfluenceMultiplier * 2f, -1f).normalized;
        }
        else if (normal == new Vector2(0, 1))
        {
            //collision on down
            
            bounceDirection = new Vector2(bounceDirectionalInfluence.x * horizontalInfluenceMultiplier, 1f).normalized;
        }
        else if (normal == new Vector2(-1, 0))
        {
            //collision on right

            bounceDirection = new Vector2(-1f, bounceDirectionalInfluence.y * verticalInfluenceMultiplier).normalized;

        }
        else if (normal == new Vector2(1, 0))
        {
            //collision on left
            bounceDirection = new Vector2(1f, bounceDirectionalInfluence.y * verticalInfluenceMultiplier).normalized;

        }
        else//Checks collision from diagonal directions
        {
            if (normal.x < 0)
            {
                if (normal.y < 0)
                {
                    //collision upper right 
                    bounceDirection = (new Vector2(-0.6f, -1).normalized / diagonalBounceDirectionDampener + new Vector2(bounceDirectionalInfluence.x * horizontalInfluenceMultiplier, 0f)).normalized;
                }
                else
                {
                    //collision downward right
                    bounceDirection = (new Vector2(-0.6f, 1).normalized / diagonalBounceDirectionDampener + new Vector2(bounceDirectionalInfluence.x * horizontalInfluenceMultiplier, 0f)).normalized;
                }
            }
            else
            {
                if (normal.y < 0)
                {
                    //collision upper  left
                    bounceDirection = (new Vector2(0.6f, -1).normalized / diagonalBounceDirectionDampener + new Vector2(bounceDirectionalInfluence.x * horizontalInfluenceMultiplier, 0f)).normalized;
                }
                else
                {
                    //collision downard left
                    bounceDirection = (new Vector2(0.6f, 1).normalized / diagonalBounceDirectionDampener + new Vector2(bounceDirectionalInfluence.x * horizontalInfluenceMultiplier, 0f)).normalized;
                }
            }

        }
        if (bounceDirection == Vector2.left || bounceDirection == Vector2.right)// Singular directional forces left and right are dampened to reduce unnatural bouncing
        {
            bounceDirection /= singularBounceDirectionDampener;
        }
    }
    private void Bounce()
    {
        rb.AddForce(bounceDirection * currentBounceMomentum);
        rb.velocity = new Vector2 (rb.velocity.x * bounceVelocityMultiplier, rb.velocity.y * bounceVelocityMultiplier);
        if (currentBounceMomentum > 0)
        {
            currentBounceMomentum -= bounceMomentumDecayRate;
        }
        else
        {
            currentBounceMomentum = 0f;
            extraMomentum += extraMomentumBuildRate;
        }
        if (currentBounceMomentum < 0)
        {
            currentBounceMomentum = 0f;
        }
        if (currentBounceMomentum > velocityClampingMomentumThreshold)
        {
            rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxBounceVelocity);
        }
    }
    private void Respawn()
    {
        isBouncing = false;
        bouncesRemaining = maxBounces;
        afterBounceAirTime = 0f;
        rb.velocity = Vector2.zero;
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

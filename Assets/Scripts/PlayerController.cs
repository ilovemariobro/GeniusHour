using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour {
    [HideInInspector]
    public Rigidbody2D rb;
    private Animator anim;
    public LayerMask whatIsGround;
    public Transform groundCheck;
    public Transform wallCheck;
    public SpriteRenderer sr;

    [Space]
    [Header("Stats")]
    public float speed = 10;
    public float jumpForce = 50;
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;
    public float dashSpeed = 20;
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float xPos;
    public float directionOffset = 154.0f;
    private float movementInputDirection;

    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool wallJumped;
    public bool wallSlide;
    public bool isDashing;
    private bool isTouchingWall;
    private bool isGrounded;
    private bool isWalking;

    [Space]

    private bool groundTouch;
    private bool hasDashed;
    
    public int side = 1;

    // Start is called before the first frame update
    void Start()
    {   
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        movementInputDirection = Input.GetAxisRaw("Horizontal");
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);

        Walk(dir);
        CheckMovementDirection();
        UpdateAnimations();

        if (isGrounded && !isDashing)
        {
            wallJumped = false;
            GetComponent<Jumping>().enabled = true;
        }
        
        rb.gravityScale = 3;

        if(isTouchingWall && !isGrounded)
        {
            if (x != 0)
            {
                wallSlide = true;
                WallSlide();
            }
        }

        if (!isTouchingWall || isGrounded)
            wallSlide = false;

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
                Jump(Vector2.up, false);
            if (isTouchingWall && !isGrounded)
                WallJump();
        }

        if (Input.GetButtonDown("Dash") && !hasDashed)
        {
            if(xRaw != 0 || yRaw != 0)
                Dash(xRaw, yRaw);
        }

        if (isGrounded && !groundTouch)
        {
            GroundTouch();
            groundTouch = true;
        }

        if(!isGrounded && groundTouch)
        {
            groundTouch = false;
        }

        if (wallSlide || !canMove)
            return;

        if(x > 0)
        {
            side = 1;
        }
        if (x < 0)
        {
            side = -1;
        }
    }

    private void FixedUpdate() {
        CheckSurroundings();
    }

    private void CheckSurroundings() {
       isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
       isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }

    private void CheckMovementDirection() {
        movementInputDirection = Input.GetAxisRaw("Horizontal");
        if (side==1 && movementInputDirection < 0) {
            Flip();
        } else if (side==-1 && movementInputDirection > 0) {
            Flip();
        }

        if (Mathf.Abs(rb.velocity.x) > 0.1f) {
            isWalking = true;
        } else {
            isWalking = false;
        }
    }

    private void UpdateAnimations() {
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isWallSliding", wallSlide);
    }

    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;
    }

    private void Dash(float x, float y)
    {
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);

        hasDashed = true;

        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);

        rb.velocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());
    }

    IEnumerator DashWait()
    {
        FindObjectOfType<GhostTrail>().ShowGhost();
        StartCoroutine(GroundDash());
        DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        rb.gravityScale = 0;
        GetComponent<Jumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        rb.gravityScale = 3;
        GetComponent<Jumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(.15f);
        if (isGrounded)
            hasDashed = false;
    }

    private void WallJump()
    {
        if ((side == 1 && isTouchingWall) || side == -1 && isTouchingWall)
        {
            side *= -1;
            Flip();
        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = isTouchingWall ? Vector2.left : Vector2.right;

        Jump((Vector2.up / 1.5f + wallDir / 1.5f), true);

        wallJumped = true;
    }

    private void WallSlide()
    {

        if (!canMove)
            return;

        bool pushingWall = false;
        if((rb.velocity.x > 0 && isTouchingWall) || (rb.velocity.x < 0 && isTouchingWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : rb.velocity.x;

        rb.velocity = new Vector2(push, -slideSpeed);
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove) {
            isWalking = false;
            return;
        }   
        if (!wallJumped)
        {
            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }

    private void Jump(Vector2 dir, bool wall)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += dir * jumpForce;
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    void RigidbodyDrag(float x)
    {
        rb.drag = x;
    }

    private void Flip() {
        side *= -1;
        transform.Rotate(0.0f, 180.0f, 0.0f);

            // Fixes a weird positioning bug when Player is rotated
            
            if (side==1) {
                xPos = transform.position.x;
                xPos += directionOffset;
                transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
            } else {
                xPos = transform.position.x;
                xPos -= directionOffset;
                transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
            }
        }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}


































/*
    private int facingDirection = 1;
    private int lastWallJumpDirection;
    private bool isFacingRight = true;
    private bool isWalking;
    private bool canNormalJump;
    private bool canWallJump;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isAttemptingToJump;
    private bool checkJumpMultiplier;
    private bool canMove;
    private bool canFlip;
    private bool hasWallJumped;
    public bool hasDashed;
    private float movementInputDirection;
    private float jumpTimer;
    private float turnTimer;
    private float wallJumpTimer;
    public float xPos;
    public float directionOffset = 154.0f;
    public float movementSpeed = 10.0f;
    public float dashSpeed = 20.0f;
    public float jumpForce = 16.0f;
    public float groundCheckRadius;
    public float wallCheckDistance;
    public float wallSlideSpeed;
    public float movementForceInAir;
    public float airDragMultiplier = 0.95f;
    public float variableJumpHeightMultiplier = 0.5f;
    public float wallHopForce;
    public float wallJumpForce;
    public float jumpTimerSet = 0.15f;
    public float turnTimerSet = 0.1f;
    public float wallJumpTimerSet = 0.5f;
    public bool isDashing;
    
    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;
    private Rigidbody2D rb;
    private Animator anim;
    public LayerMask whatIsGround;
    public Transform groundCheck;
    public Transform wallCheck;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }

    // Update is called once per frame
    void Update() {
        float rawX = Input.GetAxisRaw("Horizontal");
        float rawY = Input.GetAxisRaw("Vertical");

        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
        CheckIfWallSliding();
        CheckJump();

        if (isGrounded && !isDashing)
        {
            GetComponent<Jumping>().enabled = true;
        }

        if (Input.GetButtonDown("Fire1") && !hasDashed) {
            if(rawX != 0 || rawY != 0)
                Dash(rawX, rawY);
        }

        if (isGrounded) {
            GroundTouch();
        }
    }
    
    private void FixedUpdate() {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckIfWallSliding() {
        if (isTouchingWall & movementInputDirection == facingDirection && rb.velocity.y < 0) {
            isWallSliding = true;
        } else {
            isWallSliding = false;
        }
    }
    private void CheckSurroundings() {
       isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
       isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }

    private void CheckIfCanJump() {
        if (isTouchingWall) {
            canWallJump = true;
        }
        if (isGrounded && rb.velocity.y <= 0.01f) {
            canNormalJump = true;
        } else {
            canNormalJump = false;
        }
    }
    
    private void CheckMovementDirection() {
        if (isFacingRight && movementInputDirection < 0) {
            Flip();
        } else if (!isFacingRight && movementInputDirection > 0) {
            Flip();
        }

        if (Mathf.Abs(rb.velocity.x) > 0.1f) {
            isWalking = true;
        } else {
            isWalking = false;
        }
    }
    
    private void UpdateAnimations() {
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
    }

    private void CheckInput() {
        movementInputDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump")) {
            if (isGrounded || (canNormalJump && isTouchingWall)) {
                NormalJump(Vector2.up);
            } else {
                jumpTimer = jumpTimerSet;
                isAttemptingToJump = true;
            }
        }
        if (Input.GetButtonDown("Horizontal") && isTouchingWall) {
            if (!isGrounded && movementInputDirection != facingDirection) {
                canMove = false;
                canFlip = false;

                turnTimer = turnTimerSet;
            }
        }

        if (!canMove) {
            turnTimer -= Time.deltaTime;

            if (turnTimer <= 0) {
                canMove = true;
                canFlip = true;
            }
        }

        if (checkJumpMultiplier && !Input.GetButton("Jump")) {
            checkJumpMultiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMultiplier);
        }
    }

    private void CheckJump() {
        if (jumpTimer > 0) {
            // Wall Jump 
            if (!isGrounded && isTouchingWall && movementInputDirection != 0 && movementInputDirection != facingDirection) {
                WallJump();
            } else if (isGrounded) {
                NormalJump(Vector2.up);
            } 
        }

        if (isAttemptingToJump) {
            jumpTimer -= Time.deltaTime;
        }

        if (wallJumpTimer > 0) {
            if (hasWallJumped && movementInputDirection == -lastWallJumpDirection) {
                rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                hasWallJumped = false;
            } else if (wallJumpTimer <= 0) {
                hasWallJumped = false;
            } else {
                wallJumpTimer -= Time.deltaTime;
            }
        }
    }

    private void NormalJump(Vector2 dir) {
        if (canNormalJump && !isWallSliding) {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.velocity += dir * jumpForce;
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
        }
    }

    private void WallJump() {
        if (canWallJump) { // Wall Jump
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            isWallSliding = false;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;
        }
    }

    private void ApplyMovement() {
        if (!isGrounded && !isWallSliding && movementInputDirection == 0) {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        } else if (canMove) {
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
        }
        if (isWallSliding) {
            if (rb.velocity.y < -wallSlideSpeed) {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            }
        }
    }

    void GroundTouch() {
        hasDashed = false;
        isDashing = false;
    }

    private void Dash(float x, float y) {
        //Camera.main.transform.DOComplete();
        //Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);

        hasDashed = true;

        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);

        rb.velocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());
    }

    IEnumerator DashWait() {
        StartCoroutine(GroundDash());
        //DOVirtual.Float(14, 0, .8f, RigidbodyDrag);

        rb.gravityScale = 0;
        GetComponent<Jumping>().enabled = false;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        rb.gravityScale = 4;
        GetComponent<Jumping>().enabled = true;
        isDashing = false;
    }

    IEnumerator GroundDash() {
        yield return new WaitForSeconds(.15f);
        if (isGrounded)
            hasDashed = false;
    }

    void RigidbodyDrag(float x) {
        rb.drag = x;
    }

    private void Flip() {
        if(!isWallSliding && canFlip) {
            facingDirection *= -1;
            isFacingRight = !isFacingRight;
            transform.Rotate(0.0f, 180.0f, 0.0f);

            // Fixes a weird positioning bug when Player is rotated
            if (isFacingRight) {
                xPos = transform.position.x;
                xPos += directionOffset;
                transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
            } else {
                xPos = transform.position.x;
                xPos -= directionOffset;
                transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}
*/
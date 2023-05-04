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
    public Vector2 wallJumpDirection;

    [Space]
    [Header("Stats")]
    public float speed = 10;
    public float jumpForce = 50;
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;
    public float dashSpeed = 20;
    public float wallSlideSpeed = 2;
    public float groundCheckRadius = 0.3f;
    public float wallCheckDistance = 0.4f;
    public float xPos;
    private float directionOffset = 153.0f;
    public float wallJumpForce = 20;
    private float movementInputDirection;
    private int lastWallJumpDirection;

    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool wallJumped;
    public bool isWallSliding;
    public bool isDashing;
    private bool isTouchingWall;
    private bool isGrounded;
    private bool isWalking;
    private bool hasWallJumped;
    private bool canWallJump;

    [Space]
    public bool xInputIsRight = true;
    
    private bool groundTouch;
    private bool hasDashed;

    // Start is called before the first frame update
    void Start()
    {   
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        movementInputDirection = Input.GetAxisRaw("Horizontal");
        wallJumpDirection.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        float xInput = Input.GetAxis("Horizontal");
        float yInput = Input.GetAxis("Vertical");
        float xRawInput = Input.GetAxisRaw("Horizontal");
        float yRawInput = Input.GetAxisRaw("Vertical");
        Vector2 inputDir = new Vector2(xInput, yInput);

        Walk(inputDir);
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();

        if (isGrounded && !isDashing)
        {
            wallJumped = false;
            GetComponent<Jumping>().enabled = true;
        }
        
        if(isTouchingWall && !isGrounded)
        {
            // if (xInput != 0)
            // {
                isWallSliding = true;
                WallSlide();
            // }
        }

        if (!isTouchingWall || isGrounded)
            isWallSliding = false;

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
                Jump(Vector2.up);
            if (isTouchingWall && !isGrounded)
                WallJump();
        }

        if (Input.GetButtonDown("Dash") && !hasDashed)
            Dash(xRawInput, yRawInput);

        if (isGrounded && !groundTouch)
        {
            GroundTouch();
            groundTouch = true;
        }

        if(!isGrounded && groundTouch)
            groundTouch = false;

        if (isWallSliding || !canMove)
            return;

        if(xInput != 0) xInputIsRight = xInput > 0;
    }

    private void FixedUpdate()
    {
        CollisionDetection();
    }
    
    private void CollisionDetection()
    {
       isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
       isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }

    private void CheckIfCanJump() {
        if (isTouchingWall)
            canWallJump = true;
    }
    private void CheckIfWallSliding() {
        isWallSliding = isTouchingWall && movementInputDirection == (xInputIsRight ? 1 : -1) && rb.velocity.y < 0;
    }

    private void CheckMovementDirection()
    {
        movementInputDirection = Input.GetAxisRaw("Horizontal");
        if (xInputIsRight && movementInputDirection < 0) {
            Flip();
        } else if (!xInputIsRight && movementInputDirection > 0) {
            Flip();
        }

        isWalking = Mathf.Abs(rb.velocity.x) > 0.1f;
    }

    private void UpdateAnimations()
    {
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isWallSliding", isWallSliding);
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
        
        if (x == 0 && y == 0)
            x = xInputIsRight ? 1 : -1;

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

        rb.gravityScale = 4;
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
        if (canWallJump) {
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            isWallSliding = false;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);
            //jumpTimer = 0;
            //isAttemptingToJump = false;
            //checkJumpMultiplier = true;
            //turnTimer = 0;
            canMove = true;
            //canFlip = true;
            hasWallJumped = true;
            //wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = xInputIsRight ? -1 : 1;
        }
    }
    
    private void WallSlide() {
        if (rb.velocity.y < -wallSlideSpeed) {
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
        }
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

    private void Jump(Vector2 dir)
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

    private void Flip(bool useOffset = true) 
    {
        xInputIsRight = !xInputIsRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);

        // Fixes a weird positioning bug when Player is rotated
        xPos = transform.position.x;
            
        if (xInputIsRight && useOffset) {
            xPos += directionOffset;
        } else if (useOffset) {
            xPos -= directionOffset;
        }
        
        transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
    }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}

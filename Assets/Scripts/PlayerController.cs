using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    private float movementInputDirection;
    private bool isFacingRight = true;
    private bool isWalking;
    private bool canJump;
    private bool isGrounded;
    private bool isTouchingWall;
    public float xPos;
    public float directionOffset = 154.0f;
    public float movementSpeed = 10.0f;
    public float jumpForce = 16.0f;
    public float groundCheckRadius;
    public float wallCheckDistance = 2.0f;

    private Rigidbody2D rb;
    private Animator anim;
    public LayerMask whatIsGround;
    public Transform groundCheck;
    public Transform wallCheck;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update() {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
        CheckIfCanJump();
    }
    
    private void FixedUpdate() {
        ApplyMovement();
        CheckSurroundings();
    }

    public void CheckSurroundings() {
       isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
       isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);
    }

    private void CheckIfCanJump() {
        if(isGrounded) {
            canJump = true;
        } else {
            canJump = false;
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
    }

    private void CheckInput() {
        movementInputDirection = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump")) {
            Jump();
        }
    }

    private void Jump() {
        if(canJump) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    private void ApplyMovement() {
        rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
    }

    private void Flip() {
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

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}

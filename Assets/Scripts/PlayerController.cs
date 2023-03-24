using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] private readonly float directionOffset = 154.0f;
    [SerializeField] private readonly float movementSpeed = 10.0f;
    [SerializeField] private readonly float jumpForce = 16.0f;
    [SerializeField] private readonly float groundCheckRadius;
    [SerializeField] private readonly float wallCheckDistance = 2.0f;
    
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    
    private float movementInputDirection;
    private bool isFacingRight = true;
    private bool isWalking;
    private bool isGrounded;
    private bool isTouchingWall;

    private Rigidbody2D rb;
    private Animator anim;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update() {
        CheckInput();
        CheckMovementDirection();
        UpdateAnimations();
    }
    
    private void FixedUpdate() {
        ApplyMovement();
        CheckSurroundings();
    }

    private void CheckSurroundings() {
       isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
       isTouchingWall = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, groundLayer);
    }
    
    private void CheckMovementDirection() {
        if ((isFacingRight && movementInputDirection < 0) || (!isFacingRight && movementInputDirection > 0))
            Flip();

        isWalking = Mathf.Abs(rb.velocity.x) > 0.1f;
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
        if(isGrounded) {
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
        float xPos = transform.position.x;
        xPos += isFacingRight ? directionOffset : -directionOffset;
        transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}

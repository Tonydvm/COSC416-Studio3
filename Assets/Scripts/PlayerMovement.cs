using UnityEngine;
using TMPro;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 500f;

    [Header("Jump")]
    public float jumpForce = 7f;
    public float gravityMultiplier = 2f;
    public int maxJumps = 2;
    private int jumpsRemaining;

    [Header("Dash")]
    public float dashForce = 15f;
    public float dashDuration = 0.2f;
    private float dashTimer = 0f;
    private bool isDashing = false;


    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private float gravity = -9.81f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    [Header("UI")]
    public TextMeshProUGUI scoreTextUI;
    private int score = 0;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("PlayerMovement script requires a CharacterController component on the same GameObject.");
            enabled = false;
            return;
        }

        if (scoreTextUI == null)
        {
            Debug.LogWarning("Score Text UI is not assigned in PlayerMovement script. Coin collection score will not be displayed.");
        }
        else
        {
            UpdateScoreUI();
        }

        jumpsRemaining = maxJumps;
    }

    void Update()
    {
        GroundCheck();

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 moveInput = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        Vector3 moveDirection = Vector3.zero;
        if (moveInput.magnitude >= 0.1f)
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 cameraRight = Camera.main.transform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            moveDirection = (cameraForward * moveInput.z + cameraRight * moveInput.x).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (Input.GetButtonDown("Fire3") && !isDashing)
        {
            StartDash();
        }

        if (isDashing)
        {
            dashTimer += Time.deltaTime;
            if (dashTimer >= dashDuration)
            {
                isDashing = false;
                dashTimer = 0f;
            }
        }


        Vector3 horizontalVelocity = moveDirection * moveSpeed;
        if (isDashing)
        {
            horizontalVelocity = moveDirection * dashForce;
        }
        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;


        if (jumpsRemaining > 0 && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity * gravityMultiplier);
            jumpsRemaining--;
        }

        velocity.y += gravity * gravityMultiplier * Time.deltaTime;

        if (!isGrounded)
        {
            if (velocity.y < 0 && isGrounded)
                velocity.y = gravity * Time.deltaTime * 2f;
        }


        characterController.Move(velocity * Time.deltaTime);
    }


    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded)
        {
            jumpsRemaining = maxJumps;
        }
    }


    public void CollectCoin()
    {
        score++;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreTextUI != null)
        {
            scoreTextUI.text = "Score: " + score;
        }
    }


    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
    }

    void StartDash()
    {
        isDashing = true;
        velocity.y = 0f;
    }
}
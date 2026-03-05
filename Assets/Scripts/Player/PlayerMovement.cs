using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public enum State
    {
        WALKING,
        RUNNING,
        JUMPING,
        CROUCHING
    }

    public TMP_Text speedMonitor;
    public Image staminaBar;
    public State currentState = State.RUNNING;

    [Header("Stamina")]
    public float maxStamina;
    public float currentStamina;
    public float staminaRecoveryRate;
    public float staminaRecoveryTime;
    public float staminaSprintDrain;
    public float jumpStaminaDrain;
    public float diveStaminaDrain;
    private float currentStaminaRecovery;

    [Header("Sprint")]
    public float sprintSpeed;
    public float walkSpeed;
    public float acceleration;
    public float deceleration;
    public float airAccelMultiplier = 0.25f;
    private bool sprinting = false;

    [Header("Jump")]
    public float jumpForce;
    public float jumpForwardForce;
    public float airMultiplier;
    public float gravity;
    private float jumpCooldown = 0;
    private bool jumping = false;

    [Header("Crouch")]
    public float crouchSpeed;
    public float crouchHeight;
    public float crouchOffset;
    public float crouchCameraHeight;
    private bool crouching = false;
    private bool headCovered = false;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    public Transform orientation;
    public Transform camPos;

    private Vector2 moveInput;

    private Vector3 moveDirection;
    private Rigidbody rb;
    private CapsuleCollider hitbox;

    public bool GetGrounded() { return grounded; }
    public bool getRunning() { return moveInput != Vector2.zero; }
    public bool getJumping() { return !grounded && currentState == State.RUNNING; }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        hitbox = GetComponent<CapsuleCollider>();
        rb.freezeRotation = true;

        speedMonitor = GUIManager.Instance.elements[0].GetComponent<TMP_Text>();
        staminaBar = GUIManager.Instance.elements[1].GetComponent<Image>();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnCrouch(InputValue value)
    {
        crouching = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        jumping = value.isPressed;
    }

    public void OnSprint(InputValue value)
    {
        sprinting = value.isPressed;
    }

    private void Update()
    {
        Checks();
        DecideState();
        Move();

        speedMonitor.text = "Speed: " + GetSpeed().magnitude.ToString("0.00");
        staminaBar.fillAmount = (currentStamina - 7) / (maxStamina - 7);
    }

    void Checks()
    {
        grounded = Physics.CheckSphere(transform.position + new Vector3(0, -1.0f, 0), 0.34f, whatIsGround);
        headCovered = Physics.CheckSphere(transform.position + new Vector3(0, 0.4f, 0), 0.3f, whatIsGround);
    }

    void DecideState()
    {
        if (currentState != State.RUNNING)
        {
            if (currentStamina < maxStamina)
            {
                currentStaminaRecovery -= Time.deltaTime;
                if (currentStaminaRecovery <= 0)
                {
                    currentStamina += staminaRecoveryRate * Time.deltaTime;
                }
            }
            else
            {
                currentStamina = maxStamina;
            }
        }

        CrouchVisualHandling();

        if (!grounded)
        {
            rb.linearVelocity += new Vector3(0, -gravity * Time.deltaTime, 0);
        }
        jumpCooldown -= Time.deltaTime;

        if (jumping && grounded && !headCovered && jumpCooldown <= 0 && currentStamina > 0)
        {
            currentState = State.JUMPING;
        }
        else if (crouching && grounded || headCovered)
        {
            currentState = State.CROUCHING;
        }
        else if (sprinting && currentStamina > 0 && moveInput != Vector2.zero)
        {
            currentState = State.RUNNING;
        }
        else
        {
            currentState = State.WALKING;
        }
    }

    void Move()
    {
        switch (currentState)
        {
            case State.WALKING:
                PlayerCam.targetFOV = 84;
                Run(walkSpeed);
                break;

            case State.RUNNING:
                PlayerCam.targetFOV = 97;
                LoseStamina(staminaSprintDrain * Time.deltaTime);

                Run(sprintSpeed);
                break;

            case State.JUMPING:
                PlayerCam.targetFOV = 84;
                LoseStamina(jumpStaminaDrain);

                jumpCooldown = 0.2f;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
                rb.linearVelocity += GetSpeed() * jumpForwardForce;
                break;

            case State.CROUCHING:
                PlayerCam.targetFOV = 72;

                Run(crouchSpeed);
                break;

        }
    }

    void Run(float speed)
    {
        moveInput = moveInput.normalized;
        moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;
        moveDirection = moveDirection.normalized;
        moveDirection *= speed;

        Vector3 temp = GetSpeed();
        // on ground
        if (moveDirection != Vector3.zero && speed > 0)
        {
            temp = Vector3.Lerp(temp, moveDirection * (!grounded ? airMultiplier : 1f), Time.deltaTime * acceleration * AirFactor());
        }
        else
        {
            temp = Vector3.Lerp(temp, Vector3.zero, Time.deltaTime * deceleration * AirFactor());
        }
        rb.linearVelocity = new Vector3(temp.x, rb.linearVelocity.y, temp.z);
    }

    private void CrouchVisualHandling()
    {
        float height = playerHeight;
        Vector3 center = new Vector3(0, 0, 0);
        Vector3 cam = new Vector3(0, 0.75f, 0);

        if ((crouching || headCovered) && grounded)
        {
            height = crouchHeight;
            center = new Vector3(0, crouchOffset, 0);
            cam = new Vector3(0, crouchCameraHeight, 0);
        }

        hitbox.height = Mathf.Lerp(hitbox.height, height, Time.deltaTime * 10f);
        hitbox.center = Vector3.Lerp(hitbox.center, center, Time.deltaTime * 10f);
        camPos.localPosition = Vector3.Lerp(camPos.localPosition, cam, Time.deltaTime * 10f);
    }

    void LoseStamina(float amount)
    {
        currentStamina -= amount;
        currentStaminaRecovery = staminaRecoveryTime;
    }

    public Vector3 GetSpeed()
    {
        return new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
    }

    float AirFactor()
    {
        return grounded ? 1f : airAccelMultiplier;
    }
}
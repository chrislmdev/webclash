using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Rigidbody-driven player with gravity, air momentum, and death checks.
/// Attach to the player root with a Rigidbody and Collider.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Death")]
    [SerializeField] private float criticalYThreshold = 1f;

    [Header("Air Control")]
    [SerializeField] private float airControlForce = 8f;
    [SerializeField] private float maxHorizontalSpeed = 12f;

    [Header("Boost")]
    [SerializeField] private float speedBoostMultiplier = 1.6f;
    [SerializeField] private float speedBoostDuration = 3f;

    private Rigidbody rb;
    private bool isDead;
    private bool hasDoubleJump;
    private bool doubleJumpUsed;
    private float speedBoostEndTime = -1f;
    private Vector2 moveInput;

    public bool IsAlive => !isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetScoreReference(transform);
        }
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        ReadMoveInput();

        if (transform.position.y < criticalYThreshold)
        {
            Die();
        }
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }

        ApplyAirControl();
    }

    private void ReadMoveInput()
    {
        moveInput = Vector2.zero;

        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.isPressed)
            {
                moveInput = touch.delta.ReadValue() * 0.01f;
            }
        }
        else if (Mouse.current != null)
        {
            moveInput = Mouse.current.delta.ReadValue() * 0.01f;
        }
    }

    private void ApplyAirControl()
    {
        if (rb.velocity.y > -0.5f && rb.velocity.y < 0.5f)
        {
            return;
        }

        Vector3 force = new Vector3(moveInput.x, 0f, moveInput.y) * airControlForce;
        rb.AddForce(force, ForceMode.Acceleration);

        Vector3 horizontal = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (horizontal.magnitude > maxHorizontalSpeed)
        {
            horizontal = horizontal.normalized * maxHorizontalSpeed;
            rb.velocity = new Vector3(horizontal.x, rb.velocity.y, horizontal.z);
        }
    }

    public void ApplySpeedBoost()
    {
        speedBoostEndTime = Time.time + speedBoostDuration;
    }

    public void GrantDoubleJump()
    {
        hasDoubleJump = true;
        doubleJumpUsed = false;
    }

    public void TryDoubleJump()
    {
        if (!hasDoubleJump || doubleJumpUsed || isDead)
        {
            return;
        }

        doubleJumpUsed = true;
        Vector3 velocity = rb.velocity;
        velocity.y = Mathf.Max(velocity.y, 8f);
        rb.velocity = velocity;
    }

    public void OnLavaContact()
    {
        Die();
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }

    private void LateUpdate()
    {
        if (speedBoostEndTime > 0f && Time.time <= speedBoostEndTime && !isDead)
        {
            Vector3 velocity = rb.velocity;
            float targetForward = maxHorizontalSpeed * speedBoostMultiplier;
            if (velocity.z < targetForward)
            {
                velocity.z = Mathf.MoveTowards(velocity.z, targetForward, Time.deltaTime * 20f);
                rb.velocity = velocity;
            }
        }
    }
}

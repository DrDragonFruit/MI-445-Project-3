
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Movement Properties")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float deceleration = 7f;

    public PlayerInputActions playerControls;
    private InputAction move;
    private InputAction grapple;
    private InputAction kick;

    [Header("References")]
    [SerializeField] private GrapplingGun grapplingGun;

    [Header("Wall Kick")]
    [SerializeField] private float coyotteTime = 0.2f;

    private float horizontalInput;
    private float currentSpeed;
    private float kickBufferCounter;
    private bool isTouchingWall;
    public bool kickRequested;
    private Vector2 wallNormal;

    public float CurrentSpeed => currentSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (grapplingGun == null)
            grapplingGun = GetComponentInChildren<GrapplingGun>();

        playerControls = new PlayerInputActions();
        move = playerControls.Player.Move;
        grapple = playerControls.Player.Grapple;
        kick = playerControls.Player.Kick;
    }

    private void OnEnable()
    {
        move.Enable();
        grapple.Enable();
        kick.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
        grapple.Disable();
        kick.Disable();
    }

    private void Update()
    {
        horizontalInput = move.ReadValue<float>();

        if (kick.WasPressedThisFrame())
            kickBufferCounter = coyotteTime;
        else
            kickBufferCounter = Mathf.Max(0f, kickBufferCounter - Time.deltaTime);

        if (grapplingGun == null)
            return;

        if (grapple.WasPressedThisFrame())
            grapplingGun.SetGrapplePoint();

        if (grapple.WasReleasedThisFrame())
            grapplingGun.StopGrapple();
    }

    private void FixedUpdate()
    {
        Vector2 moveDirection = transform.right;

        if (kickBufferCounter > 0f && isTouchingWall)
        {
            WallKick(wallNormal);
            kickBufferCounter = 0f;
        }

        float acceleration = horizontalInput != 0 ? movementSpeed : deceleration;
        float targetSpeed = horizontalInput * maxSpeed;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);

        if (grapplingGun != null && grapplingGun.IsOrbiting)
            return;

        rb.linearVelocity = moveDirection * currentSpeed;
    }

    public bool IsGrappleHeld()
    {
        return grapple.IsPressed();
    }

    public void StopMovement()
    {
        currentSpeed = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (grapplingGun != null && grapplingGun.IsOrbiting)
            return;

        isTouchingWall = true;
        wallNormal = collision.GetContact(0).normal;

        if (TryWallKick())
            WallKick(wallNormal);
        else
            StopMovement();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (grapplingGun != null && grapplingGun.IsOrbiting)
            return;

        isTouchingWall = true;
        wallNormal = collision.GetContact(0).normal;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isTouchingWall = false;
    }

    private void WallKick(Vector2 wallNormal)
    {
        Vector2 facingDirection = transform.right;
        Vector2 newDirection = Vector2.Reflect(facingDirection, wallNormal);

        float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;

        rb.rotation = angle;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    public bool TryWallKick()
    {
        if (kickBufferCounter <= 0f)
            return false;

        kickBufferCounter = 0f;
        return true;
    }
}

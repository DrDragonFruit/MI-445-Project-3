
using Unity.Mathematics;
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
    [SerializeField] private GameObject paintSplatter;
    [Tooltip("How long the player is stunned when taking damage/being hit by an enemy")]
    [SerializeField] private float stunTimeOnHit = 0.25f;

    public PlayerInputActions playerControls;
    private InputAction move;
    private InputAction grapple;
    private InputAction kick;
    private InputAction hone;

    [Header("References")]
    [SerializeField] private GrapplingGun grapplingGun;
    public Animator animatior;

    [Header("Wall Kick")]
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float kickRange = 3f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Camera mainCamera;
    public float kickBufferCounter;

    private float horizontalInput;
    private float currentSpeed;

    [SerializeField] private float turnSpeed = 5f;
    private float targetAngle;
    private bool isTurning;

    public float CurrentSpeed => currentSpeed;

    private float curElapsedTimeStunned = 0.0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (grapplingGun == null)
            grapplingGun = GetComponentInChildren<GrapplingGun>();

        playerControls = new PlayerInputActions();
        move = playerControls.Player.Move;
        grapple = playerControls.Player.Grapple;
        kick = playerControls.Player.Kick;
        hone = playerControls.Player.Hone;

        curElapsedTimeStunned = stunTimeOnHit;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        move.Enable();
        grapple.Enable();
        kick.Enable();
        hone.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
        grapple.Disable();
        kick.Disable();
        hone.Disable();
    }

    private void Update()
    {
        if (curElapsedTimeStunned >= stunTimeOnHit)
        {
            horizontalInput = move.ReadValue<float>();

            if (kick.WasPressedThisFrame())
                TryWallKick();

            if (grapplingGun == null)
                return;

            if (grapple.WasPressedThisFrame())
                grapplingGun.SetGrapplePoint();

            if (grapple.WasReleasedThisFrame())
                grapplingGun.StopGrapple();

            if (hone.IsPressed())
                SetHoneTarget();

            if (hone.WasReleasedThisFrame())
                isTurning = false;
        }
        else
        {
            curElapsedTimeStunned += Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        kickBufferCounter = Mathf.Max(0f, kickBufferCounter - Time.fixedDeltaTime);

        if (curElapsedTimeStunned >= stunTimeOnHit)
        {
            float acceleration = horizontalInput != 0 ? movementSpeed : deceleration;
            float targetSpeed = horizontalInput * maxSpeed;

            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);

            animatior.SetFloat("Speed", currentSpeed);

            if (grapplingGun != null && grapplingGun.IsOrbiting)
            {
                isTurning = false;
                return;
            }

            if (isTurning)
            {
                float nextAngle = Mathf.MoveTowardsAngle(
                    rb.rotation, targetAngle, turnSpeed * Time.fixedDeltaTime);

                rb.MoveRotation(nextAngle);

                if (Mathf.Abs(Mathf.DeltaAngle(rb.rotation, targetAngle)) < 0.1f)
                    isTurning = false;
            }

            rb.linearVelocity = (Vector2)transform.right * currentSpeed;
        }
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

    public void Boost(float boostAmount)
    {
        currentSpeed *= boostAmount;
        rb.linearVelocity *= boostAmount;
        rb.angularVelocity *= boostAmount;
    }

    private void WallKick(Vector2 wallNormal)
    {
        Vector2 facingDirection = transform.right;
        Vector2 newDirection = Vector2.Reflect(facingDirection, wallNormal);

        float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;

        rb.rotation = angle;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        kickBufferCounter = coyoteTime;
    }

    public void HitByEnemy()
    {
        curElapsedTimeStunned = 0.0f;
        StopMovement();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleWallCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleWallCollision(collision);
    }

    private void HandleWallCollision(Collision2D collision)
    {
        if ((wallLayer.value & (1 << collision.gameObject.layer)) == 0)
            return;

        if (kickBufferCounter > 0f)
            return;

        //StopMovement();
    }

    private void TryWallKick()
    {
        Vector2 forward = transform.right;

        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        float radius = circle.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y)) * 0.8f;

        RaycastHit2D hit = Physics2D.CircleCast(circle.bounds.center, radius, forward, kickRange, wallLayer);

        if (hit.collider == null)
            return;

        animatior.SetTrigger("Kicked");

        if (grapplingGun != null && grapplingGun.IsOrbiting)
            grapplingGun.ReverseOrbitDirection();
        else
            WallKick(hit.normal);
    }

    private void SetHoneTarget()
    {
        if (Mouse.current == null || mainCamera == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane gamePlane = new Plane(Vector3.forward, Vector3.zero);

        if (!gamePlane.Raycast(ray, out float distance))
            return;

        Vector2 direction = (Vector2)ray.GetPoint(distance) - rb.position;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        isTurning = true;
    }
}

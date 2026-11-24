
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
    public InputAction grapple;
    private InputAction kick;

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
    public float currentSpeed;

    public float CurrentSpeed => currentSpeed;

    private float curElapsedTimeStunned = 0.0f;

    public bool isGrappling;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (grapplingGun == null)
            grapplingGun = GetComponentInChildren<GrapplingGun>();

        playerControls = new PlayerInputActions();
        move = playerControls.Player.Move;
        grapple = playerControls.Player.Grapple;
        kick = playerControls.Player.Kick;

        curElapsedTimeStunned = stunTimeOnHit;

        if (mainCamera == null)
            mainCamera = Camera.main;
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
        }
        else
        {
            curElapsedTimeStunned += Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        kickBufferCounter = Mathf.Max(0f, kickBufferCounter - Time.deltaTime);

        if (curElapsedTimeStunned >= stunTimeOnHit)
        {
            Vector2 moveDirection = transform.right;

            float acceleration = horizontalInput != 0 ? movementSpeed : deceleration;
            float targetSpeed = horizontalInput * maxSpeed;

            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);

            animatior.SetFloat("Speed", currentSpeed);

            if (grapplingGun != null && grapplingGun.IsOrbiting)
                return;

            rb.linearVelocity = moveDirection * currentSpeed;
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
        if (kickBufferCounter > 0f)
            return;

        StopMovement();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (kickBufferCounter > 0f)
            return;

        StopMovement();
    }

    private void TryWallKick()
    {
        if (Mouse.current == null || mainCamera == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        Plane gamePlane = new Plane(Vector3.forward, Vector3.zero);

        if (!gamePlane.Raycast(ray, out float distance))
            return;

        Vector2 mouseWorldPosition = ray.GetPoint(distance);
        Vector2 direction = mouseWorldPosition - rb.position;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        Vector2 forward = transform.right;
        Vector2 aimDirection = direction.normalized;

        if (Vector2.Dot(forward, aimDirection) <= 0f)
            return;

        animatior.SetTrigger("Kicked");

        RaycastHit2D hit = Physics2D.Raycast(rb.position, direction.normalized, kickRange, wallLayer);

        if (hit.collider == null)
            return;

        if (grapplingGun != null && grapplingGun.IsOrbiting)
            grapplingGun.ReverseOrbitDirection();
        else
            WallKick(hit.normal);
    }
}

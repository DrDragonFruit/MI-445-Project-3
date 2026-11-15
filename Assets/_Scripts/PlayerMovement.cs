using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Movement Properties")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float deceleration = 7f;
//temp painting stuff
    [SerializeField] private float radius, direction, maxDistance;
    [SerializeField] private Vector3 origin, directionPaint;
    [SerializeField] private LayerMask targetlayer;

    public PlayerInputActions playerControls;
    private InputAction move;
    private InputAction grapple;

    [Header("References")]
    [SerializeField] private GrapplingGun grapplingGun;

    private float horizontalInput;
    private float currentSpeed;

    public float CurrentSpeed => currentSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (grapplingGun == null)
            grapplingGun = GetComponentInChildren<GrapplingGun>();

        playerControls = new PlayerInputActions();
        move = playerControls.Player.Move;
        grapple = playerControls.Player.Grapple;
    }

    private void OnEnable()
    {
        move.Enable();
        grapple.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
        grapple.Disable();
    }

    private void Update()
    {
        horizontalInput = move.ReadValue<float>();

        if (grapplingGun == null)
            return;

        if (grapple.WasPressedThisFrame())
            grapplingGun.SetGrapplePoint();

        if (grapple.WasReleasedThisFrame())
            grapplingGun.StopGrapple();
    }

    private void FixedUpdate()
    {
        float acceleration = horizontalInput != 0 ? movementSpeed : deceleration;
        float targetSpeed = horizontalInput * maxSpeed;

        origin = transform.position;
        directionPaint = transform.right;

        if (Physics.SphereCast(origin, radius, directionPaint, out RaycastHit hit, maxDistance, targetlayer))
        {
            if (hit.collider != null)
            {
                Debug.Log("Hit a collider");
                if (hit.collider.TryGetComponent<paintable>(out paintable paint))
                {
                    Debug.Log("trying to paint");
                    paint.ApplyPaint(hit.transform.position, 1f, 1f, Color.blue);
                }
            }
        }
        

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);

        if (grapplingGun != null && grapplingGun.IsOrbiting)
            return;

        Vector2 moveDirection = transform.right;
        rb.linearVelocity = moveDirection * currentSpeed;
    }

    public bool IsGrappleHeld()
    {
        return grapple.IsPressed();
    }

    public void StopMovement()
    {
        currentSpeed = 0f;
        rb.linearVelocity = Vector3.zero;
        //rb.angularVelocity = 0f;
    }
}


using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Movement Properties")]
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float deceleration = 7f;

    [Header("References")]
    [SerializeField] private GrapplingGun grapplingGun;

    private float horizontalInput;
    private float currentSpeed;

    public float CurrentSpeed => currentSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (grapplingGun == null) grapplingGun = GetComponentInChildren<GrapplingGun>();
    }

    private void Update()
    {
        horizontalInput = Input.GetKey(KeyCode.W) ? 1f : 0f;
    }

    private void FixedUpdate()
    {
        float acceleration = horizontalInput != 0 ? movementSpeed : deceleration;
        float targetSpeed = horizontalInput * maxSpeed;

        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);

        if (grapplingGun != null && grapplingGun.IsOrbiting) 
            return;

        Vector2 moveDirection = transform.right;
        rb.linearVelocity = moveDirection * currentSpeed;
    }

    public void StopMovement()
    {
        currentSpeed = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}

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

    public float MaxSpeed => maxSpeed;
    public float Acceleration => movementSpeed;
    public float Deceleration => deceleration;

    private float horizontalInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        horizontalInput = Input.GetKey(KeyCode.W) ? 1f : 0f;
    }

    private void FixedUpdate()
    {
        if (grapplingGun != null && grapplingGun.IsOrbiting) 
            return;

        float acceleration = horizontalInput != 0 ? movementSpeed : deceleration;
        float targetSpeed = horizontalInput * maxSpeed;

        Vector2 moveDirection = transform.right;
        float currentSpeed = Vector2.Dot(rb.linearVelocity, moveDirection);
        float newSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = moveDirection * newSpeed;
    }
}
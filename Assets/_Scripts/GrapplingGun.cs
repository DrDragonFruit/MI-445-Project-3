
using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingGun : MonoBehaviour
{
    [Header("Scripts Ref:")]
    public GrapplingRope grappleRope;
    public PlayerMovement playerMovement;

    [Header("Layers Settings:")]
    [SerializeField] private bool grappleToAll = false;
    [SerializeField] private int grappableLayerNumber = 9;

    [Header("Main Camera:")]
    public Camera m_camera;

    [Header("Transform Ref:")]
    public Transform gunHolder;
    public Transform gunPivot;
    public Transform firePoint;

    [Header("Physics Ref:")]
    public Rigidbody2D m_rigidbody;

    [Header("Rotation:")]
    [SerializeField] private bool rotateOverTime = true;
    [Range(0, 60)][SerializeField] private float rotationSpeed = 4;

    [Header("Distance:")]
    [SerializeField] private bool hasMaxDistance = false;
    [SerializeField] private float maxDistnace = 20f;

    [HideInInspector] public Vector2 grapplePoint;
    [HideInInspector] public Vector2 grappleDistanceVector;

    [Header("Orbit Collision")]
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private float collisionOffset = 0.02f;

    private readonly RaycastHit2D[] collisionHits = new RaycastHit2D[10];

    private bool isOrbiting;
    private float orbitDirection = 1f;
    private RigidbodyType2D originalBodyType;

    public bool IsOrbiting => isOrbiting;

    private void Awake()
    {
        if (gunHolder == null) gunHolder = transform.root;
        if (m_rigidbody == null) m_rigidbody = gunHolder.GetComponent<Rigidbody2D>();
        if (playerMovement == null) playerMovement = gunHolder.GetComponent<PlayerMovement>();
        if (m_camera == null) m_camera = Camera.main;
    }

    private void Start()
    {
        grappleRope.enabled = false;
        originalBodyType = m_rigidbody.bodyType;
        isOrbiting = false;
    }

    private void Update()
    {
        if (grappleRope.enabled)
            RotateGun(grapplePoint, false);
        else
            RotateGun(GetMouseWorldPosition(), true);
    }

    private void FixedUpdate()
    {
        if (!isOrbiting) 
            return;

        Orbit();
    }

    private void Orbit()
    {
        m_rigidbody.linearVelocity = Vector2.zero;
        m_rigidbody.angularVelocity = 0f;

        Vector2 direction = m_rigidbody.position - grapplePoint;
        float radius = direction.magnitude;

        if (radius < 0.001f)
            return;

        float speed = playerMovement.CurrentSpeed;
        float angle = orbitDirection * speed / radius * Mathf.Rad2Deg * Time.fixedDeltaTime;

        Vector2 nextPosition = grapplePoint + (Vector2)(Quaternion.Euler(0f, 0f, angle) * direction);
        Vector2 movement = nextPosition - m_rigidbody.position;

        if (movement.sqrMagnitude < 0.000001f)
            return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(collisionLayers);
        filter.useLayerMask = true;

        int hitCount = m_rigidbody.Cast(movement.normalized, filter, collisionHits, movement.magnitude + collisionOffset);

        if (hitCount > 0)
        {
            playerMovement.StopMovement();
            return;
        }

        m_rigidbody.MovePosition(nextPosition);
        m_rigidbody.MoveRotation(m_rigidbody.rotation + angle);
    }

    private void RotateGun(Vector3 lookPoint, bool allowRotationOverTime)
    {
        Vector3 distanceVector = lookPoint - gunPivot.position;
        float angle = Mathf.Atan2(distanceVector.y, distanceVector.x) * Mathf.Rad2Deg;

        if (rotateOverTime && allowRotationOverTime)
        {
            gunPivot.rotation = Quaternion.Lerp(gunPivot.rotation, Quaternion.AngleAxis(angle, Vector3.forward), Time.deltaTime * rotationSpeed);
        }
        else
        {
            gunPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void SetGrapplePoint()
    {
        Vector2 mousePos = GetMouseWorldPosition();
        Vector2 origin = firePoint.position;
        Vector2 direction = mousePos - origin;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        int playerLayer = gunHolder.gameObject.layer;
        int layerMask = grappleToAll ? ~(1 << playerLayer) : 1 << grappableLayerNumber;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction.normalized, direction.magnitude, layerMask);

        if (hit.collider == null)
            return;

        if (!grappleToAll && hit.collider.gameObject.layer != grappableLayerNumber)
            return;

        if (hasMaxDistance && Vector2.Distance(hit.point, origin) > maxDistnace)
            return;

        grapplePoint = hit.point;
        grappleDistanceVector = grapplePoint - (Vector2)gunPivot.position;

        grappleRope.enabled = true;
    }

    public void Grapple()
    {
        if (!playerMovement.IsGrappleHeld())
            return;

        if (isOrbiting)
            return;

        if (!grappleRope.enabled)
            return;

        StartOrbit();
    }

    private void StartOrbit()
    {
        Vector2 direction = (Vector2)gunHolder.position - grapplePoint;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        Vector2 tangent = new Vector2(-direction.y, direction.x).normalized;
        Vector2 facingDirection = gunHolder.right;

        float dot = Vector2.Dot(facingDirection, tangent);

        orbitDirection = dot >= 0f ? 1f : -1f;

        Vector2 orbitTangent = tangent * orbitDirection;
        float angle = Mathf.Atan2(orbitTangent.y, orbitTangent.x) * Mathf.Rad2Deg;

        originalBodyType = m_rigidbody.bodyType;
        m_rigidbody.linearVelocity = Vector2.zero;

        gunHolder.rotation = Quaternion.Euler(0f, 0f, angle);

        isOrbiting = true;
    }

    public void StopGrapple()
    {
        isOrbiting = false;
        grappleRope.enabled = false;

        m_rigidbody.linearVelocity = (Vector2)gunHolder.right * playerMovement.CurrentSpeed;
        m_rigidbody.angularVelocity = 0f;
    }

    private Vector2 GetMouseWorldPosition()
    {
        if (Mouse.current == null)
            return firePoint.position;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Ray ray = m_camera.ScreenPointToRay(mousePosition);
        Plane gamePlane = new Plane(Vector3.forward, Vector3.zero);

        if (gamePlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            return new Vector2(worldPos.x, worldPos.y);
        }

        return firePoint.position;
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint != null && hasMaxDistance)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, maxDistnace);
        }
    }
}

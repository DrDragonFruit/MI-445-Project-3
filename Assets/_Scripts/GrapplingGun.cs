using UnityEngine;

public class GrapplingGun : MonoBehaviour
{
    [Header("Scripts Ref:")]
    public GrapplingRope grappleRope;

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
    public SpringJoint2D m_springJoint2D;
    public Rigidbody2D m_rigidbody;

    [Header("Rotation:")]
    [SerializeField] private bool rotateOverTime = true;
    [Range(0, 60)]
    [SerializeField] private float rotationSpeed = 4f;

    [Header("Orbit Settings:")]
    [SerializeField] private float orbitAcceleration = 20f;

    private float orbitSpeed;
    private float orbitDirection = 1f;
    private bool isOrbiting;

    public bool IsOrbiting => isOrbiting;

    [Header("Distance:")]
    [SerializeField] private bool hasMaxDistance = false;
    [SerializeField] private float maxDistnace = 20f;

    private enum LaunchType
    {
        Transform_Launch,
        Physics_Launch
    }

    [Header("Launching:")]
    [SerializeField] private bool launchToPoint = true;
    [SerializeField] private LaunchType launchType = LaunchType.Physics_Launch;
    [SerializeField] private float launchSpeed = 1f;

    [Header("No Launch To Point:")]
    [SerializeField] private bool autoConfigureDistance = false;
    [SerializeField] private float targetDistance = 3f;
    [SerializeField] private float targetFrequncy = 1f;

    [HideInInspector] public Vector2 grapplePoint;
    [HideInInspector] public Vector2 grappleDistanceVector;

    private PlayerMovement playerMovement;

    private void Awake()
    {
        if (m_rigidbody == null) 
            m_rigidbody = gunHolder.GetComponent<Rigidbody2D>();

        if (m_springJoint2D == null) 
            m_springJoint2D = gunHolder.GetComponent<SpringJoint2D>();

        if (m_camera == null) 
            m_camera = Camera.main;

        playerMovement = gunHolder.GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        grappleRope.enabled = false;
        m_springJoint2D.enabled = false;
        isOrbiting = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            SetGrapplePoint();
        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            if (grappleRope.enabled)
            {
                RotateGun(grapplePoint, false);
            }
            else
            {
                Vector2 mousePos = GetMouseWorldPosition();
                RotateGun(mousePos, true);
            }

            if (launchToPoint && grappleRope.isGrappling && launchType == LaunchType.Transform_Launch)
            {
                Vector2 firePointDistance = (Vector2)firePoint.position - (Vector2)gunHolder.position;
                Vector2 targetPos = grapplePoint - firePointDistance;
                gunHolder.position = Vector2.Lerp(gunHolder.position, targetPos, Time.deltaTime * launchSpeed);
            }
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            StopGrapple();
        }
        else
        {
            Vector2 mousePos = GetMouseWorldPosition();
            RotateGun(mousePos, true);
        }
    }

    private void FixedUpdate()
    {
        if (!isOrbiting) 
            return;

        float targetSpeed = Input.GetKey(KeyCode.W) ? playerMovement.MaxSpeed : 0f;
        float acceleration = Input.GetKey(KeyCode.W) ? playerMovement.Acceleration : playerMovement.Deceleration;

        orbitSpeed = Mathf.MoveTowards(orbitSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);

        Vector2 direction = m_rigidbody.position - grapplePoint;
        if (direction.sqrMagnitude < 0.0001f) 
            return;

        Vector2 tangent = new Vector2(-direction.y, direction.x).normalized * orbitDirection;
        Vector2 targetVelocity = tangent * orbitSpeed;

        m_rigidbody.linearVelocity = Vector2.MoveTowards(m_rigidbody.linearVelocity, targetVelocity, orbitAcceleration * Time.fixedDeltaTime);

        Vector2 velocity = m_rigidbody.linearVelocity;

        if (velocity.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            m_rigidbody.MoveRotation(angle);
        }
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

    private void SetGrapplePoint()
    {
        Vector2 mousePos = GetMouseWorldPosition();
        Vector2 origin = firePoint.position;
        Vector2 direction = mousePos - origin;

        if (direction.sqrMagnitude < 0.0001f) 
            return;

        int layerMask = grappleToAll ? ~LayerMask.GetMask("Player") : 1 << grappableLayerNumber;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction.normalized, direction.magnitude, layerMask);

        if (hit.collider == null)
        {
            Debug.Log("No surface hit!");
            return;
        }

        if (!grappleToAll && hit.collider.gameObject.layer != grappableLayerNumber)
        {
            Debug.Log("Object is not grappleable!");
            return;
        }

        if (hasMaxDistance && Vector2.Distance(hit.point, origin) > maxDistnace) 
            return;

        grapplePoint = hit.point;
        grappleDistanceVector = grapplePoint - (Vector2)gunPivot.position;

        grappleRope.enabled = true;
    }

    public void Grapple()
    {
        m_springJoint2D.enabled = false;
        m_springJoint2D.connectedBody = null;
        m_springJoint2D.autoConfigureDistance = false;
        m_springJoint2D.connectedAnchor = grapplePoint;

        if (!launchToPoint && !autoConfigureDistance)
        {
            m_springJoint2D.distance = targetDistance;
            m_springJoint2D.frequency = targetFrequncy;
        }
        else if (!launchToPoint && autoConfigureDistance)
        {
            m_springJoint2D.autoConfigureDistance = true;
            m_springJoint2D.frequency = 0f;
        }
        else
        {
            m_springJoint2D.distance = Vector2.Distance(m_rigidbody.position, grapplePoint);
            m_springJoint2D.dampingRatio = 1f;
            m_springJoint2D.frequency = 10f;
        }

        m_springJoint2D.enabled = true;
        StartOrbit();
    }

    private void StartOrbit()
    {
        Vector2 direction = m_rigidbody.position - grapplePoint;
        if (direction.sqrMagnitude < 0.0001f) 
            return;

        Vector2 tangent = new Vector2(-direction.y, direction.x).normalized;
        float dot = Vector2.Dot(m_rigidbody.linearVelocity, tangent);

        orbitDirection = dot >= 0f ? 1f : -1f;
        orbitSpeed = m_rigidbody.linearVelocity.magnitude;

        isOrbiting = true;

        Debug.Log("Orbiting started!");
    }

    private void StopGrapple()
    {
        isOrbiting = false;
        grappleRope.enabled = false;
        m_springJoint2D.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint != null && hasMaxDistance)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, maxDistnace);
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);
        Plane gamePlane = new Plane(Vector3.forward, Vector3.zero);

        if (gamePlane.Raycast(ray, out float distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            return new Vector2(worldPos.x, worldPos.y);
        }

        return firePoint.position;
    }
}
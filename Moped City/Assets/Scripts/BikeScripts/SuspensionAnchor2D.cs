using UnityEngine;

public class SuspensionAnchor2D : MonoBehaviour
{
    [Header("Suspension Settings")]
    public float rayLength = 1f;
    public LayerMask groundMask = ~0;
    [Range(1, 4)] public int suspensionPoints = 1;

    [Header("Spring Settings")]
    public float restHeight = 0.5f;
    public float springStrength = 800f;
    public float springDamping = 15f;

    [Header("Wheel Visuals")]
    public Transform wheelVisual;
    public float wheelRadius = 0.25f;
    public bool rotateWheel = true;
    public float rotationSpeed = 200f;
    public float wheelSmoothSpeed = 12f;    // Smoothing factor for wheel movement
    private Vector3 targetWheelPosition;    // Target position for smooth interpolation
    private Vector3 currentVelocity;        // Reference velocity for SmoothDamp

    [Header("Stability Settings")]
    public float stabilityForce = 0.5f;
    public float angularDamping = 10f;
    public float maxStabilityTorque = 200f;

    private Rigidbody2D bikeRigidbody;
    private float lastLength;
    private RaycastHit2D lastHit;
    private float springVelocity;
    private float wheelRotation;
    private float currentCompression;       // Track current suspension compression

    [Header("Debug")]
    public bool showDebugRays = true;

    void Awake()
    {
        bikeRigidbody = GetComponentInParent<Rigidbody2D>();
        if (bikeRigidbody == null)
        {
            Debug.LogError("No Rigidbody2D found in parent objects!", this);
            enabled = false;
            return;
        }
        lastLength = rayLength;

        // Initialize wheel position
        if (wheelVisual != null)
        {
            targetWheelPosition = wheelVisual.position;
        }
    }

    void FixedUpdate()
    {
        CastSuspensionRay();
        ApplySuspension();
    }

    void Update()
    {
        UpdateWheelVisuals();
    }

    void CastSuspensionRay()
    {
        Vector2 rayOrigin = transform.position;
        Vector2 rayDirection = (Vector2)(transform.TransformDirection(Vector3.down));
        lastHit = Physics2D.Raycast(rayOrigin, rayDirection, rayLength, groundMask);
    }

    void ApplySuspension()
    {
        if (!lastHit.collider)
        {
            currentCompression = 0;
            return;
        }

        // Calculate spring compression
        float suspensionLength = lastHit.distance;
        currentCompression = rayLength - suspensionLength;

        // Calculate spring velocity
        springVelocity = (suspensionLength - lastLength) / Time.fixedDeltaTime;
        lastLength = suspensionLength;

        // Calculate suspension force
        float springForce = (currentCompression - restHeight) * springStrength;
        float dampingForce = springVelocity * springDamping;
        float suspensionForce = springForce - dampingForce;

        if (suspensionForce > 0)
        {
            // Use the opposite of ray direction for force
            Vector2 forceDir = (Vector2)(transform.TransformDirection(Vector3.up));
            Vector2 forcePoint = lastHit.point;

            // Apply suspension force
            bikeRigidbody.AddForceAtPosition(forceDir * suspensionForce, forcePoint, ForceMode2D.Force);

            // Improved stability system
            ApplyStabilityForces();

            // Gravity compensation based on compression
            float compressionRatio = Mathf.Clamp01(currentCompression / rayLength);
            Vector2 gravityForce = -Physics2D.gravity * bikeRigidbody.mass * compressionRatio;
            bikeRigidbody.AddForceAtPosition(gravityForce * Time.fixedDeltaTime, forcePoint, ForceMode2D.Force);
        }
    }

    void UpdateWheelVisuals()
    {
        if (wheelVisual == null) return;

        // Calculate target wheel position, maintaining X position relative to suspension anchor
        Vector3 targetPos = wheelVisual.position;
        if (lastHit.collider != null)
        {
            // Only update Y position based on suspension compression
            float compressedOffset = Mathf.Lerp(wheelRadius, wheelRadius * 0.8f, currentCompression / rayLength);
            targetPos.y = lastHit.point.y + compressedOffset;
        }
        else
        {
            // When not grounded, extend to maximum length while maintaining X position
            targetPos.y = transform.position.y - (rayLength - wheelRadius);
        }

        // Keep X position locked to suspension anchor point
        targetPos.x = transform.position.x;

        // Smoothly move wheel on Y axis only
        wheelVisual.position = Vector3.SmoothDamp(
            wheelVisual.position,
            targetPos,
            ref currentVelocity,
            1f / wheelSmoothSpeed
        );

        // Update wheel rotation with correct axis of rotation
        if (rotateWheel)
        {
            // Calculate rotation based on horizontal velocity
            float horizontalSpeed = bikeRigidbody.linearVelocity.x;
            wheelRotation += horizontalSpeed * rotationSpeed * Time.deltaTime;
            wheelRotation %= 360f;

            // Create rotation around X-axis while maintaining initial X:90 orientation
            Quaternion baseRotation = Quaternion.Euler(90, 0, 0);
            Quaternion spinRotation = Quaternion.Euler(wheelRotation, 0, 0);
            wheelVisual.rotation = baseRotation * spinRotation;
        }
        else
        {
            // Keep wheel at X:90 and only match bike's rotation if needed
            wheelVisual.rotation = Quaternion.Euler(90, 0, 0);
        }
    }

    void ApplyStabilityForces()
    {
        // Get current state
        float currentAngle = bikeRigidbody.rotation;
        float angularVel = bikeRigidbody.angularVelocity;

        // Normalize angle to -180 to 180 range
        while (currentAngle > 180f) currentAngle -= 360f;
        while (currentAngle < -180f) currentAngle += 360f;

        // Calculate stabilizing torque
        float angleStability = -currentAngle * stabilityForce;
        float rotationDamping = -angularVel * angularDamping;

        // Combine and clamp the total torque
        float totalTorque = angleStability + rotationDamping;
        totalTorque = Mathf.Clamp(totalTorque, -maxStabilityTorque, maxStabilityTorque);

        // Apply smoothly
        float smoothedTorque = totalTorque * Time.fixedDeltaTime;
        bikeRigidbody.AddTorque(smoothedTorque, ForceMode2D.Force);

        if (Mathf.Abs(angularVel) > 1000f)
        {
            bikeRigidbody.angularVelocity *= 0.9f;
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugRays) return;

        Vector3 start = transform.position;
        Vector3 rayDir = transform.TransformDirection(Vector3.down);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, start + rayDir * rayLength);

        if (Application.isPlaying && lastHit.collider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lastHit.point, 0.05f);
            Gizmos.color = Color.green;
            float currentLength = lastHit.distance;
            Gizmos.DrawLine(start, start + rayDir * currentLength);
        }
    }
}
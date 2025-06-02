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

    [Header("Stability Settings")]
    public float stabilityForce = 0.5f;        // Force to keep bike upright
    public float angularDamping = 10f;         // Damping for rotation
    public float maxStabilityTorque = 200f;    // Maximum stabilizing torque

    private Rigidbody2D bikeRigidbody;
    private float lastLength;
    private RaycastHit2D lastHit;
    private float springVelocity;

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
    }

    void FixedUpdate()
    {
        CastSuspensionRay();
        ApplySuspension();
    }

    void CastSuspensionRay()
    {
        Vector2 rayOrigin = transform.position;
        lastHit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, groundMask);
    }

    void ApplySuspension()
    {
        if (!lastHit.collider) return;

        // Calculate spring compression
        float suspensionLength = lastHit.distance;
        float compression = rayLength - suspensionLength;

        // Calculate spring velocity
        springVelocity = (suspensionLength - lastLength) / Time.fixedDeltaTime;
        lastLength = suspensionLength;

        // Calculate suspension force
        float springForce = (compression - restHeight) * springStrength;
        float dampingForce = springVelocity * springDamping;
        float suspensionForce = springForce - dampingForce;

        if (suspensionForce > 0)
        {
            Vector2 forceDir = Vector2.up;
            Vector2 forcePoint = lastHit.point;

            // Apply suspension force
            bikeRigidbody.AddForceAtPosition(forceDir * suspensionForce, forcePoint, ForceMode2D.Force);

            // Improved stability system
            ApplyStabilityForces();

            // Gravity compensation based on compression
            float compressionRatio = Mathf.Clamp01(compression / rayLength);
            Vector2 gravityForce = -Physics2D.gravity * bikeRigidbody.mass * compressionRatio;
            bikeRigidbody.AddForceAtPosition(gravityForce * Time.fixedDeltaTime, forcePoint, ForceMode2D.Force);
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

        // Emergency anti-spin when rotation is too fast
        if (Mathf.Abs(angularVel) > 1000f)
        {
            bikeRigidbody.angularVelocity *= 0.9f; // Emergency brake
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugRays) return;

        Vector3 start = transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, start + Vector3.down * rayLength);

        if (Application.isPlaying && lastHit.collider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lastHit.point, 0.05f);
            Gizmos.color = Color.green;
            float currentLength = lastHit.distance;
            Gizmos.DrawLine(start, start + Vector3.down * currentLength);
        }
    }
}
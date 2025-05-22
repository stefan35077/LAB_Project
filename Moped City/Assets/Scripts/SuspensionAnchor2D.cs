using UnityEngine;

public class SuspensionAnchor2D : MonoBehaviour
{
    [Header("Ray Settings")]
    public float rayLength = 1f;
    public LayerMask groundMask = ~0;

    [Header("Spring Settings")]
    public float stiffness = 1f;
    public float damping = 0.1f;
    public float valueThreshold = 0.01f;
    public float velocityThreshold = 0.01f;

    private Rigidbody2D bikeRigidbody;
    private float currentValue;    // Current suspension compression
    private float currentVelocity; // Current suspension velocity
    private float targetValue;     // Target compression
    private RaycastHit2D lastHit;
    private Vector2 initialLocalPos; // Initial local position of the suspension point

    void Awake()
    {
        // Get the bike's Rigidbody2D (parent)
        bikeRigidbody = GetComponentInParent<Rigidbody2D>();
        // Store initial local position
        initialLocalPos = transform.localPosition;

        currentValue = 0f; // 0 = fully extended, rayLength = fully compressed
        targetValue = 0f;
    }

    void FixedUpdate()
    {
        CastRay();
        UpdateSpring();
        ApplyForce();
    }

    void CastRay()
    {
        Vector2 rayStart = transform.position;
        Vector2 rayDirection = Vector2.down;

        lastHit = Physics2D.Raycast(rayStart, rayDirection, rayLength, groundMask);

        if (lastHit.collider != null)
        {
            // Calculate how much the suspension should compress
            float compression = rayLength - lastHit.distance;
            compression = Mathf.Max(0, compression); // Never negative
            targetValue = compression;
        }
        else
        {
            targetValue = 0f; // Fully extended when not hitting ground
        }
    }

    void UpdateSpring()
    {
        float dampingFactor = Mathf.Max(0, 1 - damping * Time.fixedDeltaTime);
        float acceleration = (targetValue - currentValue) * stiffness * Time.fixedDeltaTime;
        currentVelocity = currentVelocity * dampingFactor + acceleration;
        currentValue += currentVelocity * Time.fixedDeltaTime;

        if (Mathf.Abs(currentValue - targetValue) < valueThreshold &&
            Mathf.Abs(currentVelocity) < velocityThreshold)
        {
            currentValue = targetValue;
            currentVelocity = 0f;
        }
    }

    void ApplyForce()
    {
        if (lastHit.collider != null && bikeRigidbody != null)
        {
            // Calculate spring force based on compression
            float springForce = currentValue * stiffness;

            // Apply upward force at the contact point
            Vector2 forceDirection = Vector2.up;
            bikeRigidbody.AddForceAtPosition(forceDirection * springForce, lastHit.point);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw the suspension ray
        Vector3 start = transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(start, start + Vector3.down * rayLength);

        if (Application.isPlaying && lastHit.collider != null)
        {
            // Draw hit point
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lastHit.point, 0.05f);

            // Draw compression amount
            Gizmos.color = Color.green;
            float currentLength = rayLength - currentValue;
            Gizmos.DrawLine(start, start + Vector3.down * currentLength);
        }
    }
}
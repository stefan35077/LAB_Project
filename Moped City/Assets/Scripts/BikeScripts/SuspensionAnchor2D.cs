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
    public float wheelSmoothSpeed = 12f;
    private Vector3 targetWheelPosition;
    private Vector3 currentVelocity;

    [Header("Stability Settings")]
    public float stabilityForce = 0.5f;
    public float angularDamping = 10f;
    public float maxStabilityTorque = 200f;

    private Rigidbody2D bikeRigidbody;
    private float lastLength;
    private RaycastHit2D lastHit;
    private float springVelocity;
    private float wheelRotation;
    private float currentCompression;

    [Header("Debug")]
    public bool showDebugRays = true;


    private bool suspensionActive;
    private Vector2 cachedHitPoint;

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
            suspensionActive = false;
            return;
        }

        suspensionActive = true;
        cachedHitPoint = lastHit.point;

        // CALCULATE HOW MUCH THE SUSPENSION IS COMPRESSED BASED ON RAYCAST HIT DISTANCE
        float suspensionLength = lastHit.distance;
        currentCompression = rayLength - suspensionLength;

        // CALCULATE HOW FAST THE SPRING LENGTH IS CHANGING BETWEEN FRAMES
        springVelocity = (suspensionLength - lastLength) / Time.fixedDeltaTime;
        lastLength = suspensionLength;

        // THE SPRING FORCE IS BASED ON HOW MUCH THE SUSPENSION IS COMPRESSED FROM ITS REST HEIGHT
        float springForce = (currentCompression - restHeight) * springStrength;
        float dampingForce = springVelocity * springDamping;
        float suspensionForce = springForce - dampingForce;

        if (suspensionForce > 0)
        {
            Vector2 forceDir = (Vector2)(transform.TransformDirection(Vector3.up));
            Vector2 forcePoint = lastHit.point;

            // APPLY FORCE AT CONTACT POINT TO SIMULATE SUSPENSION REACTING TO GROUND CONTACT
            bikeRigidbody.AddForceAtPosition(forceDir * suspensionForce, forcePoint, ForceMode2D.Force);

            // ADD TORQUE TO KEEP THE BIKE UPRIGHT BASED ON ROTATION AND ANGULAR VELOCITY
            ApplyStabilityForces();

            // ADD A PORTION OF GRAVITY BACK BASED ON HOW COMPRESSED THE SUSPENSION IS (SIMULATES MASS PRESSING DOWN)
            float compressionRatio = Mathf.Clamp01(currentCompression / rayLength);
            Vector2 gravityForce = -Physics2D.gravity * bikeRigidbody.mass * compressionRatio;
            bikeRigidbody.AddForceAtPosition(gravityForce * Time.fixedDeltaTime, forcePoint, ForceMode2D.Force);
        }
    }

    void UpdateWheelVisuals()
    {
        if (wheelVisual == null) return;

        Vector3 upDirection = transform.TransformDirection(Vector3.up);
        Vector3 targetPos;

        if (lastHit.collider != null)
        {
            // Als we de grond raken ? plaats het wiel visueel op het contactpunt + radius
            targetPos = (Vector3)lastHit.point + upDirection * wheelRadius;
        }
        else
        {
            // Als we in de lucht zijn ? plaats het wiel op de rustpositie van de vering
            targetPos = transform.position - upDirection * (rayLength - restHeight - wheelRadius);
        }

        // Z behouden voor visuele stabiliteit in 2D/2.5D
        targetPos.z = wheelVisual.position.z;

        // Smooth beweging
        wheelVisual.position = Vector3.SmoothDamp(
            wheelVisual.position,
            targetPos,
            ref currentVelocity,
            1f / wheelSmoothSpeed
        );

        // Optioneel: wiel laten draaien
        if (rotateWheel)
        {
            float horizontalSpeed = bikeRigidbody.linearVelocity.x;
            wheelRotation += horizontalSpeed * rotationSpeed * Time.deltaTime;
            wheelRotation %= 360f;

            Quaternion baseRotation = Quaternion.Euler(90, 0, 0);
            Quaternion spinRotation = Quaternion.Euler(wheelRotation, 0, 0);
            wheelVisual.rotation = baseRotation * spinRotation;
        }
        else
        {
            wheelVisual.rotation = Quaternion.Euler(90, 0, 0);
        }
    }


    void ApplyStabilityForces()
    {
        float currentAngle = bikeRigidbody.rotation;
        float angularVel = bikeRigidbody.angularVelocity;

        // NORMALIZE ANGLE TO BE BETWEEN -180 AND 180 DEGREES
        while (currentAngle > 180f) currentAngle -= 360f;
        while (currentAngle < -180f) currentAngle += 360f;

        // GENERATE TORQUE TO STABILIZE THE BIKE BASED ON ANGLE AND VELOCITY
        float angleStability = -currentAngle * stabilityForce;
        float rotationDamping = -angularVel * angularDamping;

        float totalTorque = angleStability + rotationDamping;

        // LIMIT TORQUE TO A MAXIMUM VALUE TO PREVENT OVER-CORRECTION
        totalTorque = Mathf.Clamp(totalTorque, -maxStabilityTorque, maxStabilityTorque);

        float smoothedTorque = totalTorque * Time.fixedDeltaTime;
        bikeRigidbody.AddTorque(smoothedTorque, ForceMode2D.Force);

        // IF ANGULAR VELOCITY IS TOO HIGH, SLOW IT DOWN TO PREVENT SPINNING OUT
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

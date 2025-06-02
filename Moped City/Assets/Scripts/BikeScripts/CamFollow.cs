using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 baseOffset = new Vector3(0, 5, -10);
    public float smoothSpeed = 0.125f;

    [Header("Zoom Settings")]
    public float minZoom = 5f;
    public float maxZoom = 15f;
    public float zoomSpeed = 5f;
    public float velocityToZoomFactor = 0.5f;

    private Camera cam;
    private Rigidbody targetRb;

    void Start()
    {
        cam = Camera.main;

        if (target != null)
            targetRb = target.GetComponent<Rigidbody>();

        if (cam == null)
        {
            Debug.LogError("Main camera not found!");
            enabled = false;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Move camera smoothly
        Vector3 desiredPosition = target.position + baseOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Look at target
        transform.LookAt(target);

        // --- Dynamic Zoom ---
        float speed = targetRb != null ? targetRb.linearVelocity.magnitude : 0f;
        float targetZoom = Mathf.Lerp(minZoom, maxZoom, speed * velocityToZoomFactor);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetZoom, Time.deltaTime * zoomSpeed);
    }
}

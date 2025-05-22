using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target; // The object the camera will follow
    public Vector3 offset = new Vector3(0, 5, -10); // Offset from the target
    public float smoothSpeed = 0.125f; // Smoothness factor (lower = smoother)

    void LateUpdate()
    {
        if (target == null) return; // Ensure the target is assigned

        // Desired position of the camera
        Vector3 desiredPosition = target.position + offset;

        // Smoothly interpolate between current and desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Update the camera's position
        transform.position = smoothedPosition;

        // Optional: Make the camera look at the target
        transform.LookAt(target);
    }
}
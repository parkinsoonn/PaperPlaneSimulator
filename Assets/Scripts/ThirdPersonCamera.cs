using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 7f;
    public float height = 2.5f;
    public float smoothTime = 0.15f;
    public float lookAhead = 3f;

    Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;

        // Follow the heading only, so banking and pitching don't swing the camera around.
        Vector3 flatForward = Vector3.ProjectOnPlane(target.forward, Vector3.up);
        if (flatForward.sqrMagnitude < 0.001f) flatForward = transform.forward;
        flatForward.Normalize();

        Vector3 desired = target.position - flatForward * distance + Vector3.up * height;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        transform.LookAt(target.position + flatForward * lookAhead);
    }
}

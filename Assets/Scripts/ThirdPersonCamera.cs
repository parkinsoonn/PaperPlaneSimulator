using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 7f;
    public float height = 2.5f;
    public float smoothTime = 0.15f;
    public float lookAhead = 3f;
    public float headingSmoothTime = 0.1f;    // how softly the camera swings around when the plane turns
    public float rotationSharpness = 10f;     // how quickly the view catches up with the look point

    Vector3 velocity;
    float headingYaw, headingVelocity;
    bool initialized;

    void LateUpdate()
    {
        if (target == null) return;

        // Follow the heading only, so banking and pitching don't swing the camera around.
        Vector3 flatTarget = Vector3.ProjectOnPlane(target.forward, Vector3.up);
        float targetYaw = flatTarget.sqrMagnitude < 0.001f
            ? headingYaw
            : Mathf.Atan2(flatTarget.x, flatTarget.z) * Mathf.Rad2Deg;

        if (!initialized)
        {
            headingYaw = targetYaw;
            initialized = true;
        }
        headingYaw = Mathf.SmoothDampAngle(headingYaw, targetYaw, ref headingVelocity, headingSmoothTime);
        Vector3 flatForward = Quaternion.Euler(0f, headingYaw, 0f) * Vector3.forward;

        Vector3 desired = target.position - flatForward * distance + Vector3.up * height;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);

        Vector3 lookDir = target.position + flatForward * lookAhead - transform.position;
        if (lookDir.sqrMagnitude < 0.0001f) return;
        Quaternion look = Quaternion.LookRotation(lookDir, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime));
    }
}

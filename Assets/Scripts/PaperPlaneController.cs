using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PaperPlaneController : MonoBehaviour
{
    [Header("Launch")]
    public float minLaunchSpeed = 8f;
    public float maxLaunchSpeed = 28f;
    public float maxChargeTime = 1.5f;  // seconds of holding Space for full power
    public float launchPitch = 30f;

    [Header("Controls")]
    public float pitchRate = 60f;      // W/S, degrees per second
    public float turnRate = 70f;       // A/D yaw, degrees per second
    public float maxBank = 40f;
    public float minPitch = -60f;
    public float maxPitch = 45f;

    [Header("Aerodynamics")]
    public float liftCoefficient = 0.12f;   // lift accel = coef * forwardSpeed^2
    public float dragCoefficient = 0.01f;   // drag accel = coef * speed^2
    public float velocityAlign = 2.5f;      // how fast velocity follows the nose
    public float noseAlign = 1.5f;          // how fast the nose drops toward the flight path

    Rigidbody rb;
    bool flying;
    float yaw, pitch, bank;
    float launchGrace;   // ignore touching the pad right after launch

    public float Charge { get; private set; }   // 0..1, how long Space has been held
    public bool IsCharging { get; private set; }
    public bool IsFlying => flying;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        yaw = transform.eulerAngles.y;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null || flying) return;

        if (kb.spaceKey.wasPressedThisFrame) IsCharging = true;
        if (IsCharging && kb.spaceKey.isPressed)
            Charge = Mathf.Clamp01(Charge + Time.deltaTime / maxChargeTime);
        if (IsCharging && kb.spaceKey.wasReleasedThisFrame)
            Launch(Charge);
    }

    void Launch(float charge)
    {
        IsCharging = false;
        Charge = 0f;
        flying = true;
        launchGrace = 0.3f;
        pitch = launchPitch;
        bank = 0f;
        rb.rotation = Quaternion.Euler(-pitch, yaw, 0f);
        rb.linearVelocity = rb.rotation * Vector3.forward * Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, charge);
    }

    void FixedUpdate()
    {
        if (!flying) return;

        float dt = Time.fixedDeltaTime;
        launchGrace -= dt;
        var kb = Keyboard.current;
        float pitchInput = 0f, turnInput = 0f;
        if (kb != null)
        {
            if (kb.wKey.isPressed) pitchInput += 1f;
            if (kb.sKey.isPressed) pitchInput -= 1f;
            if (kb.dKey.isPressed) turnInput += 1f;
            if (kb.aKey.isPressed) turnInput -= 1f;
        }

        Vector3 vel = rb.linearVelocity;
        float speed = vel.magnitude;

        // Nose slowly drops toward the flight path, so a slow plane noses down and glides.
        if (speed > 0.1f)
        {
            float pathAngle = Mathf.Asin(Mathf.Clamp(vel.y / speed, -1f, 1f)) * Mathf.Rad2Deg;
            pitch = Mathf.MoveTowards(pitch, pathAngle, noseAlign * Mathf.Abs(pitch - pathAngle) * dt);
        }

        pitch = Mathf.Clamp(pitch + pitchInput * pitchRate * dt, minPitch, maxPitch);
        yaw += turnInput * turnRate * dt;
        bank = Mathf.Lerp(bank, -turnInput * maxBank, 4f * dt);

        Quaternion rot = Quaternion.Euler(-pitch, yaw, bank);
        rb.MoveRotation(rot);

        Vector3 forward = rot * Vector3.forward;
        Vector3 right = rot * Vector3.right;

        // Velocity bends toward where the nose points (keeps speed, turns climb into slowdown).
        vel = Vector3.Slerp(vel, forward * speed, velocityAlign * dt);
        rb.linearVelocity = vel;
        if (speed < 0.1f) return;

        // Lift is perpendicular to the velocity so it never adds energy; drag only removes it,
        // which is what makes the plane eventually glide down.
        Vector3 liftDir = Vector3.Cross(vel / speed, right).normalized;
        float forwardSpeed = Mathf.Max(0f, Vector3.Dot(vel, forward));
        rb.AddForce(liftDir * liftCoefficient * forwardSpeed * forwardSpeed, ForceMode.Acceleration);
        rb.AddForce(-vel * dragCoefficient * speed, ForceMode.Acceleration);
    }

    void OnCollisionEnter(Collision collision) => Land();
    void OnCollisionStay(Collision collision) => Land();

    void Land()
    {
        if (!flying || launchGrace > 0f) return;
        flying = false;
        pitch = 0f;
        bank = 0f;
        rb.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}

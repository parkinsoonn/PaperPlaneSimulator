using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PaperPlaneController : MonoBehaviour
{
    [Header("Launch")]
    public float minLaunchSpeed = 8f;
    public float maxLaunchSpeed = 28f;
    public float maxChargeTime = 1.5f;  // seconds for the power to sweep 0 -> 100 (then back down)
    public float launchPitch = 30f;

    [Header("Controls")]
    public float pitchRate = 60f;      // W/S, degrees per second
    public float turnRate = 70f;       // A/D yaw, degrees per second
    public float maxBank = 40f;
    public float minPitch = -60f;
    public float maxPitch = 45f;
    public float turnResponse = 10f;   // how fast the turn ramps in/out, so A/D never snaps the heading

    [Header("Mouse")]
    public float mouseSensitivity = 0.004f;   // virtual stick units per pixel of mouse movement
    public float mouseRecenter = 1.5f;        // per second; the stick drifts back so the plane levels out
    public bool invertMouseY = false;

    [Header("Corridor")]
    public float corridorHalfWidth = 10f;   // street + sidewalk, centered on the launch point, running along +Z
    public float corridorSoftZone = 4f;     // within this distance of the edge the plane is eased back

    [Header("Aerodynamics")]
    public float liftCoefficient = 0.12f;   // lift accel = coef * forwardSpeed^2
    public float dragCoefficient = 0.01f;   // drag accel = coef * speed^2
    public float velocityAlign = 2.5f;      // how fast velocity follows the nose
    public float noseAlign = 1.5f;          // how fast the nose drops toward the flight path

    Rigidbody rb;
    bool flying;
    float yaw, pitch, bank;
    float turnAxis;      // smoothed A/D input, -1..1
    Vector2 mouseStick;  // virtual joystick driven by mouse movement, -1..1
    float launchGrace;   // ignore touching the pad right after launch

    public float Charge { get; private set; }   // 0..1, swings back and forth while Space is held
    public bool IsCharging { get; private set; }
    public bool IsFlying => flying;
    public float CorridorCenterX => startPosition.x;
    public bool HasLanded { get; private set; }  // landed and waiting for a restart
    public float FlightDistance { get; private set; }   // horizontal meters from the launch point; kept after landing
    Vector3 launchPoint;
    bool hasLaunched;
    float chargeTime;
    Vector3 startPosition;
    Quaternion startRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        startPosition = transform.position;
        startRotation = transform.rotation;
        yaw = transform.eulerAngles.y;
    }

    void Update()
    {
        if (flying)
        {
            UpdateMouseStick();
            return;
        }

        var kb = Keyboard.current;
        if (kb == null || HasLanded) return;

        if (kb.spaceKey.wasPressedThisFrame)
        {
            IsCharging = true;
            chargeTime = 0f;
        }
        if (IsCharging && kb.spaceKey.isPressed)
        {
            chargeTime += Time.deltaTime;
            Charge = Mathf.PingPong(chargeTime / maxChargeTime, 1f);
        }
        if (IsCharging && kb.spaceKey.wasReleasedThisFrame)
            Launch(Charge);
    }

    // Mouse movement pushes a virtual stick; it eases back to center when the mouse stops.
    void UpdateMouseStick()
    {
        var mouse = Mouse.current;
        if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 delta = mouse.delta.ReadValue() * mouseSensitivity;
            if (invertMouseY) delta.y = -delta.y;
            mouseStick += delta;
            mouseStick.x = Mathf.Clamp(mouseStick.x, -1f, 1f);
            mouseStick.y = Mathf.Clamp(mouseStick.y, -1f, 1f);
        }
        mouseStick = Vector2.MoveTowards(mouseStick, Vector2.zero, mouseRecenter * Time.deltaTime);
    }

    // Puts the plane back on the launch pad, ready for a new throw.
    public void ResetToPad()
    {
        flying = false;
        HasLanded = false;
        hasLaunched = false;
        IsCharging = false;
        Charge = 0f;
        FlightDistance = 0f;
        pitch = 0f;
        bank = 0f;
        turnAxis = 0f;
        mouseStick = Vector2.zero;
        yaw = startRotation.eulerAngles.y;
        rb.linearVelocity = Vector3.zero;
        rb.position = startPosition;
        rb.rotation = startRotation;
        transform.SetPositionAndRotation(startPosition, startRotation);
    }

    void Launch(float charge)
    {
        IsCharging = false;
        Charge = 0f;
        flying = true;
        launchGrace = 0.3f;
        launchPoint = rb.position;
        hasLaunched = true;
        FlightDistance = 0f;
        pitch = launchPitch;
        bank = 0f;
        turnAxis = 0f;
        mouseStick = Vector2.zero;
        rb.rotation = Quaternion.Euler(-pitch, yaw, 0f);
        rb.linearVelocity = rb.rotation * Vector3.forward * Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, charge);
    }

    void FixedUpdate()
    {
        // Keep measuring after touchdown so the slide counts toward the throw.
        if (hasLaunched)
        {
            Vector3 travelled = rb.position - launchPoint;
            FlightDistance = new Vector2(travelled.x, travelled.z).magnitude;
        }
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
        pitchInput = Mathf.Clamp(pitchInput + mouseStick.y, -1f, 1f);
        turnInput = Mathf.Clamp(turnInput + mouseStick.x, -1f, 1f);

        Vector3 vel = rb.linearVelocity;
        float speed = vel.magnitude;

        // Nose slowly drops toward the flight path, so a slow plane noses down and glides.
        if (speed > 0.1f)
        {
            float pathAngle = Mathf.Asin(Mathf.Clamp(vel.y / speed, -1f, 1f)) * Mathf.Rad2Deg;
            pitch = Mathf.MoveTowards(pitch, pathAngle, noseAlign * Mathf.Abs(pitch - pathAngle) * dt);
        }

        // Near the street edge, steer back toward the middle (only while still drifting outward).
        float offset = rb.position.x - CorridorCenterX;
        float edge = Mathf.InverseLerp(corridorHalfWidth - corridorSoftZone, corridorHalfWidth, Mathf.Abs(offset));
        if (edge > 0f && (Mathf.Sign(vel.x) == Mathf.Sign(offset) || edge >= 1f))
            turnInput = Mathf.Clamp(turnInput - Mathf.Sign(offset) * edge * 1.5f, -1f, 1f);

        turnAxis = Mathf.MoveTowards(turnAxis, turnInput, turnResponse * dt);

        pitch = Mathf.Clamp(pitch + pitchInput * pitchRate * dt, minPitch, maxPitch);
        yaw += turnAxis * turnRate * dt;
        bank = Mathf.Lerp(bank, -turnAxis * maxBank, 8f * dt);

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

    // Speed pickup: adds speed along the current flight direction.
    public void AddBoost(float amount)
    {
        if (!flying) return;
        Vector3 vel = rb.linearVelocity;
        if (vel.sqrMagnitude < 0.01f) vel = transform.forward;
        rb.linearVelocity += vel.normalized * amount;
    }

    void OnCollisionEnter(Collision collision) => Land();
    void OnCollisionStay(Collision collision) => Land();

    void Land()
    {
        if (!flying || launchGrace > 0f) return;
        flying = false;
        HasLanded = true;
        pitch = 0f;
        bank = 0f;
        rb.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}

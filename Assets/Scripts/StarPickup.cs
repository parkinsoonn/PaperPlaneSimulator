using UnityEngine;

// A floating star: spins in place and gives the plane a speed boost (and some gold) when flown through.
public class StarPickup : MonoBehaviour
{
    public float boost = 3f;
    public int gold = 1;
    public float spinSpeed = 120f;
    public Transform glow;          // halo around the star (optional)
    public float pulseAmount = 0.15f;
    public float pulseSpeed = 4f;

    Vector3 glowScale;
    float phase;

    void Start()
    {
        if (glow != null) glowScale = glow.localScale;
        phase = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        if (glow != null)
            glow.localScale = glowScale * (1f + pulseAmount * Mathf.Sin(Time.time * pulseSpeed + phase));
    }

    void OnTriggerEnter(Collider other)
    {
        var plane = other.attachedRigidbody != null ? other.attachedRigidbody.GetComponent<PaperPlaneController>() : null;
        if (plane == null || !plane.IsFlying) return;
        plane.AddBoost(boost);
        plane.AddGold(gold);
        Destroy(gameObject);
    }
}

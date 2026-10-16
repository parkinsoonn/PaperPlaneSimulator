using UnityEngine;

// A floating star: spins in place and gives the plane a speed boost when flown through.
public class StarPickup : MonoBehaviour
{
    public float boost = 3f;
    public float spinSpeed = 120f;

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        var plane = other.attachedRigidbody != null ? other.attachedRigidbody.GetComponent<PaperPlaneController>() : null;
        if (plane == null || !plane.IsFlying) return;
        plane.AddBoost(boost);
        Destroy(gameObject);
    }
}

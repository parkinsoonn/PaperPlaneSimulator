using UnityEngine;

// Faint wind streaks from both wingtips while the plane is flying.
[RequireComponent(typeof(PaperPlaneController))]
public class WindTrail : MonoBehaviour
{
    public Material material;
    public Vector3 wingtipOffset = new Vector3(0.79f, 0.59f, -1.1f);   // right wingtip, local; mirrored for the left
    public float trailTime = 0.35f;
    public float width = 0.035f;
    [Range(0f, 1f)] public float alpha = 0.3f;

    PaperPlaneController plane;
    TrailRenderer[] trails;

    void Awake()
    {
        plane = GetComponent<PaperPlaneController>();
        var mirrored = new Vector3(-wingtipOffset.x, wingtipOffset.y, wingtipOffset.z);
        trails = new[] { CreateTrail("WindTrailRight", wingtipOffset), CreateTrail("WindTrailLeft", mirrored) };
    }

    TrailRenderer CreateTrail(string name, Vector3 localPosition)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPosition;

        var trail = go.AddComponent<TrailRenderer>();
        trail.sharedMaterial = material;
        trail.time = trailTime;
        trail.minVertexDistance = 0.1f;
        trail.widthCurve = AnimationCurve.Linear(0f, width, 1f, 0f);
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(alpha, 0f), new GradientAlphaKey(0f, 1f) });
        trail.colorGradient = gradient;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;
        trail.emitting = false;
        return trail;
    }

    void Update()
    {
        bool onPad = !plane.IsFlying && !plane.HasLanded;
        foreach (var trail in trails)
        {
            trail.emitting = plane.IsFlying;
            // Don't draw a streak across the reset teleport back to the pad.
            if (onPad && trail.positionCount > 0) trail.Clear();
        }
    }
}

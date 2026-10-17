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
        trail.minVertexDistance = 0.1f;
        ApplyStyle(trail, null, width, trailTime);
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trail.receiveShadows = false;
        trail.emitting = false;
        return trail;
    }

    // Shop effect: colors along the streak (null = the classic faint white), thickness and length.
    public void SetStyle(Gradient colors, float trailWidth, float time)
    {
        foreach (var trail in trails) ApplyStyle(trail, colors, trailWidth, time);
    }

    // Moves the streaks to another plane shape's wingtips (right tip given, left is mirrored).
    public void SetWingtips(Vector3 rightTip)
    {
        trails[0].transform.localPosition = rightTip;
        trails[1].transform.localPosition = new Vector3(-rightTip.x, rightTip.y, rightTip.z);
        foreach (var trail in trails) trail.Clear();
    }

    void ApplyStyle(TrailRenderer trail, Gradient colors, float trailWidth, float time)
    {
        trail.time = time;
        trail.widthCurve = AnimationCurve.Linear(0f, trailWidth, 1f, 0f);
        if (colors == null)
        {
            colors = new Gradient();
            colors.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(alpha, 0f), new GradientAlphaKey(0f, 1f) });
        }
        trail.colorGradient = colors;
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

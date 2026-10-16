using UnityEngine;

// Endless street along +Z: asphalt with a dashed center line, raised sidewalks, and empty
// building lots on both sides. Segments behind the plane are moved to the front as it flies.
// Each segment has LotLeft / LotRight anchors where building models can be placed later.
public class StreetBuilder : MonoBehaviour
{
    public PaperPlaneController plane;
    public Material asphalt;
    public Material laneLine;
    public Material sidewalk;
    public Material lot;
    public float segmentLength = 20f;
    public int segmentCount = 14;
    public float roadWidth = 12f;
    public float sidewalkWidth = 4f;
    public float lotWidth = 20f;
    public float startZ = -30f;

    Transform[] segments;
    bool moved;

    void Start()
    {
        float centerX = plane != null ? plane.CorridorCenterX : 0f;
        segments = new Transform[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            segments[i] = BuildSegment(i);
            segments[i].position = new Vector3(centerX, 0f, startZ + i * segmentLength);
        }
    }

    Transform BuildSegment(int index)
    {
        var root = new GameObject($"StreetSegment_{index}").transform;
        root.SetParent(transform, false);

        float half = roadWidth * 0.5f;
        float sidewalkX = half + sidewalkWidth * 0.5f;
        float lotX = half + sidewalkWidth + lotWidth * 0.5f;
        float midZ = segmentLength * 0.5f;

        // Surfaces sit slightly above the big Ground plane to avoid z-fighting.
        Block("Asphalt", root, asphalt, new Vector3(0f, -0.03f, midZ), new Vector3(roadWidth, 0.1f, segmentLength), true);
        Block("LaneLine", root, laneLine, new Vector3(0f, 0.025f, midZ), new Vector3(0.25f, 0.01f, segmentLength * 0.4f), false);
        Block("SidewalkLeft", root, sidewalk, new Vector3(-sidewalkX, 0.1f, midZ), new Vector3(sidewalkWidth, 0.2f, segmentLength), true);
        Block("SidewalkRight", root, sidewalk, new Vector3(sidewalkX, 0.1f, midZ), new Vector3(sidewalkWidth, 0.2f, segmentLength), true);
        Block("LotLeftGround", root, lot, new Vector3(-lotX, -0.02f, midZ), new Vector3(lotWidth, 0.1f, segmentLength), true);
        Block("LotRightGround", root, lot, new Vector3(lotX, -0.02f, midZ), new Vector3(lotWidth, 0.1f, segmentLength), true);

        Anchor("LotLeft", root, new Vector3(-lotX, 0.03f, midZ));
        Anchor("LotRight", root, new Vector3(lotX, 0.03f, midZ));
        return root;
    }

    static void Block(string name, Transform parent, Material material, Vector3 center, Vector3 size, bool collider)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = center;
        go.transform.localScale = size;
        go.GetComponent<MeshRenderer>().sharedMaterial = material;
        if (!collider) Destroy(go.GetComponent<Collider>());
    }

    static void Anchor(string name, Transform parent, Vector3 position)
    {
        var anchor = new GameObject(name).transform;
        anchor.SetParent(parent, false);
        anchor.localPosition = position;
    }

    void LateUpdate()
    {
        if (plane == null || segments == null) return;

        // Back on the pad: put the street back where it started.
        if (!plane.IsFlying && !plane.HasLanded)
        {
            if (moved) ResetSegments();
            return;
        }

        float planeZ = plane.transform.position.z;
        float loopLength = segmentCount * segmentLength;
        foreach (var segment in segments)
        {
            // Keep one segment of margin behind the plane before recycling it to the front.
            if (planeZ > segment.position.z + 2f * segmentLength)
            {
                segment.position += Vector3.forward * loopLength;
                moved = true;
            }
        }
    }

    void ResetSegments()
    {
        float centerX = plane.CorridorCenterX;
        for (int i = 0; i < segments.Length; i++)
            segments[i].position = new Vector3(centerX, 0f, startZ + i * segmentLength);
        moved = false;
    }
}

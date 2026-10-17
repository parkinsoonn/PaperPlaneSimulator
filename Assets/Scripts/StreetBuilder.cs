using System.Collections.Generic;
using UnityEngine;

// Endless street that follows a StreetPath (straight runs and soft left/right bends): asphalt
// with dashed lane lines, raised sidewalks, building lots and grass on both sides. Each 20 m
// segment is rebuilt as a curved strip for the part of the path it covers; segments behind the
// plane are moved to the front as it flies. Each segment has LotLeft / LotRight anchors.
public class StreetBuilder : MonoBehaviour
{
    public PaperPlaneController plane;
    public Transform ground;             // big invisible landing collider; follows the plane
    public Material asphalt;
    public Material laneLine;
    public Material sidewalk;
    public Material lot;
    public Material field;               // grass beyond the lots (the flat Ground can't bend with the curved world)
    public float segmentLength = 20f;
    public int segmentCount = 14;
    public float roadWidth = 16f;
    public float laneWidth = 4f;         // dashed lane lines every laneWidth from the center
    public float sidewalkWidth = 4f;
    public float lotWidth = 20f;
    public float fieldWidth = 150f;
    public float startZ = -30f;
    public int curveSteps = 4;           // sub-steps per segment for the curved strips
    public float fieldBandWidth = 10f;   // the field is split across its width so the curved world bends it like the buildings on it
    public float groundSnap = 100f;

    [Header("Turns")]
    public int straightStart = 8;
    public Vector2Int straightRun = new Vector2Int(6, 16);
    public Vector2Int bendSegments = new Vector2Int(4, 6);
    public Vector2 bendAngle = new Vector2(30f, 60f);
    public float maxDeviation = 60f;

    public event System.Action StreetReset;                     // before segments go back to the start
    public event System.Action<Transform, int> SegmentPlaced;   // a segment was built, moved forward or reset

    public StreetPath Path { get; private set; }
    public float PlaneProgress { get; private set; }            // plane's distance along the street

    class Segment
    {
        public Transform root;
        public int index;
        public Mesh asphalt, sidewalkLeft, sidewalkRight, lotLeft, lotRight, fieldLeft, fieldRight;
        public readonly List<(Transform dash, float x, float length)> dashes = new List<(Transform, float, float)>();
        public Transform lotAnchorLeft, lotAnchorRight;
    }

    Segment[] segments;
    int planeHint;
    bool moved;

    void Awake()
    {
        float centerX = plane != null ? plane.transform.position.x : 0f;
        Path = new StreetPath(new Vector3(centerX, 0f, startZ), 0f, segmentLength, Random.Range(0, int.MaxValue))
        {
            straightStart = straightStart,
            straightRun = straightRun,
            bendSegments = bendSegments,
            bendAngle = bendAngle,
            maxDeviation = maxDeviation,
        };
        // Settings are read on Reset, so regenerate with them applied.
        Path.Reset(Random.Range(0, int.MaxValue));
    }

    void Start()
    {
        segments = new Segment[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            segments[i] = BuildSegment(i);
            Place(segments[i], i);
        }
    }

    Segment BuildSegment(int index)
    {
        var s = new Segment { root = new GameObject($"StreetSegment_{index}").transform };
        s.root.SetParent(transform, false);

        s.asphalt = Strip("Asphalt", s.root, asphalt);
        s.sidewalkLeft = Strip("SidewalkLeft", s.root, sidewalk);
        s.sidewalkRight = Strip("SidewalkRight", s.root, sidewalk);
        s.lotLeft = Strip("LotLeftGround", s.root, lot);
        s.lotRight = Strip("LotRightGround", s.root, lot);
        // Grass: visual only, the big Ground collider underneath catches landings.
        s.fieldLeft = Strip("FieldLeft", s.root, field);
        s.fieldRight = Strip("FieldRight", s.root, field);

        float half = roadWidth * 0.5f;
        s.dashes.Add((Block("LaneLine", s.root, laneLine, new Vector3(0.25f, 0.01f, segmentLength * 0.4f)), 0f, segmentLength * 0.4f));
        for (float x = laneWidth; laneWidth > 0f && x < half - 1f; x += laneWidth)
        {
            s.dashes.Add((Block("LaneLineLeft", s.root, laneLine, new Vector3(0.15f, 0.01f, segmentLength * 0.25f)), -x, segmentLength * 0.25f));
            s.dashes.Add((Block("LaneLineRight", s.root, laneLine, new Vector3(0.15f, 0.01f, segmentLength * 0.25f)), x, segmentLength * 0.25f));
        }

        s.lotAnchorLeft = new GameObject("LotLeft").transform;
        s.lotAnchorLeft.SetParent(s.root, false);
        s.lotAnchorRight = new GameObject("LotRight").transform;
        s.lotAnchorRight.SetParent(s.root, false);
        return s;
    }

    // Moves a segment to path segment `index` and rebuilds its curved strips.
    void Place(Segment s, int index)
    {
        s.index = index;
        s.root.SetPositionAndRotation(Path.SegmentStart(index), Quaternion.Euler(0f, Path.SegmentYaw(index), 0f));

        float half = roadWidth * 0.5f;
        float walk = half + sidewalkWidth;
        float lotEnd = walk + lotWidth;
        float fieldEnd = lotEnd + fieldWidth;

        // Surfaces sit slightly above the big Ground plane to avoid z-fighting.
        BuildStrip(s.asphalt, index, -half, half, 0.02f, 0f);
        BuildStrip(s.sidewalkLeft, index, -walk, -half, 0.2f, 1f);    // curb faces the road (+x side)
        BuildStrip(s.sidewalkRight, index, half, walk, 0.2f, -1f);    // curb faces the road (-x side)
        BuildStrip(s.lotLeft, index, -lotEnd, -walk, 0.03f, 0f);
        BuildStrip(s.lotRight, index, walk, lotEnd, 0.03f, 0f);
        int fieldBands = Mathf.Max(1, Mathf.CeilToInt(fieldWidth / Mathf.Max(1f, fieldBandWidth)));
        BuildStrip(s.fieldLeft, index, -fieldEnd, -lotEnd, 0f, 0f, fieldBands);
        BuildStrip(s.fieldRight, index, lotEnd, fieldEnd, 0f, 0f, fieldBands);

        float mid = segmentLength * 0.5f;
        foreach (var (dash, x, _) in s.dashes)
            SetOnArc(dash, index, mid, x, 0.025f);
        SetOnArc(s.lotAnchorLeft, index, mid, -(walk + lotWidth * 0.5f), 0.03f);
        SetOnArc(s.lotAnchorRight, index, mid, walk + lotWidth * 0.5f, 0.03f);

        SegmentPlaced?.Invoke(s.root, index);
    }

    void SetOnArc(Transform t, int index, float s, float x, float y)
    {
        Path.LocalPose(index, s, out var p, out var yaw);
        var rotation = Quaternion.Euler(0f, yaw, 0f);
        t.localPosition = p + rotation * new Vector3(x, y, 0f);
        t.localRotation = rotation;
    }

    // Flat strip between lateral offsets x0..x1 at height y along the segment's arc. curbSide != 0
    // adds a vertical face from the ground up to y on the edge facing that x direction. bands > 1 splits
    // the strip across its width (the curved world bends per vertex, so wide strips need inner vertices).
    void BuildStrip(Mesh mesh, int index, float x0, float x1, float y, float curbSide, int bands = 1)
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        int steps = Mathf.Max(1, curveSteps);
        for (int k = 0; k < steps; k++)
        {
            Path.LocalPose(index, segmentLength * k / steps, out var p0, out var yaw0);
            Path.LocalPose(index, segmentLength * (k + 1) / steps, out var p1, out var yaw1);
            Vector3 r0 = Quaternion.Euler(0f, yaw0, 0f) * Vector3.right;
            Vector3 r1 = Quaternion.Euler(0f, yaw1, 0f) * Vector3.right;
            Vector3 up = Vector3.up * y;

            for (int b = 0; b < bands; b++)
            {
                float xa = Mathf.Lerp(x0, x1, (float)b / bands), xb = Mathf.Lerp(x0, x1, (float)(b + 1) / bands);
                AddQuad(vertices, triangles, p0 + r0 * xa + up, p0 + r0 * xb + up, p1 + r1 * xb + up, p1 + r1 * xa + up, Vector3.up);
            }
            if (curbSide != 0f)
            {
                float xe = curbSide > 0f ? x1 : x0;
                Vector3 a = p0 + r0 * xe, b = p1 + r1 * xe;
                AddQuad(vertices, triangles, a, a + up, b + up, b, r0 * curbSide);
            }
        }
        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    // Adds quad a-b-c-d (in order around its edge), wound so its front faces `facing`.
    static void AddQuad(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 facing)
    {
        if (Vector3.Dot(Vector3.Cross(b - a, c - a), facing) < 0f) (b, d) = (d, b);
        int i = vertices.Count;
        vertices.Add(a); vertices.Add(b); vertices.Add(c); vertices.Add(d);
        triangles.Add(i); triangles.Add(i + 1); triangles.Add(i + 2);
        triangles.Add(i); triangles.Add(i + 2); triangles.Add(i + 3);
    }

    static Mesh Strip(string name, Transform parent, Material material)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var mesh = new Mesh { name = name };
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = material;
        return mesh;
    }

    static Transform Block(string name, Transform parent, Material material, Vector3 size)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localScale = size;
        go.GetComponent<MeshRenderer>().sharedMaterial = material;
        Destroy(go.GetComponent<Collider>());
        return go.transform;
    }

    void LateUpdate()
    {
        if (plane == null || segments == null) return;

        Vector3 planePos = plane.transform.position;
        if (ground != null)
        {
            // Keep the landing collider under the plane wherever the street wanders.
            var snapped = new Vector3(Mathf.Round(planePos.x / groundSnap) * groundSnap, ground.position.y,
                Mathf.Round(planePos.z / groundSnap) * groundSnap);
            if ((ground.position - snapped).sqrMagnitude > 0.01f) ground.position = snapped;
        }

        // Back on the pad: new street layout from the start.
        if (!plane.IsFlying && !plane.HasLanded)
        {
            if (moved) ResetSegments();
            PlaneProgress = Path.Progress(planePos, ref planeHint);
            return;
        }

        PlaneProgress = Path.Progress(planePos, ref planeHint);
        int planeSegment = Mathf.FloorToInt(PlaneProgress / segmentLength);
        foreach (var segment in segments)
        {
            // Keep one segment of margin behind the plane before recycling it to the front.
            if (segment.index < planeSegment - 1)
            {
                Place(segment, segment.index + segmentCount);
                moved = true;
            }
        }
    }

    void ResetSegments()
    {
        StreetReset?.Invoke();
        Path.Reset(Random.Range(0, int.MaxValue));
        planeHint = 0;
        for (int i = 0; i < segments.Length; i++) Place(segments[i], i);
        moved = false;
    }
}

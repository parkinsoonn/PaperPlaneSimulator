using System.Collections.Generic;
using UnityEngine;

// Scatters speed stars over the street ahead of the plane during a throw.
// The bigger the boost, the rarer the star.
public class StarSpawner : MonoBehaviour
{
    [System.Serializable]
    public class StarType
    {
        public string name;
        public Color color;
        public float weight;   // relative chance
        public float boost;    // m/s added on pickup
        public int gold;       // shop gold earned on pickup
    }

    public PaperPlaneController plane;
    public StreetBuilder street;         // stars are placed along the street's path
    public Material starMaterial;
    public Material glowMaterial;        // Custom/CurvedGlow halo drawn around each star
    public float glowIntensity = 1.6f;   // HDR boost on the star color so bloom picks it up
    public float glowSize = 6f;          // halo size relative to the star
    public float spacing = 35f;          // meters between stars along the street
    public float spawnAhead = 120f;
    public float despawnBehind = 20f;
    public float starSize = 0.6f;
    public float halfWidth = 11f;        // stars stay over the street
    public float openingDistance = 150f; // stars this close to the launch point are placed higher,
    public float openingHeight = 12f;    // since the plane climbs right after the throw
    public StarType[] types =
    {
        new StarType { name = "Blue",   color = new Color(0.25f, 0.55f, 1f),  weight = 78f, boost = 6f,  gold = 1 },
        new StarType { name = "Yellow", color = new Color(1f, 0.85f, 0.15f),  weight = 18f, boost = 12f, gold = 3 },
        new StarType { name = "Red",    color = new Color(1f, 0.2f, 0.15f),   weight = 4f,  boost = 24f, gold = 6 },
    };

    readonly List<(GameObject star, float s)> stars = new List<(GameObject, float)>();
    Mesh starMesh;
    Mesh glowMesh;
    float nextS;     // distances along the street
    float launchS;
    bool active;

    void Awake()
    {
        starMesh = BuildStarMesh(1f, 0.45f, 0.3f);
        glowMesh = BuildQuad();
    }

    void Update()
    {
        if (plane == null || street == null || street.Path == null) return;

        // Back on the pad: clear the previous throw's stars.
        if (!plane.IsFlying && !plane.HasLanded)
        {
            if (active) Clear();
            return;
        }
        if (!plane.IsFlying) return;

        float s = street.PlaneProgress;
        if (!active)
        {
            active = true;
            launchS = s;
            nextS = s + spacing * 1.5f;
        }

        while (nextS < s + spawnAhead)
        {
            Spawn(nextS, plane.transform.position.y);
            nextS += spacing;
        }

        for (int i = stars.Count - 1; i >= 0; i--)
        {
            if (stars[i].star == null) stars.RemoveAt(i);   // already collected
            else if (stars[i].s < s - despawnBehind)
            {
                Destroy(stars[i].star);
                stars.RemoveAt(i);
            }
        }
    }

    void Spawn(float s, float planeHeight)
    {
        StarType type = PickType();
        var pose = street.Path.GetPose(s);
        float side = Random.Range(-halfWidth, halfWidth);
        float baseHeight = s - launchS < openingDistance ? Mathf.Max(planeHeight, openingHeight) : planeHeight;
        float y = Mathf.Clamp(baseHeight + Random.Range(-4f, 2f), 1.5f, 25f);

        var go = new GameObject($"Star_{type.name}");
        go.transform.SetParent(transform, false);
        go.transform.position = pose.position + pose.right * side + Vector3.up * y;
        go.transform.localScale = Vector3.one * starSize;
        go.AddComponent<MeshFilter>().sharedMesh = starMesh;
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = starMaterial;
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", type.color * glowIntensity);
        renderer.SetPropertyBlock(block);
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Transform glow = null;
        if (glowMaterial != null)
        {
            glow = new GameObject("Glow").transform;
            glow.SetParent(go.transform, false);
            glow.localScale = Vector3.one * glowSize;
            glow.gameObject.AddComponent<MeshFilter>().sharedMesh = glowMesh;
            var glowRenderer = glow.gameObject.AddComponent<MeshRenderer>();
            glowRenderer.sharedMaterial = glowMaterial;
            glowRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var glowBlock = new MaterialPropertyBlock();
            glowBlock.SetColor("_BaseColor", type.color);
            glowRenderer.SetPropertyBlock(glowBlock);
        }

        var trigger = go.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 1.6f;   // a bit generous so stars are easy to grab
        var pickup = go.AddComponent<StarPickup>();
        pickup.boost = type.boost;
        pickup.gold = type.gold;
        pickup.sound = "star_" + type.name.ToLowerInvariant();
        pickup.glow = glow;
        stars.Add((go, s));
    }

    // Unit quad in the xy plane; the glow shader turns it to face the camera.
    static Mesh BuildQuad()
    {
        var mesh = new Mesh { name = "GlowQuad" };
        mesh.SetVertices(new[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f), new Vector3(0.5f, 0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f) });
        mesh.SetUVs(0, new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f) });
        mesh.SetTriangles(new[] { 0, 2, 1, 0, 3, 2 }, 0);
        // Billboarded in the shader, so give it generous bounds to avoid culling.
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 2f);
        return mesh;
    }

    StarType PickType()
    {
        float total = 0f;
        foreach (var t in types) total += t.weight;
        float r = Random.value * total;
        foreach (var t in types)
        {
            if (r < t.weight) return t;
            r -= t.weight;
        }
        return types[types.Length - 1];
    }

    void Clear()
    {
        foreach (var (star, _) in stars)
            if (star != null) Destroy(star);
        stars.Clear();
        active = false;
    }

    // Flat 5-pointed star, extruded along Z, facing the plane.
    static Mesh BuildStarMesh(float outer, float inner, float depth)
    {
        const int points = 5;
        int rim = points * 2;
        var verts = new List<Vector3>();
        var tris = new List<int>();
        float half = depth * 0.5f;

        Vector3 RimPoint(int i)
        {
            float a = Mathf.PI * 0.5f + i * Mathf.PI / points;
            float r = i % 2 == 0 ? outer : inner;
            return new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f);
        }

        // Front and back faces: center fan.
        for (int side = 0; side < 2; side++)
        {
            float z = side == 0 ? -half : half;
            int center = verts.Count;
            verts.Add(new Vector3(0f, 0f, z));
            for (int i = 0; i < rim; i++) verts.Add(RimPoint(i) + new Vector3(0f, 0f, z));
            for (int i = 0; i < rim; i++)
            {
                int a = center + 1 + i, b = center + 1 + (i + 1) % rim;
                if (side == 0) tris.AddRange(new[] { center, b, a });
                else tris.AddRange(new[] { center, a, b });
            }
        }

        // Side walls, one quad per edge (own vertices for flat shading).
        for (int i = 0; i < rim; i++)
        {
            Vector3 p0 = RimPoint(i), p1 = RimPoint((i + 1) % rim);
            int s = verts.Count;
            verts.Add(p0 + Vector3.back * half);
            verts.Add(p1 + Vector3.back * half);
            verts.Add(p1 + Vector3.forward * half);
            verts.Add(p0 + Vector3.forward * half);
            tris.AddRange(new[] { s, s + 1, s + 2, s, s + 2, s + 3 });
        }

        var mesh = new Mesh { name = "Star" };
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}

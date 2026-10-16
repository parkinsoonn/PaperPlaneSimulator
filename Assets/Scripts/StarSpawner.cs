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
    }

    public PaperPlaneController plane;
    public Material starMaterial;
    public float spacing = 25f;          // meters between stars along the street
    public float spawnAhead = 120f;
    public float despawnBehind = 20f;
    public float starSize = 0.6f;
    public float halfWidth = 9f;         // stars stay over the street
    public float openingDistance = 150f; // stars this close to the launch point are placed higher,
    public float openingHeight = 12f;    // since the plane climbs right after the throw
    public StarType[] types =
    {
        new StarType { name = "Blue",   color = new Color(0.25f, 0.55f, 1f),  weight = 78f, boost = 3f },
        new StarType { name = "Yellow", color = new Color(1f, 0.85f, 0.15f),  weight = 18f, boost = 6f },
        new StarType { name = "Red",    color = new Color(1f, 0.2f, 0.15f),   weight = 4f,  boost = 12f },
    };

    readonly List<GameObject> stars = new List<GameObject>();
    Mesh starMesh;
    float nextZ;
    float launchZ;
    bool active;

    void Awake()
    {
        starMesh = BuildStarMesh(1f, 0.45f, 0.3f);
    }

    void Update()
    {
        if (plane == null) return;

        // Back on the pad: clear the previous throw's stars.
        if (!plane.IsFlying && !plane.HasLanded)
        {
            if (active) Clear();
            return;
        }
        if (!plane.IsFlying) return;

        Vector3 p = plane.transform.position;
        if (!active)
        {
            active = true;
            launchZ = p.z;
            nextZ = p.z + spacing * 1.5f;
        }

        while (nextZ < p.z + spawnAhead)
        {
            Spawn(nextZ, p.y);
            nextZ += spacing;
        }

        for (int i = stars.Count - 1; i >= 0; i--)
        {
            if (stars[i] == null) stars.RemoveAt(i);   // already collected
            else if (stars[i].transform.position.z < p.z - despawnBehind)
            {
                Destroy(stars[i]);
                stars.RemoveAt(i);
            }
        }
    }

    void Spawn(float z, float planeHeight)
    {
        StarType type = PickType();
        float x = plane.CorridorCenterX + Random.Range(-halfWidth, halfWidth);
        float baseHeight = z - launchZ < openingDistance ? Mathf.Max(planeHeight, openingHeight) : planeHeight;
        float y = Mathf.Clamp(baseHeight + Random.Range(-4f, 2f), 1.5f, 25f);

        var go = new GameObject($"Star_{type.name}");
        go.transform.SetParent(transform, false);
        go.transform.position = new Vector3(x, y, z);
        go.transform.localScale = Vector3.one * starSize;
        go.AddComponent<MeshFilter>().sharedMesh = starMesh;
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = starMaterial;
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", type.color);
        renderer.SetPropertyBlock(block);
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        var trigger = go.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 1.6f;   // a bit generous so stars are easy to grab
        go.AddComponent<StarPickup>().boost = type.boost;
        stars.Add(go);
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
        foreach (var s in stars)
            if (s != null) Destroy(s);
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

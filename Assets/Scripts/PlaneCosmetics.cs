using System.Collections.Generic;
using UnityEngine;

// Shows the equipped shop cosmetics on the plane: paper color, shape and wind trail. Looks only;
// the collider and flight physics stay the same for every shape.
[RequireComponent(typeof(WindTrail))]
public class PlaneCosmetics : MonoBehaviour
{
    public Transform model;           // the classic paper_plane model
    public Material paperMaterial;    // tinted with the paper color
    public Material shadeMaterial;    // tinted darker (folds and underside)
    public Material accentMaterial;   // red details, never tinted
    public float shadeRatio = 0.625f; // Plane_paper_shade / Plane_paper brightness

    const string ShapeFolder = "Planes/";   // Assets/Resources/Planes/<id>.json

    WindTrail trail;
    Vector3 classicWingtip;
    readonly Dictionary<string, (GameObject go, Vector3 wingtip)> shapeModels = new Dictionary<string, (GameObject, Vector3)>();

    // True when the shape's JSON exists, so the shop only lists shapes that can be shown.
    public static bool HasShape(string shapeId) =>
        string.IsNullOrEmpty(shapeId) || Resources.Load<TextAsset>(ShapeFolder + shapeId) != null;

    void Awake()
    {
        trail = GetComponent<WindTrail>();
        classicWingtip = trail.wingtipOffset;
    }

    void OnEnable() => PlayerWallet.Changed += Apply;
    void OnDisable() => PlayerWallet.Changed -= Apply;
    void Start() => Apply();

    void Apply()
    {
        ApplyShape(PlayerWallet.Equipped(CosmeticCategory.Shape).shapeId);
        ApplyColor(PlayerWallet.Equipped(CosmeticCategory.Color).color);
        var effect = PlayerWallet.Equipped(CosmeticCategory.Effect);
        if (effect.trailColors == null) trail.SetStyle(null, trail.width, trail.trailTime);
        else trail.SetStyle(effect.trailColors, effect.trailWidth, effect.trailTime);
    }

    // Empty id (or a shape whose JSON is missing) shows the classic model.
    void ApplyShape(string shapeId)
    {
        if (!string.IsNullOrEmpty(shapeId) && !shapeModels.ContainsKey(shapeId))
        {
            var created = CreateShapeModel(shapeId);
            if (created.go != null) shapeModels[shapeId] = created;
        }
        bool custom = !string.IsNullOrEmpty(shapeId) && shapeModels.ContainsKey(shapeId);
        if (model != null) model.gameObject.SetActive(!custom);
        foreach (var pair in shapeModels) pair.Value.go.SetActive(custom && pair.Key == shapeId);
        trail.SetWingtips(custom ? shapeModels[shapeId].wingtip : classicWingtip);
    }

    void ApplyColor(Color color)
    {
        var block = new MaterialPropertyBlock();
        foreach (var r in GetComponentsInChildren<Renderer>(true))
        {
            var materials = r.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != paperMaterial && materials[i] != shadeMaterial) continue;
                block.Clear();
                block.SetColor("_BaseColor", materials[i] == paperMaterial ? color : color * shadeRatio);
                r.SetPropertyBlock(block, i);
            }
        }
    }

    (GameObject go, Vector3 wingtip) CreateShapeModel(string shapeId)
    {
        var asset = Resources.Load<TextAsset>(ShapeFolder + shapeId);
        var data = asset != null ? JsonUtility.FromJson<PlaneShapeData>(asset.text) : null;
        if (data == null || data.panels == null) return (null, Vector3.zero);

        var go = new GameObject("ShapeModel_" + shapeId);
        go.transform.SetParent(transform, false);
        go.AddComponent<MeshFilter>().sharedMesh = BuildMesh(data);
        go.AddComponent<MeshRenderer>().sharedMaterials = new[] { paperMaterial, shadeMaterial, accentMaterial };
        var tip = data.wingtip != null && data.wingtip.Length >= 3
            ? new Vector3(data.wingtip[0], data.wingtip[1], data.wingtip[2])
            : classicWingtip;
        return (go, tip);
    }

    // Each panel becomes a thin slab: top and bottom faces plus side walls, flat shaded.
    // Submeshes: 0 = paper, 1 = shade, 2 = accent.
    static Mesh BuildMesh(PlaneShapeData data)
    {
        var verts = new List<Vector3>();
        var normals = new List<Vector3>();
        var tris = new[] { new List<int>(), new List<int>(), new List<int>() };

        foreach (var panel in data.panels)
        {
            if (panel.points == null || panel.points.Length < 9) continue;
            int count = Mathf.Min(panel.points.Length / 3, 4);
            var points = new Vector3[count];
            for (int i = 0; i < count; i++)
                points[i] = new Vector3(panel.points[i * 3], panel.points[i * 3 + 1], panel.points[i * 3 + 2]);
            int sub = panel.mat == "shade" ? 1 : panel.mat == "accent" ? 2 : 0;

            AddSlab(points, panel.thickness, verts, normals, tris[sub]);
            if (panel.mirror)
            {
                for (int i = 0; i < count; i++) points[i].x = -points[i].x;
                AddSlab(points, panel.thickness, verts, normals, tris[sub]);
            }
        }

        var mesh = new Mesh { name = data.id };
        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.subMeshCount = 3;
        for (int i = 0; i < 3; i++) mesh.SetTriangles(tris[i], i);
        mesh.RecalculateBounds();
        return mesh;
    }

    static void AddSlab(Vector3[] points, float thickness, List<Vector3> verts, List<Vector3> normals, List<int> tris)
    {
        Vector3 n = Vector3.Cross(points[1] - points[0], points[2] - points[0]).normalized;
        if (n == Vector3.zero) return;
        Vector3 half = n * Mathf.Max(thickness, 0.001f) * 0.5f;
        int count = points.Length;
        var top = new Vector3[count];
        var bottom = new Vector3[count];
        Vector3 center = Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            top[i] = points[i] + half;
            bottom[i] = points[i] - half;
            center += points[i] / count;
        }

        AddFace(top, n, verts, normals, tris);
        AddFace(bottom, -n, verts, normals, tris);
        for (int i = 0; i < count; i++)
        {
            int j = (i + 1) % count;
            Vector3 mid = (points[i] + points[j]) * 0.5f;
            Vector3 outward = Vector3.ProjectOnPlane(mid - center, n).normalized;
            AddFace(new[] { top[i], top[j], bottom[j], bottom[i] }, outward, verts, normals, tris);
        }
    }

    // Fan-triangulates a convex face, wound so it faces `normal`.
    static void AddFace(Vector3[] face, Vector3 normal, List<Vector3> verts, List<Vector3> normals, List<int> tris)
    {
        int start = verts.Count;
        foreach (var p in face)
        {
            verts.Add(p);
            normals.Add(normal);
        }
        bool flip = Vector3.Dot(Vector3.Cross(face[1] - face[0], face[2] - face[0]), normal) < 0f;
        for (int i = 1; i < face.Length - 1; i++)
        {
            if (flip) tris.AddRange(new[] { start, start + i + 1, start + i });
            else tris.AddRange(new[] { start, start + i, start + i + 1 });
        }
    }
}

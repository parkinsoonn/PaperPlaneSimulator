using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Turns a list of PartData into one flat-shaded mesh with the part colors baked into vertex
// colors, so a whole building draws with a single material (Custom/CurvedWorld + vertex color).
public static class CityMeshBuilder
{
    // Unit shapes fit the -0.5..0.5 box; a part's size scales them directly.
    struct Shape
    {
        public Vector3[] vertices;
        public Vector3[] normals;
    }

    static readonly Dictionary<string, Shape> shapes = new Dictionary<string, Shape>();

    public static Mesh Build(PartData[] parts, string name)
    {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var colors = new List<Color32>();

        if (parts != null)
        {
            foreach (var part in parts)
            {
                Vector3 pos = ToVector(part.pos, Vector3.zero);
                Vector3 size = ToVector(part.size, Vector3.one);
                Quaternion rot = Quaternion.Euler(ToVector(part.rot, Vector3.zero));
                Color32 color = ParseColor(part.color);

                if (part.type == "windows")
                    AddWindows(part, Matrix4x4.TRS(pos, rot, Vector3.one), size, color, vertices, normals, colors);
                else if (part.type == "facade")
                    AddFacade(part, pos, size, rot, color, vertices, normals, colors);
                else
                    AddShape(GetShape(part.type), Matrix4x4.TRS(pos, rot, size), color, vertices, normals, colors);
            }
        }

        var mesh = new Mesh { name = name };
        if (vertices.Count > 65000) mesh.indexFormat = IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetColors(colors);
        var triangles = new int[vertices.Count];
        for (int i = 0; i < triangles.Length; i++) triangles[i] = i;
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    public static Color ParseColor(string hex)
    {
        return !string.IsNullOrEmpty(hex) && ColorUtility.TryParseHtmlString(hex, out var c) ? c : new Color(0.6f, 0.6f, 0.6f);
    }

    static Vector3 ToVector(float[] v, Vector3 fallback)
    {
        return v != null && v.Length >= 3 ? new Vector3(v[0], v[1], v[2]) : fallback;
    }

    // Window grids on all four vertical sides of a building body. size = body depth (x),
    // window band height (y), frontage (z); grids keep 1 m clear of each corner.
    static void AddFacade(PartData part, Vector3 pos, Vector3 size, Quaternion rot, Color32 color,
        List<Vector3> vertices, List<Vector3> normals, List<Color32> colors)
    {
        var body = Matrix4x4.TRS(pos, rot, Vector3.one);
        float dx = size.x * 0.5f + part.offset;
        float dz = size.z * 0.5f + part.offset;
        var alongZ = new Vector3(part.thickness, size.y, Mathf.Max(0f, size.z - 2f));
        var alongX = new Vector3(part.thickness, size.y, Mathf.Max(0f, size.x - 2f));
        var turn = Quaternion.Euler(0f, 90f, 0f);
        AddWindows(part, body * Matrix4x4.TRS(new Vector3(-dx, 0f, 0f), Quaternion.identity, Vector3.one), alongZ, color, vertices, normals, colors);
        AddWindows(part, body * Matrix4x4.TRS(new Vector3(dx, 0f, 0f), Quaternion.identity, Vector3.one), alongZ, color, vertices, normals, colors);
        AddWindows(part, body * Matrix4x4.TRS(new Vector3(0f, 0f, -dz), turn, Vector3.one), alongX, color, vertices, normals, colors);
        AddWindows(part, body * Matrix4x4.TRS(new Vector3(0f, 0f, dz), turn, Vector3.one), alongX, color, vertices, normals, colors);
    }

    // Grid of small boxes in the grid's local y/z plane (thin along x), e.g. windows on one wall.
    static void AddWindows(PartData part, Matrix4x4 grid, Vector3 size, Color32 color,
        List<Vector3> vertices, List<Vector3> normals, List<Color32> colors)
    {
        float rowStep = part.spacing != null && part.spacing.Length > 0 ? part.spacing[0] : 3f;
        float colStep = part.spacing != null && part.spacing.Length > 1 ? part.spacing[1] : 2.5f;
        float cellHeight = part.cell != null && part.cell.Length > 0 ? part.cell[0] : rowStep * 0.55f;
        // A cell width of 0 means "the whole grid width" (e.g. floor lines on every face of a facade).
        float cellWidth = part.cell != null && part.cell.Length > 1
            ? (part.cell[1] > 0f ? part.cell[1] : size.z)
            : colStep * 0.5f;
        int rows = Mathf.Max(1, Mathf.FloorToInt(size.y / rowStep + 0.001f));
        int cols = Mathf.Max(1, Mathf.FloorToInt(size.z / colStep + 0.001f));

        var box = GetShape("box");
        var cellSize = new Vector3(size.x, cellHeight, cellWidth);
        for (int r = 0; r < rows; r++)
        {
            float y = (r - (rows - 1) * 0.5f) * rowStep;
            for (int c = 0; c < cols; c++)
            {
                float z = (c - (cols - 1) * 0.5f) * colStep;
                var local = Matrix4x4.TRS(new Vector3(0f, y, z), Quaternion.identity, cellSize);
                AddShape(box, grid * local, color, vertices, normals, colors);
            }
        }
    }

    static void AddShape(Shape shape, Matrix4x4 matrix, Color32 color,
        List<Vector3> vertices, List<Vector3> normals, List<Color32> colors)
    {
        Matrix4x4 normalMatrix = matrix.inverse.transpose;
        for (int i = 0; i < shape.vertices.Length; i++)
        {
            vertices.Add(matrix.MultiplyPoint3x4(shape.vertices[i]));
            normals.Add(normalMatrix.MultiplyVector(shape.normals[i]).normalized);
            colors.Add(color);
        }
    }

    static Shape GetShape(string type)
    {
        if (string.IsNullOrEmpty(type)) type = "box";
        if (shapes.TryGetValue(type, out var shape)) return shape;

        var builder = new ShapeBuilder();
        switch (type)
        {
            case "cylinder": builder.Cylinder(12); break;
            case "cone": builder.Cone(12); break;
            case "wedge": builder.Wedge(); break;
            case "dome": builder.Dome(12, 4); break;
            default: builder.Box(); break;
        }
        shape = new Shape { vertices = builder.vertices.ToArray(), normals = builder.normals.ToArray() };
        shapes[type] = shape;
        return shape;
    }

    // Builds flat-shaded triangles; every triangle is wound to face away from an interior point,
    // so the shapes below only need their corners, not hand-checked winding.
    class ShapeBuilder
    {
        public readonly List<Vector3> vertices = new List<Vector3>();
        public readonly List<Vector3> normals = new List<Vector3>();
        Vector3 interior;

        void Tri(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (n.sqrMagnitude < 1e-10f) return;
            if (Vector3.Dot(n, (a + b + c) / 3f - interior) < 0f)
            {
                (b, c) = (c, b);
                n = -n;
            }
            n.Normalize();
            vertices.Add(a); vertices.Add(b); vertices.Add(c);
            normals.Add(n); normals.Add(n); normals.Add(n);
        }

        void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            Tri(a, b, c);
            Tri(a, c, d);
        }

        static Vector3 Ring(int i, int sides, float radius, float y)
        {
            float angle = i * Mathf.PI * 2f / sides;
            return new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius);
        }

        public void Box()
        {
            interior = Vector3.zero;
            float h = 0.5f;
            var p = new Vector3[8];
            for (int i = 0; i < 8; i++)
                p[i] = new Vector3((i & 1) == 0 ? -h : h, (i & 2) == 0 ? -h : h, (i & 4) == 0 ? -h : h);
            Quad(p[0], p[1], p[3], p[2]);   // -z
            Quad(p[4], p[5], p[7], p[6]);   // +z
            Quad(p[0], p[2], p[6], p[4]);   // -x
            Quad(p[1], p[3], p[7], p[5]);   // +x
            Quad(p[0], p[1], p[5], p[4]);   // -y
            Quad(p[2], p[3], p[7], p[6]);   // +y
        }

        public void Cylinder(int sides)
        {
            interior = Vector3.zero;
            var top = new Vector3(0f, 0.5f, 0f);
            var bottom = new Vector3(0f, -0.5f, 0f);
            for (int i = 0; i < sides; i++)
            {
                Vector3 b0 = Ring(i, sides, 0.5f, -0.5f), b1 = Ring(i + 1, sides, 0.5f, -0.5f);
                Vector3 t0 = Ring(i, sides, 0.5f, 0.5f), t1 = Ring(i + 1, sides, 0.5f, 0.5f);
                Quad(b0, b1, t1, t0);
                Tri(top, t0, t1);
                Tri(bottom, b0, b1);
            }
        }

        public void Cone(int sides)
        {
            interior = new Vector3(0f, -0.25f, 0f);
            var apex = new Vector3(0f, 0.5f, 0f);
            var bottom = new Vector3(0f, -0.5f, 0f);
            for (int i = 0; i < sides; i++)
            {
                Vector3 b0 = Ring(i, sides, 0.5f, -0.5f), b1 = Ring(i + 1, sides, 0.5f, -0.5f);
                Tri(apex, b0, b1);
                Tri(bottom, b0, b1);
            }
        }

        // Triangular prism: ridge along z at the top, full width at the bottom (gable roof).
        public void Wedge()
        {
            interior = new Vector3(0f, -0.2f, 0f);
            Vector3 l0 = new Vector3(-0.5f, -0.5f, -0.5f), r0 = new Vector3(0.5f, -0.5f, -0.5f), t0 = new Vector3(0f, 0.5f, -0.5f);
            Vector3 l1 = new Vector3(-0.5f, -0.5f, 0.5f), r1 = new Vector3(0.5f, -0.5f, 0.5f), t1 = new Vector3(0f, 0.5f, 0.5f);
            Tri(l0, r0, t0);
            Tri(l1, r1, t1);
            Quad(l0, r0, r1, l1);
            Quad(l0, t0, t1, l1);
            Quad(r0, t0, t1, r1);
        }

        // Half-ellipsoid: flat base at y = -0.5, top at y = +0.5.
        public void Dome(int sides, int rings)
        {
            interior = new Vector3(0f, -0.3f, 0f);
            var bottom = new Vector3(0f, -0.5f, 0f);
            for (int j = 0; j < rings; j++)
            {
                float a0 = j * Mathf.PI * 0.5f / rings, a1 = (j + 1) * Mathf.PI * 0.5f / rings;
                float y0 = -0.5f + Mathf.Sin(a0), y1 = -0.5f + Mathf.Sin(a1);
                float r0 = 0.5f * Mathf.Cos(a0), r1 = 0.5f * Mathf.Cos(a1);
                for (int i = 0; i < sides; i++)
                {
                    Vector3 p00 = Ring(i, sides, r0, y0), p01 = Ring(i + 1, sides, r0, y0);
                    Vector3 p10 = Ring(i, sides, r1, y1), p11 = Ring(i + 1, sides, r1, y1);
                    if (j == rings - 1) Tri(p00, p01, p10);   // top ring collapses to a point
                    else Quad(p00, p01, p11, p10);
                    if (j == 0) Tri(bottom, p00, p01);
                }
            }
        }
    }
}

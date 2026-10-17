using System.Collections.Generic;
using UnityEngine;

// A point on the street's center line.
public struct PathPose
{
    public Vector3 position;
    public float yaw;          // degrees, 0 = +Z
    public Vector3 forward;
    public Vector3 right;
}

// The street's center line: a chain of 20 m segments, each one a circular arc (straight when its
// bend is 0), so consecutive segments join without gaps. Straight runs alternate with soft
// left/right bends chosen from a seed; the heading never strays more than maxDeviation from the
// start, so the street can't loop back onto itself. Segments are generated on demand.
public class StreetPath
{
    public readonly float segmentLength;
    public int straightStart = 8;                       // segments kept straight around the launch pad
    public Vector2Int straightRun = new Vector2Int(6, 16);
    public Vector2Int bendSegments = new Vector2Int(4, 6);
    public Vector2 bendAngle = new Vector2(30f, 60f);   // total turn of one bend, degrees
    public float maxDeviation = 60f;

    readonly Vector3 origin;
    readonly float originYaw;
    readonly List<Vector3> starts = new List<Vector3>();
    readonly List<float> yaws = new List<float>();
    readonly List<float> bends = new List<float>();
    System.Random rng;
    int straightLeft, bendLeft;
    float bendStep;

    public StreetPath(Vector3 origin, float originYaw, float segmentLength, int seed)
    {
        this.origin = origin;
        this.originYaw = originYaw;
        this.segmentLength = segmentLength;
        Reset(seed);
    }

    public void Reset(int seed)
    {
        rng = new System.Random(seed);
        starts.Clear();
        yaws.Clear();
        bends.Clear();
        starts.Add(origin);
        yaws.Add(originYaw);
        straightLeft = straightStart;
        bendLeft = 0;
    }

    public Vector3 SegmentStart(int i) { Ensure(i); return starts[Mathf.Max(0, i)]; }
    public float SegmentYaw(int i) { Ensure(i); return yaws[Mathf.Max(0, i)]; }
    public float Bend(int i) { Ensure(i); return i < 0 ? 0f : bends[i]; }

    // Pose inside segment i, in that segment's local frame (start at origin, heading +Z).
    public void LocalPose(int i, float s, out Vector3 position, out float yaw)
    {
        float bend = Bend(i);
        position = ArcPoint(bend, s, segmentLength);
        yaw = bend * s / segmentLength;
    }

    // World pose at distance s along the street (s < 0 extends straight back from the start).
    public PathPose GetPose(float s)
    {
        int i = Mathf.Max(0, Mathf.FloorToInt(s / segmentLength));
        Ensure(i);
        float local = s - i * segmentLength;
        LocalPose(i, local, out var p, out var localYaw);
        float yaw = yaws[i] + localYaw;
        var rotation = Quaternion.Euler(0f, yaws[i], 0f);
        var heading = Quaternion.Euler(0f, yaw, 0f);
        return new PathPose
        {
            position = starts[i] + rotation * p,
            yaw = yaw,
            forward = heading * Vector3.forward,
            right = heading * Vector3.right,
        };
    }

    // Distance along the street of the point nearest to p. hint is the segment searched first
    // (pass the previous result's segment back in to keep this cheap).
    public float Progress(Vector3 p, ref int hint)
    {
        int i = Mathf.Max(0, hint);
        int lastMove = 0, previous = i;
        for (int step = 0; step < 64; step++)
        {
            float t = LocalProgress(i, p);
            bool ahead = t >= segmentLength, behind = t < 0f && i > 0;
            // Bouncing between two neighbors means p sits on their joint: settle on this segment.
            bool bounced = (ahead && lastMove < 0) || (behind && lastMove > 0);
            if (bounced && Mathf.Abs(i - previous) <= 1)
            {
                hint = i;
                return i * segmentLength + Mathf.Clamp(t, 0f, segmentLength);
            }
            previous = i;
            // Far from the hint (e.g. after a teleport) jump several segments at once.
            if (ahead) { i += Mathf.Clamp(Mathf.FloorToInt(t / segmentLength), 1, 50); lastMove = 1; continue; }
            if (behind) { i = Mathf.Max(0, i - Mathf.Clamp(Mathf.CeilToInt(-t / segmentLength), 1, 50)); lastMove = -1; continue; }
            hint = i;
            return i * segmentLength + t;
        }
        hint = i;
        return i * segmentLength;
    }

    // Lateral offset of p from the center line at distance s (positive = right of the street).
    public float LateralOffset(Vector3 p, float s)
    {
        var pose = GetPose(s);
        return Vector3.Dot(p - pose.position, pose.right);
    }

    float LocalProgress(int i, Vector3 p)
    {
        Ensure(i);
        var rotation = Quaternion.Euler(0f, yaws[i], 0f);
        float t = Vector3.Dot(p - starts[i], rotation * Vector3.forward);
        // Far outside this segment the straight projection is enough to pick the next one to try.
        if (t < -segmentLength || t > 2f * segmentLength) return t;
        // A few Newton steps along the arc; segments bend at most a few degrees per meter.
        for (int k = 0; k < 3; k++)
        {
            LocalPose(i, t, out var local, out var localYaw);
            var world = starts[i] + rotation * local;
            var forward = Quaternion.Euler(0f, yaws[i] + localYaw, 0f) * Vector3.forward;
            t += Vector3.Dot(p - world, forward);
        }
        return t;
    }

    // Point at arc length s on an arc that turns bendDeg over length (local frame, heading +Z).
    public static Vector3 ArcPoint(float bendDeg, float s, float length)
    {
        float k = bendDeg * Mathf.Deg2Rad / length;
        if (Mathf.Abs(k) < 1e-6f) return new Vector3(0f, 0f, s);
        float phi = k * s;
        float r = 1f / k;
        return new Vector3(r * (1f - Mathf.Cos(phi)), 0f, r * Mathf.Sin(phi));
    }

    void Ensure(int i)
    {
        while (bends.Count <= i) Generate();
    }

    void Generate()
    {
        int i = bends.Count;
        float bend = NextBend(yaws[i] - originYaw);
        bends.Add(bend);
        var rotation = Quaternion.Euler(0f, yaws[i], 0f);
        starts.Add(starts[i] + rotation * ArcPoint(bend, segmentLength, segmentLength));
        yaws.Add(yaws[i] + bend);
    }

    float NextBend(float heading)
    {
        if (bendLeft > 0) { bendLeft--; return bendStep; }
        if (straightLeft > 0) { straightLeft--; return 0f; }

        // Start a new bend; turn the other way if this one would stray too far from the start heading.
        int segments = rng.Next(bendSegments.x, bendSegments.y + 1);
        float total = Mathf.Lerp(bendAngle.x, bendAngle.y, (float)rng.NextDouble());
        int direction = rng.Next(2) == 0 ? -1 : 1;
        if (Mathf.Abs(heading + direction * total) > maxDeviation) direction = -direction;
        total = Mathf.Min(total, maxDeviation - direction * heading);

        bendStep = direction * total / segments;
        bendLeft = segments - 1;
        straightLeft = rng.Next(straightRun.x, straightRun.y + 1);
        return bendStep;
    }
}

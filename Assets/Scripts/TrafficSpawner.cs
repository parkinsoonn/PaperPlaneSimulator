using System.Collections.Generic;
using UnityEngine;

// Two-way traffic on the street: the left lanes come toward the plane, the right lanes drive the
// same way more slowly (the plane overtakes them). Vehicles follow the street's bends, keep their
// distance to the one ahead, and are pooled. Types and colors come from Assets/Data/vehicles.json.
public class TrafficSpawner : MonoBehaviour
{
    public PaperPlaneController plane;
    public StreetBuilder street;
    public CityBuilder city;               // picks vehicle types that fit the current city
    public TextAsset vehicles;
    public Material material;              // Custom/CurvedWorld with vertex colors on

    public float[] oncomingLanes = { -6f, -2f };
    public float[] sameWayLanes = { 2f, 6f };
    public Vector2 oncomingSpeed = new Vector2(10f, 18f);
    public Vector2 sameWaySpeed = new Vector2(5f, 9f);
    public Vector2 spacing = new Vector2(18f, 45f);   // meters between vehicles in a lane
    public float spawnAhead = 190f;                   // inside the fog, so they don't pop in
    public float despawnBehind = 30f;
    public float clearStart = 60f;                    // no traffic right after the launch pad
    public float keepGap = 6f;                        // bumper-to-bumper distance kept in a lane
    public int maxVehicles = 24;

    class Lane
    {
        public float offset;
        public int direction;
        public float speed;                           // this throw's lane speed
        public readonly List<Vehicle> vehicles = new List<Vehicle>();
    }

    class Model
    {
        public VehicleData data;
        public Mesh[] meshes;                         // one per body color
    }

    readonly List<Model> models = new List<Model>();
    readonly List<Lane> lanes = new List<Lane>();
    readonly Stack<Vehicle> pool = new Stack<Vehicle>();
    int activeCount;
    bool active;

    void Awake()
    {
        if (vehicles == null) return;
        var set = JsonUtility.FromJson<VehicleSet>(vehicles.text);
        if (set?.vehicles == null) return;
        foreach (var data in set.vehicles)
        {
            var colors = data.bodyColors != null && data.bodyColors.Length > 0 ? data.bodyColors : new[] { "#B0B0B0" };
            var model = new Model { data = data, meshes = new Mesh[colors.Length] };
            for (int i = 0; i < colors.Length; i++)
                model.meshes[i] = CityMeshBuilder.Build(WithBodyColor(data.parts, colors[i]), $"{data.id}_{i}");
            models.Add(model);
        }
        foreach (float x in oncomingLanes) lanes.Add(new Lane { offset = x, direction = -1 });
        foreach (float x in sameWayLanes) lanes.Add(new Lane { offset = x, direction = 1 });
    }

    static PartData[] WithBodyColor(PartData[] parts, string body)
    {
        var result = new PartData[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            var p = parts[i];
            result[i] = new PartData
            {
                type = p.type, pos = p.pos, size = p.size, rot = p.rot, spacing = p.spacing, cell = p.cell,
                offset = p.offset, thickness = p.thickness,
                color = p.color == "@body" ? body : p.color,
            };
        }
        return result;
    }

    void Update()
    {
        if (plane == null || street == null || street.Path == null || models.Count == 0) return;

        // Back on the pad: clear the road.
        if (!plane.IsFlying && !plane.HasLanded)
        {
            if (active) ClearAll();
            return;
        }

        float planeS = street.PlaneProgress;
        if (!active) StartThrow(planeS);

        foreach (var lane in lanes)
        {
            Drive(lane, planeS);
            // New vehicles only appear far ahead, in the fog.
            Fill(lane, planeS, spawnAhead - spacing.y);
        }
    }

    void StartThrow(float planeS)
    {
        active = true;
        foreach (var lane in lanes)
        {
            var range = lane.direction < 0 ? oncomingSpeed : sameWaySpeed;
            lane.speed = Random.Range(range.x, range.y);
            // At launch the whole road ahead is filled, starting a little past the pad.
            Fill(lane, planeS, clearStart + Random.Range(0f, spacing.y));
        }
    }

    // Moves a lane's vehicles, keeping distance to the one ahead, and removes those left behind.
    void Drive(Lane lane, float planeS)
    {
        float dt = Time.deltaTime;
        // Order so that index 0 is the front-most vehicle in the lane's driving direction.
        lane.vehicles.Sort((a, b) => lane.direction > 0 ? b.s.CompareTo(a.s) : a.s.CompareTo(b.s));
        for (int i = 0; i < lane.vehicles.Count; i++)
        {
            var v = lane.vehicles[i];
            v.currentSpeed = v.speed;
            if (i > 0)
            {
                var ahead = lane.vehicles[i - 1];
                float gap = Mathf.Abs(ahead.s - v.s) - (ahead.length + v.length) * 0.5f;
                if (gap < keepGap) v.currentSpeed = Mathf.Min(v.speed, ahead.currentSpeed);
            }
            v.s += v.direction * v.currentSpeed * dt;
            Pose(v);
        }

        for (int i = lane.vehicles.Count - 1; i >= 0; i--)
        {
            var v = lane.vehicles[i];
            if (v.s < planeS - despawnBehind || v.s > planeS + spawnAhead + 60f)
            {
                lane.vehicles.RemoveAt(i);
                Release(v);
            }
        }
    }

    // Spawns vehicles in a lane between minAhead and spawnAhead meters in front of the plane,
    // each one a random spacing beyond the farthest vehicle already in the lane.
    void Fill(Lane lane, float planeS, float minAhead)
    {
        while (activeCount < maxVehicles)
        {
            float s = planeS + minAhead;
            foreach (var v in lane.vehicles) s = Mathf.Max(s, v.s + Random.Range(spacing.x, spacing.y));
            if (s > planeS + spawnAhead) return;
            lane.vehicles.Add(Spawn(lane, s));
        }
    }

    Vehicle Spawn(Lane lane, float s)
    {
        string cityId = city != null ? city.CityIdAt(Mathf.FloorToInt(s / street.segmentLength)) : null;
        var model = PickModel(cityId);
        var mesh = model.meshes[Random.Range(0, model.meshes.Length)];

        var v = pool.Count > 0 ? pool.Pop() : Create();
        v.gameObject.SetActive(true);
        v.GetComponent<MeshFilter>().sharedMesh = mesh;
        var box = v.GetComponent<BoxCollider>();
        box.center = mesh.bounds.center;
        box.size = mesh.bounds.size;
        v.s = s;
        v.lane = lane.offset;
        v.direction = lane.direction;
        v.speed = lane.speed * model.data.speedScale;
        v.currentSpeed = v.speed;
        v.length = mesh.bounds.size.z;
        Pose(v);
        activeCount++;
        return v;
    }

    Model PickModel(string cityId)
    {
        float total = 0f;
        foreach (var m in models) if (Fits(m, cityId)) total += m.data.weight;
        float r = Random.value * total;
        foreach (var m in models)
        {
            if (!Fits(m, cityId)) continue;
            r -= m.data.weight;
            if (r <= 0f) return m;
        }
        return models[0];
    }

    static bool Fits(Model m, string cityId)
    {
        var cities = m.data.cities;
        if (cities == null || cities.Length == 0) return true;
        foreach (var c in cities) if (c == cityId) return true;
        return false;
    }

    void Pose(Vehicle v)
    {
        var pose = street.Path.GetPose(v.s);
        float yaw = v.direction > 0 ? pose.yaw : pose.yaw + 180f;
        v.transform.SetPositionAndRotation(pose.position + pose.right * v.lane + Vector3.up * 0.02f, Quaternion.Euler(0f, yaw, 0f));
    }

    Vehicle Create()
    {
        var go = new GameObject("Vehicle");
        go.transform.SetParent(transform, false);
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>().sharedMaterial = material;
        var box = go.AddComponent<BoxCollider>();
        box.isTrigger = true;
        var body = go.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        return go.AddComponent<Vehicle>();
    }

    void Release(Vehicle v)
    {
        v.gameObject.SetActive(false);
        pool.Push(v);
        activeCount--;
    }

    void ClearAll()
    {
        foreach (var lane in lanes)
        {
            foreach (var v in lane.vehicles) Release(v);
            lane.vehicles.Clear();
        }
        active = false;
    }
}

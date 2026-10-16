using System.Collections.Generic;
using UnityEngine;

// Fills the street's building lots with city-themed buildings and sidewalk props.
// Cities follow each other along the road (a zone of buildings, then a short park), then repeat.
// Layout is decided per lot from its position and a per-throw seed, so recycled street segments
// always show the same buildings for the same spot.
public class CityBuilder : MonoBehaviour
{
    public PaperPlaneController plane;
    public StreetBuilder street;
    public Material buildingMaterial;      // Custom/CurvedWorld with vertex colors on
    public TextAsset[] cities;             // JSON city definitions, in order along the road
    public TextAsset park;                 // props for the park between two cities (same JSON format)
    public int zoneSegments = 16;          // 320 m of buildings per city
    public int transitionSegments = 2;     // 40 m park between cities
    public float sidewalkHeight = 0.2f;
    public float lotHeight = 0.03f;
    public float fogBlendSpeed = 0.5f;
    public int maxPropsPerSide = 10;

    class City
    {
        public CityData data;
        public Mesh[] buildingMeshes;
        public Mesh[] propMeshes;
        public List<int> normal = new List<int>();   // non-landmark building indices
        public List<int> singles = new List<int>();  // non-landmark, one lot wide
        public int landmark = -1;
        public Color lotColor, sidewalkColor, fogColor;
    }

    class Slot
    {
        public Transform root;
        public Transform model;
        public MeshFilter filter;
        public MeshRenderer renderer;
        public BoxCollider collider;
    }

    class SegmentSlots
    {
        public readonly Slot[] buildings = new Slot[2];          // 0 = left, 1 = right
        public readonly List<Slot>[] props = { new List<Slot>(), new List<Slot>() };
        public readonly Renderer[] lots = new Renderer[2];
        public readonly Renderer[] sidewalks = new Renderer[2];
    }

    readonly List<City> cityList = new List<City>();
    City parkCity;
    readonly Dictionary<Transform, SegmentSlots> slots = new Dictionary<Transform, SegmentSlots>();
    MaterialPropertyBlock block;
    Material skyInstance;
    Color parkColor = new Color(0.22f, 0.33f, 0.19f);
    int seed;

    void Awake()
    {
        block = new MaterialPropertyBlock();
        seed = Random.Range(0, int.MaxValue);
        if (street != null && street.field != null && street.field.HasProperty("_BaseColor"))
            parkColor = street.field.GetColor("_BaseColor");
        LoadCities();

        // Work on a copy so fog-colored sky changes never touch the asset.
        if (RenderSettings.skybox != null)
        {
            skyInstance = new Material(RenderSettings.skybox);
            RenderSettings.skybox = skyInstance;
        }
    }

    void OnEnable()
    {
        if (street == null) return;
        street.StreetReset += OnStreetReset;
        street.SegmentPlaced += OnSegmentPlaced;
    }

    void OnDisable()
    {
        if (street == null) return;
        street.StreetReset -= OnStreetReset;
        street.SegmentPlaced -= OnSegmentPlaced;
    }

    void LoadCities()
    {
        if (park != null) parkCity = LoadCity(park);
        if (cities == null) return;
        foreach (var asset in cities)
        {
            var city = asset != null ? LoadCity(asset) : null;
            if (city != null) cityList.Add(city);
        }
    }

    City LoadCity(TextAsset asset)
    {
        var data = JsonUtility.FromJson<CityData>(asset.text);
        if (data == null) return null;

        var city = new City { data = data };
        var buildings = data.buildings ?? new BuildingData[0];
        city.buildingMeshes = new Mesh[buildings.Length];
        for (int i = 0; i < buildings.Length; i++)
        {
            city.buildingMeshes[i] = CityMeshBuilder.Build(buildings[i].parts, $"{data.id}_{buildings[i].id}");
            if (buildings[i].landmark) city.landmark = i;
            else
            {
                city.normal.Add(i);
                if (buildings[i].segments <= 1) city.singles.Add(i);
            }
        }
        var props = data.props ?? new PropData[0];
        city.propMeshes = new Mesh[props.Length];
        for (int i = 0; i < props.Length; i++)
            city.propMeshes[i] = CityMeshBuilder.Build(props[i].parts, $"{data.id}_{props[i].id}");

        var env = data.environment ?? new CityEnvironment();
        city.lotColor = CityMeshBuilder.ParseColor(env.lotColor);
        city.sidewalkColor = CityMeshBuilder.ParseColor(env.sidewalkColor);
        city.fogColor = string.IsNullOrEmpty(env.fogColor) ? RenderSettings.fogColor : CityMeshBuilder.ParseColor(env.fogColor);
        return city;
    }

    // New throw: new layout.
    void OnStreetReset() => seed = Random.Range(0, int.MaxValue);

    int CycleLength => zoneSegments + transitionSegments;

    int LotIndex(float z) => Mathf.Max(0, Mathf.RoundToInt((z - street.startZ) / street.segmentLength));

    void OnSegmentPlaced(Transform segment)
    {
        var s = GetSlots(segment);
        if (cityList.Count == 0)
        {
            foreach (var slot in s.buildings) slot.root.gameObject.SetActive(false);
            return;
        }

        int lot = LotIndex(segment.position.z);
        int cycle = lot / CycleLength;
        int local = lot % CycleLength;
        City city = cityList[cycle % cityList.Count];
        City next = cityList[(cycle + 1) % cityList.Count];
        bool park = local >= zoneSegments;

        // Ground colors: city colors in the zone, grass lots in the park between cities.
        float t = park ? (local - zoneSegments + 1f) / (transitionSegments + 1f) : 0f;
        SetColor(s.lots, park ? parkColor : city.lotColor);
        SetColor(s.sidewalks, park ? Color.Lerp(city.sidewalkColor, next.sidewalkColor, t) : city.sidewalkColor);

        float lotX = street.roadWidth * 0.5f + street.sidewalkWidth + street.lotWidth * 0.5f;
        for (int side = 0; side < 2; side++)
        {
            float sign = side == 0 ? -1f : 1f;
            var slot = s.buildings[side];
            int building = -1;
            float zOffset = 0f;
            if (!park) PickBuilding(city, cycle, local, side, out building, out zOffset);

            if (building < 0)
            {
                slot.root.gameObject.SetActive(false);
            }
            else
            {
                slot.root.gameObject.SetActive(true);
                slot.root.localPosition = new Vector3(sign * lotX, lotHeight, zOffset);
                PlaceModel(slot, city.buildingMeshes[building], sign, true);
            }

            PlaceProps(s.props[side], segment, park ? parkCity : city, sign);
        }
    }

    // Lots are handled in pairs (blocks): one two-lot building, or two one-lot buildings.
    void PickBuilding(City city, int cycle, int local, int side, out int building, out float zOffset)
    {
        float segmentLength = street.segmentLength;
        int blockIndex = local / 2;
        int half = local % 2;
        var rng = new System.Random(Hash(seed, cycle, blockIndex, side));
        bool landmarkHere = city.landmark >= 0
            && blockIndex == zoneSegments / 4
            && side == (Hash(seed, cycle, 999, 0) & 1);

        int first = landmarkHere ? city.landmark : Pick(city, city.normal, rng);
        building = -1;
        zOffset = segmentLength * 0.5f;
        if (first < 0) return;

        if (city.data.buildings[first].segments >= 2)
        {
            // Hosted by the first lot of the block, centered on the block.
            if (half == 0)
            {
                building = first;
                zOffset = segmentLength;
            }
            return;
        }
        building = half == 0 ? first : Pick(city, city.singles, rng);
    }

    int Pick(City city, List<int> candidates, System.Random rng)
    {
        if (candidates.Count == 0) return -1;
        float total = 0f;
        foreach (int i in candidates) total += Mathf.Max(0f, city.data.buildings[i].weight);
        float r = (float)rng.NextDouble() * total;
        foreach (int i in candidates)
        {
            r -= Mathf.Max(0f, city.data.buildings[i].weight);
            if (r < 0f) return i;
        }
        return candidates[candidates.Count - 1];
    }

    void PlaceProps(List<Slot> pool, Transform segment, City city, float sign)
    {
        int used = 0;
        if (city != null && city.data.props != null)
        {
            float rel = segment.position.z - street.startZ;
            for (int p = 0; p < city.data.props.Length; p++)
            {
                var prop = city.data.props[p];
                if (prop.spacing <= 0f) continue;
                float z = Mathf.Ceil((rel - prop.phase) / prop.spacing) * prop.spacing + prop.phase;
                for (; z < rel + street.segmentLength && used < maxPropsPerSide; z += prop.spacing)
                {
                    if (pool.Count <= used) pool.Add(CreateSlot(segment, $"Prop_{(sign < 0 ? "L" : "R")}{pool.Count}", false));
                    var slot = pool[used++];
                    slot.root.gameObject.SetActive(true);
                    // Props past the sidewalk (e.g. park trees) stand on the lot, not on the curb.
                    float y = prop.offsetX > street.roadWidth * 0.5f + street.sidewalkWidth ? lotHeight : sidewalkHeight;
                    slot.root.localPosition = new Vector3(sign * prop.offsetX, y, z - rel);
                    PlaceModel(slot, city.propMeshes[p], sign, false);
                }
            }
        }
        for (int i = used; i < pool.Count; i++) pool[i].root.gameObject.SetActive(false);
    }

    // Models are authored for the right side (x = away from the road); the left side is mirrored.
    static void PlaceModel(Slot slot, Mesh mesh, float sign, bool withCollider)
    {
        slot.filter.sharedMesh = mesh;
        slot.model.localScale = new Vector3(sign, 1f, 1f);
        if (!withCollider || slot.collider == null) return;
        Bounds b = mesh.bounds;
        slot.collider.center = new Vector3(b.center.x * sign, b.center.y, b.center.z);
        slot.collider.size = b.size;
    }

    SegmentSlots GetSlots(Transform segment)
    {
        if (slots.TryGetValue(segment, out var s)) return s;
        s = new SegmentSlots();
        s.buildings[0] = CreateSlot(segment, "Building_L", true);
        s.buildings[1] = CreateSlot(segment, "Building_R", true);
        s.lots[0] = FindRenderer(segment, "LotLeftGround");
        s.lots[1] = FindRenderer(segment, "LotRightGround");
        s.sidewalks[0] = FindRenderer(segment, "SidewalkLeft");
        s.sidewalks[1] = FindRenderer(segment, "SidewalkRight");
        slots[segment] = s;
        return s;
    }

    Slot CreateSlot(Transform parent, string name, bool withCollider)
    {
        var root = new GameObject(name).transform;
        root.SetParent(parent, false);
        var model = new GameObject("Model").transform;
        model.SetParent(root, false);
        var slot = new Slot
        {
            root = root,
            model = model,
            filter = model.gameObject.AddComponent<MeshFilter>(),
            renderer = model.gameObject.AddComponent<MeshRenderer>(),
        };
        slot.renderer.sharedMaterial = buildingMaterial;
        if (withCollider) slot.collider = root.gameObject.AddComponent<BoxCollider>();
        return slot;
    }

    static Renderer FindRenderer(Transform segment, string name)
    {
        var child = segment.Find(name);
        return child != null ? child.GetComponent<Renderer>() : null;
    }

    void SetColor(Renderer[] renderers, Color color)
    {
        block.SetColor("_BaseColor", color);
        foreach (var r in renderers)
            if (r != null) r.SetPropertyBlock(block);
    }

    void Update()
    {
        if (plane == null || cityList.Count == 0) return;

        // Fog (and the sky below the horizon) drift to the color of the city the plane is in.
        int lot = LotIndex(plane.transform.position.z);
        int cycle = lot / CycleLength;
        int local = lot % CycleLength;
        Color target = cityList[cycle % cityList.Count].fogColor;
        if (local >= zoneSegments)
        {
            float t = (local - zoneSegments + 1f) / (transitionSegments + 1f);
            target = Color.Lerp(target, cityList[(cycle + 1) % cityList.Count].fogColor, t);
        }
        Color fog = Color.Lerp(RenderSettings.fogColor, target, 1f - Mathf.Exp(-fogBlendSpeed * Time.deltaTime));
        RenderSettings.fogColor = fog;
        if (skyInstance != null && skyInstance.HasProperty("_GroundColor")) skyInstance.SetColor("_GroundColor", fog);
    }

    static int Hash(int a, int b, int c, int d)
    {
        unchecked
        {
            int h = a;
            h = h * 31 + b;
            h = h * 31 + c;
            h = h * 31 + d;
            h ^= h >> 13;
            h *= (int)0x5bd1e995;
            h ^= h >> 15;
            return h & 0x7fffffff;
        }
    }
}

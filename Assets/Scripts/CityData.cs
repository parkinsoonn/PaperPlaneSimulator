using System;

// City definitions loaded from Assets/Data/Cities/*.json (JsonUtility).
// Coordinates: x = away from the road, y = up, z = along the road; meters.
// Building parts are relative to the lot center on the ground; prop parts to the prop's foot.

[Serializable]
public class CityData
{
    public string id;
    public string name;
    public CityEnvironment environment;
    public BuildingData[] buildings;
    public PropData[] props;
}

[Serializable]
public class CityEnvironment
{
    public string lotColor;
    public string sidewalkColor;
    public string fogColor;
}

[Serializable]
public class BuildingData
{
    public string id;
    public int segments = 1;     // 1 = one 20 m lot, 2 = spans two lots
    public float weight = 1f;    // relative chance among the city's normal buildings
    public bool landmark;        // placed once per city zone
    public PartData[] parts;
}

[Serializable]
public class PropData
{
    public string id;
    public float spacing = 20f;  // meters between copies along the sidewalk
    public float phase;          // offset of the first copy along the road
    public float offsetX = 9f;   // distance from the road center
    public PartData[] parts;
}

[Serializable]
public class PartData
{
    public string type;          // box, cylinder, cone, wedge, dome, windows, facade
    public float[] pos;          // center
    public float[] size;         // x, y, z
    public float[] rot;          // euler degrees (optional)
    public string color;         // hex, e.g. "#8B3A2B"
    public float[] spacing;      // windows/facade: row step, column step
    public float[] cell;         // windows/facade (optional): window height, width
    public float offset = 0.05f;     // facade: how far the grid sits out from the wall
    public float thickness = 0.15f;  // facade: window depth
}

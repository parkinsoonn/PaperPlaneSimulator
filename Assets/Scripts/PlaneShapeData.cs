using System;

// JSON format of a shop plane shape (Assets/Resources/Planes/<id>.json), read with JsonUtility.
// The model is a set of thin flat paper panels, like the classic paper_plane model.
[Serializable]
public class PlaneShapeData
{
    public string id;
    public string name;
    public float[] wingtip;       // right wingtip (x, y, z) where the wind trail starts; the left one is mirrored
    public PanelData[] panels;
}

[Serializable]
public class PanelData
{
    public string name;
    public float[] points;        // 3 or 4 corners of a flat panel, flattened: x1,y1,z1, x2,y2,z2, ...
    public float thickness;       // meters, across the panel's plane, centered on it
    public string mat;            // "paper", "shade" or "accent"
    public bool mirror;           // also add the panel mirrored across x = 0 (the left half)
}

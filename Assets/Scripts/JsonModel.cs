using UnityEngine;

// Builds this object's mesh from a JSON parts list (same format as a city building),
// e.g. the launch tower. Runs in the editor too so the model shows in the Scene view.
[ExecuteAlways]
public class JsonModel : MonoBehaviour
{
    public TextAsset model;     // BuildingData JSON: { "id": ..., "parts": [ ... ] }
    public Material material;   // Custom/CurvedWorld with vertex colors on

    void OnEnable() => Rebuild();
    void OnValidate() => Rebuild();

    void Rebuild()
    {
        if (model == null) return;
        var data = JsonUtility.FromJson<BuildingData>(model.text);
        if (data == null) return;

        var filter = GetComponent<MeshFilter>();
        if (filter == null) filter = gameObject.AddComponent<MeshFilter>();
        var renderer = GetComponent<MeshRenderer>();
        if (renderer == null) renderer = gameObject.AddComponent<MeshRenderer>();

        var mesh = CityMeshBuilder.Build(data.parts, data.id);
        mesh.hideFlags = HideFlags.DontSave;   // generated, never stored in the scene
        filter.sharedMesh = mesh;
        if (material != null) renderer.sharedMaterial = material;
    }
}

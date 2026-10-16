using UnityEngine;
using UnityEngine.Rendering;

// Drives the Custom/CurvedWorld shader: the world bends down with distance from the camera
// that is rendering, like Subway Surfers. Visual only; colliders stay flat.
[ExecuteAlways]
public class CurvedWorld : MonoBehaviour
{
    public float curvature = 0.0015f;   // meters of drop per squared meter (0.0015 -> 15 m at 100 m)

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCamera;
        Shader.SetGlobalFloat("_CurveStrength", curvature);
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCamera;
        Shader.SetGlobalFloat("_CurveStrength", 0f);
    }

    void OnValidate() => Shader.SetGlobalFloat("_CurveStrength", curvature);

    // Bend around whichever camera is about to render (game camera, scene view, previews).
    void OnBeginCamera(ScriptableRenderContext context, Camera cam)
    {
        Shader.SetGlobalFloat("_CurveStrength", curvature);
        Shader.SetGlobalVector("_CurveOrigin", cam.transform.position);
    }
}

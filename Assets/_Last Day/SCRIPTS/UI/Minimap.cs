using UnityEngine;
using UnityEngine.Rendering;

public class Minimap : MonoBehaviour
{   

    Camera cam;

    void OnEnable()
    {
        cam = GetComponent<Camera>();

        RenderPipelineManager.beginCameraRendering += BeginCamera;
        RenderPipelineManager.endCameraRendering += EndCamera;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= BeginCamera;
        RenderPipelineManager.endCameraRendering -= EndCamera;
    }

    void BeginCamera(ScriptableRenderContext context, Camera renderingCamera)
    {
        if (renderingCamera == cam)
        {
            RenderSettings.fog = false;
        }
    }

    void EndCamera(ScriptableRenderContext context, Camera renderingCamera)
    {
        if (renderingCamera == cam)
        {
            RenderSettings.fog = true;
        }
    }


}
using UnityEngine;
using UnityEngine.UI;

public class GunAim : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public RectTransform crosshairRect;

    [Header("Settings")]
    public float range = 1000f;

    void Update()
    {
        UpdateCrosshair();
    }

    void UpdateCrosshair()
    {
        if (crosshairRect == null) return;

        // ray từ giữa màn hình
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, range))
            targetPoint = hit.point;
        else
            targetPoint = ray.GetPoint(range);

        // chiếu target point ra màn hình
        Vector3 screenPos = cam.WorldToScreenPoint(targetPoint);

        if (screenPos.z > 0)
            crosshairRect.position = screenPos;
    }
}
using UnityEngine;
using UnityEngine.UI;

public class CrosshairFollow : MonoBehaviour
{
    [Header("References")]
    public Transform muzzlePoint;        // Kéo MuzzlePoint vào đây
    public RectTransform crosshairRect;  // Kéo Crosshair UI vào đây
    public Camera mainCamera;

    [Header("Settings")]
    public bool hideWhenBehindCamera = true;
    public Image crosshairImage; // optional, để ẩn/hiện

    void LateUpdate() // LateUpdate để chạy sau animation
    {
        if (muzzlePoint == null || crosshairRect == null) return;

        Vector3 screenPos = mainCamera.WorldToScreenPoint(muzzlePoint.position);

        // Nếu đầu nòng nằm sau camera thì ẩn crosshair
        bool isInFront = screenPos.z > 0;

        if (hideWhenBehindCamera && crosshairImage != null)
            crosshairImage.enabled = isInFront;

        if (isInFront)
            crosshairRect.position = new Vector2(screenPos.x, screenPos.y);
    }
}
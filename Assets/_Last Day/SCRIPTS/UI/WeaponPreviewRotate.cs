using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponPreviewRotate : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Target & Rotation Settings")]
    public Transform previewRoot;
    public float rotateSpeed = 5f;

    private bool dragging;
    private float currentY;
    private float fixedX;
    private float fixedZ;

    void Start()
    {
        if (previewRoot != null)
        {
            InitRotation(previewRoot);
        }
    }

    void Update()
    {
        if (!dragging || previewRoot == null)
            return;

        currentY -= Input.GetAxis("Mouse X") * rotateSpeed;

        previewRoot.localRotation = Quaternion.Euler(
            fixedX,
            currentY,
            fixedZ
        );
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
    }

    // Gọi hàm này từ GunShopUI khi người chơi click chọn một khẩu súng mới
    public void UpdatePreviewGun(Transform newGunTransform)
    {
        previewRoot = newGunTransform;
        InitRotation(newGunTransform);
    }

    private void InitRotation(Transform target)
    {
        fixedX = target.localEulerAngles.x;
        fixedZ = target.localEulerAngles.z;
        currentY = target.localEulerAngles.y;
    }

    // Đưa súng về góc nhìn ngang ban đầu
    public void ResetRotation()
    {
        if (previewRoot == null) return;
        currentY = 0f;
        previewRoot.localRotation = Quaternion.Euler(fixedX, currentY, fixedZ);
    }
}
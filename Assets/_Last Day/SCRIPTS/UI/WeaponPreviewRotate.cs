using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponPreviewRotate : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler
{
    public Transform previewRoot;

    public float rotateSpeed = 5f;

    private bool dragging;
    private float currentY;

    private float fixedX;
    private float fixedZ;

    void Start()
    {
        fixedX = previewRoot.localEulerAngles.x;
        fixedZ = previewRoot.localEulerAngles.z;

        currentY = previewRoot.localEulerAngles.y;
    }

    void Update()
    {
        if (!dragging)
            return;

        currentY -= Input.GetAxis("Mouse X") * rotateSpeed;

        previewRoot.localRotation =
            Quaternion.Euler(
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
}
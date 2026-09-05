using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHold : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    private Vector3 originalScale;
    private Color originalColor;
    public Graphic targetGraphic;
    public Color hoverColor = new Color32(0xC9, 0xFF, 0x2D, 0xFF);
    public float hoverScale = 1.1f;
    public float holdScale = 0.95f;

    void Start()
    {
        originalScale = transform.localScale;

        if (targetGraphic == null)
            targetGraphic = GetComponent<Graphic>() ?? GetComponentInChildren<Graphic>();

        if (targetGraphic != null)
            originalColor = targetGraphic.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * hoverScale;
        if (targetGraphic != null)
            targetGraphic.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
        if (targetGraphic != null)
            targetGraphic.color = originalColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = originalScale * holdScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = originalScale * hoverScale;
        if (targetGraphic != null)
            targetGraphic.color = hoverColor;
    }
}
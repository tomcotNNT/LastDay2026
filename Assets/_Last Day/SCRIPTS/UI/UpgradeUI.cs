using System.Collections;
using UnityEngine;

public class UpgradeUI : MonoBehaviour
{
    [Header("Slots")]
    public RectTransform[] slots;
    public float shakeDuration = 1f;
    public float shakeStrength = 12f;

    [Header("Result")]
    public TextPopup resultUI;

    private Vector2[] startPositions;
    private bool isUpgrading;

    void Awake()
    {
        startPositions = new Vector2[slots.Length];

        for (int i = 0; i < slots.Length; i++)
            startPositions[i] = slots[i].anchoredPosition;
    }

    public void OnClickUpgrade()
    {
        if (isUpgrading) return;

        bool success = Random.value <= 0.7f; // 70% thành công
        StartCoroutine(UpgradeSequence(success));
    }

    IEnumerator UpgradeSequence(bool success)
    {
        isUpgrading = true;

        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < slots.Length; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * shakeStrength;
                slots[i].anchoredPosition = startPositions[i] + randomOffset;
            }

            yield return null;
        }

        for (int i = 0; i < slots.Length; i++)
            slots[i].anchoredPosition = startPositions[i];

        if (success)
            resultUI.ShowSuccess();
        else
            resultUI.ShowFail();

        isUpgrading = false;
    }
}
using System.Collections;
using TMPro;
using UnityEngine;

public class MissionUI : MonoBehaviour
{   
    public static MissionUI Instance;
    
    [Header("UI")]
    public RectTransform panel;
    public TMP_Text missionText;

    [Header("Mission Text")]
    [TextArea]
    public string textToShow;

    [Header("Slide")]
    public Vector2 hiddenPos;
    public Vector2 showPos;

    private float slideSpeed = 8f;

    [Header("Typing")]
    private float startTypingDelay = 0.5f;
    private float typingSpeed = 0.03f;

    // =====================================================
    // START
    // =====================================================

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(ShowUI());
    }

    // =====================================================
    // SHOW UI
    // =====================================================

    IEnumerator ShowUI()
    {
        missionText.text = "";

        // bắt đầu ở ngoài màn hình
        panel.anchoredPosition = hiddenPos;

        // chạy từ trái qua
        while (
            Vector2.Distance(
                panel.anchoredPosition,
                showPos
            ) > 1f
        )
        {
            panel.anchoredPosition =
                Vector2.Lerp(
                    panel.anchoredPosition,
                    showPos,
                    Time.deltaTime * slideSpeed
                );

            yield return null;
        }

        panel.anchoredPosition = showPos;

        // chờ 1.5s
        yield return new WaitForSeconds(
            startTypingDelay
        );

        // chạy chữ từng chữ
        foreach(char c in textToShow)
        {
            missionText.text += c;

            yield return new WaitForSeconds(
                typingSpeed
            );
        }
    }
}
using UnityEngine;
using TMPro;
using System.Collections;

public class LoadingFade : MonoBehaviour
{
    public float loadingTime = 3f;
    public float dotSpeed = 0.5f;
    public float fadeDuration = 1f;

    private TMP_Text text;

    void Start()
    {
        text = GetComponentInChildren<TMP_Text>();

        StartCoroutine(LoadingRoutine());
    }

    IEnumerator LoadingRoutine()
    {
        float timer = 0f;

        // Loading...
        while (timer < loadingTime)
        {
            text.text = "Loading.";
            yield return new WaitForSeconds(dotSpeed);

            timer += dotSpeed;
            if (timer >= loadingTime) break;

            text.text = "Loading..";
            yield return new WaitForSeconds(dotSpeed);

            timer += dotSpeed;
            if (timer >= loadingTime) break;

            text.text = "Loading...";
            yield return new WaitForSeconds(dotSpeed);

            timer += dotSpeed;
        }

        // Fade out
        float fade = 0f;
        Color startColor = text.color;

        while (fade < fadeDuration)
        {
            fade += Time.deltaTime;

            Color c = startColor;
            c.a = Mathf.Lerp(1f, 0f, fade / fadeDuration);

            text.color = c;

            yield return null;
        }

        gameObject.SetActive(false);
    }
}
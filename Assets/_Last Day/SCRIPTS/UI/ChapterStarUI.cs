using UnityEngine;

public class ChapterStarUI : MonoBehaviour
{
    public int chapterIndex = 1;

    public GameObject[] stars;

    void Start()
    {
        int starCount =
            PlayerPrefs.GetInt(
                "Chapter_" + chapterIndex + "_Stars",
                0
            );

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].SetActive(i < starCount);
        }
    }
}
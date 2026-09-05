using UnityEngine;
using UnityEngine.SceneManagement;

public class MapMenuController : MonoBehaviour
{
    [System.Serializable]
    public class ChapterInfo
    {
        public string chapterName = "Chapter 1";
        public string sceneName = "Map1";
        public GameObject[] stars; // Kéo các icon sao của Map tương ứng vào đây
    }

    [Header("Chapter Configurations")]
    [SerializeField] private ChapterInfo[] chapters;

    [Header("Default Scenes")]
    [SerializeField] private string lobbySceneName = "Lobby";

    private void Start()
    {
        UpdateAllChapterStars();
    }

    // Đọc số sao đã lưu từ PlayerPrefs và kích hoạt icon sao
    private void UpdateAllChapterStars()
    {
        for (int i = 0; i < chapters.Length; i++)
        {
            int chapterIndex = i + 1;
            int starCount = PlayerPrefs.GetInt("Chapter_" + chapterIndex + "_Stars", 0);

            if (chapters[i].stars != null)
            {
                for (int s = 0; s < chapters[i].stars.Length; s++)
                {
                    if (chapters[i].stars[s] != null)
                    {
                        chapters[i].stars[s].SetActive(s < starCount);
                    }
                }
            }
        }
    }

    // Gán vào On Click () của nút Map1
    public void LoadChapter1()
    {
        SceneManager.LoadScene(chapters.Length > 0 ? chapters[0].sceneName : "Map1");
    }

    // Gán vào On Click () của nút Map2 (nếu mở khóa)
    public void LoadChapter2()
    {
        SceneManager.LoadScene(chapters.Length > 1 ? chapters[1].sceneName : "Map2");
    }

    // Tải Scene theo tên tùy biến truyền trực tiếp từ Inspector
    public void LoadChapterByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Gán vào On Click () của nút BACK
    public void BackToLobby()
    {
        SceneManager.LoadScene(lobbySceneName);
    }
}
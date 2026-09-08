using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentBGM : MonoBehaviour
{
    public static PersistentBGM Instance;

    [Header("Audio Source")]
    public AudioSource bgmSource;

    [Header("Danh sách Scene sử dụng bản nhạc này")]
    // Nhập chính xác tên các Scene cần phát nhạc nền chung
    public string[] allowedScenes = new string[] { "Start", "Lobby", "Upgrade", "MainMenu" };

    [Header("Scene Vòng quay (Dùng nhạc riêng)")]
    public string wheelSceneName = "Lucky";

    void Awake()
    {
        // 1. Triển khai Singleton: Đảm bảo chỉ có DUY NHẤT 1 bộ phát nhạc tồn tại
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Bắt buộc tách khỏi GameObject cha trước khi DontDestroyOnLoad
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        if (bgmSource == null)
            bgmSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Kiểm tra xem Scene vừa load có thuộc danh sách dùng nhạc chung không
        bool isAllowed = false;
        foreach (string sceneName in allowedScenes)
        {
            if (scene.name.Equals(sceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                isAllowed = true;
                break;
            }
        }

        if (isAllowed)
        {
            // Nếu nhạc đang tạm dừng (do vừa từ scene Lucky quay lại) -> Tiếp tục phát
            if (bgmSource != null && !bgmSource.isPlaying)
            {
                bgmSource.UnPause();
                if (!bgmSource.isPlaying)
                    bgmSource.Play();
            }
        }
        else
        {
            // Nếu bước vào scene Lucky (hoặc Map bắn súng) -> Tạm dừng nhạc để nhường chỗ
            if (bgmSource != null && bgmSource.isPlaying)
            {
                bgmSource.Pause();
            }
        }
    }
}
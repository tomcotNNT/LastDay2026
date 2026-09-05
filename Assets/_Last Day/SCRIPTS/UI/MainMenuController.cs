using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels (Bảng giao diện)")]
    public GameObject mainButtonsPanel; // Kéo GameObject 'Menu' vào đây
    public GameObject settingsPanel;    // Kéo GameObject 'SETTING' vào đây
    public GameObject howToPlayPanel;   // Kéo GameObject 'HowToPlay' vào đây

    // 1. Chức năng nút PLAY -> Chuyển sang màn Lobby
    public void OnClickPlay()
    {
        SceneManager.LoadScene("Lobby"); 
    }

    // 2. Chức năng nút ADMIN TEST -> Chuyển sang màn Admin
    public void OnClickAdminTest()
    {
        SceneManager.LoadScene("Admin");
    }

    // 3. Chức năng mở bảng OPTION / SETTING
    public void OnOpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
    }

    // 4. Chức năng mở bảng HOW TO PLAY
    public void OnOpenHowToPlay()
    {
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(false);
    }

    // 5. Chức năng nút BACK (Quay lại menu chính từ Setting / HowToPlay)
    public void OnCloseAllSubPanels()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (mainButtonsPanel != null) mainButtonsPanel.SetActive(true);
    }

    // 6. Chức năng nút EXIT -> Thoát Game
    public void OnClickExit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Dừng chạy trên Unity Editor
        #else
            Application.Quit(); // Đóng game khi đã build thành file .exe
        #endif
    }

    // 7. Hàm phụ trợ tải bất kỳ Scene nào bằng tên (Store, Lucky, Upgrade, MapMenu...)
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}   
using UnityEngine;
using UnityEngine.UI;

public class GameplaySettingUI : MonoBehaviour
{
    [Header("Buttons ON / OFF")]
    public Button minimapOnButton;
    public Button minimapOffButton;
    public Button healthBarOnButton;
    public Button healthBarOffButton;

    [Header("Tab Buttons")]
    public Button gameplayTabButton;
    public Button uiTabButton;
    public Button screenModeTabButton;

    [Header("Colors")]
    public Color activeColor = new Color32(0xC9, 0xFF, 0x2D, 0xFF);
    public Color inactiveColor = Color.white;

    [Header("Panels Content")]
    public GameObject GameplayContent;
    public GameObject UIContent;
    public GameObject ScreenModeContent;

    private GameObject minimapObj;
    private GameObject healthBarObj;

    void Start()
    {
        // 1. Tự động đăng ký sự kiện Click cho các Tab
        if (gameplayTabButton != null) gameplayTabButton.onClick.AddListener(ShowGameplayContent);
        if (uiTabButton != null) uiTabButton.onClick.AddListener(ShowUIContent);
        if (screenModeTabButton != null) screenModeTabButton.onClick.AddListener(ShowScreenModeContent);

        // 2. Tự động đăng ký sự kiện Click cho nút ON / OFF
        if (minimapOnButton != null) minimapOnButton.onClick.AddListener(OnMinimapOn);
        if (minimapOffButton != null) minimapOffButton.onClick.AddListener(OnMinimapOff);
        if (healthBarOnButton != null) healthBarOnButton.onClick.AddListener(OnHealthBarOn);
        if (healthBarOffButton != null) healthBarOffButton.onClick.AddListener(OnHealthBarOff);

        // 3. Đọc dữ liệu đã lưu từ PlayerPrefs (mặc định là 1: BẬT)
        bool minimap = PlayerPrefs.GetInt("Minimap", 1) == 1;
        bool healthbar = PlayerPrefs.GetInt("HealthBar", 1) == 1;

        SetMinimap(minimap, true);
        SetHealthBar(healthbar, true);

        // Mặc định mở Tab Âm thanh (hoặc tab Gameplay tùy chọn)
        ShowUIContent();
    }

    public void OnMinimapOn() => SetMinimap(true);
    public void OnMinimapOff() => SetMinimap(false);
    public void OnHealthBarOn() => SetHealthBar(true);
    public void OnHealthBarOff() => SetHealthBar(false);

    public void ShowGameplayContent()
    {
        SetContentActive(true, false, false);
        SetTabColors(true, false, false);
    }

    public void ShowUIContent()
    {
        SetContentActive(false, true, false);
        SetTabColors(false, true, false);
    }

    public void ShowScreenModeContent()
    {
        SetContentActive(false, false, true);
        SetTabColors(false, false, true);
    }

    void SetContentActive(bool gameplay, bool ui, bool screen)
    {
        if (GameplayContent != null) GameplayContent.SetActive(gameplay);
        if (UIContent != null) UIContent.SetActive(ui);
        if (ScreenModeContent != null) ScreenModeContent.SetActive(screen);
    }

    void SetTabColors(bool gameplay, bool ui, bool screen)
    {
        SetButtonColor(gameplayTabButton, gameplay ? activeColor : inactiveColor);
        SetButtonColor(uiTabButton, ui ? activeColor : inactiveColor);
        SetButtonColor(screenModeTabButton, screen ? activeColor : inactiveColor);
    }

    void SetMinimap(bool isOn, bool initializing = false)
    {
        if (minimapObj == null) minimapObj = GameObject.FindWithTag("Minimap");
        if (minimapObj != null) minimapObj.SetActive(isOn);

        if (!initializing)
        {
            PlayerPrefs.SetInt("Minimap", isOn ? 1 : 0);
            PlayerPrefs.Save();
        }

        UpdateButtonColors(minimapOnButton, minimapOffButton, isOn);
    }

    void SetHealthBar(bool isOn, bool initializing = false)
    {
        if (healthBarObj == null) healthBarObj = GameObject.FindWithTag("HealthBar");
        if (healthBarObj != null) healthBarObj.SetActive(isOn);

        if (!initializing)
        {
            PlayerPrefs.SetInt("HealthBar", isOn ? 1 : 0);
            PlayerPrefs.Save();
        }

        UpdateButtonColors(healthBarOnButton, healthBarOffButton, isOn);
    }

    void UpdateButtonColors(Button onButton, Button offButton, bool isOn)
    {
        SetButtonColor(onButton, isOn ? activeColor : inactiveColor);
        SetButtonColor(offButton, isOn ? inactiveColor : activeColor);
    }

    void SetButtonColor(Button button, Color color)
    {
        if (button != null && button.targetGraphic != null)
            button.targetGraphic.color = color;
    }
}
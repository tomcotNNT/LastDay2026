using TMPro;
using UnityEngine;

public class ScreenModeSetting : MonoBehaviour
{
    public TMP_Dropdown modeDropdown;

    void Start()
    {
        int savedMode = PlayerPrefs.GetInt("ScreenMode", 1);

        modeDropdown.value = savedMode;
        modeDropdown.RefreshShownValue();

        ApplyMode(savedMode);

        modeDropdown.onValueChanged.AddListener(ApplyMode);
    }

    public void ApplyMode(int mode)
    {
        switch (mode)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;

            case 1:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;

            case 2:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
        }

        PlayerPrefs.SetInt("ScreenMode", mode);
        PlayerPrefs.Save();
    }
}
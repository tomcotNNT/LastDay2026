using UnityEngine;
using UnityEngine.UI;

public class AudioUIManager : MonoBehaviour
{
    [Header("Fill Images")]
    public Image masterFill;        // Dùng cho thanh MUSIC
    public Image sfxFill;           // Dùng cho thanh SFX
    public Image environmentFill;   // Có thể để trống nếu menu không có thanh môi trường

    void Start()
    {
        RefreshUI();
    }

    public void MasterMinus()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.ChangeMaster(-0.1f);
        RefreshUI();
    }

    public void MasterPlus()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.ChangeMaster(0.1f);
        RefreshUI();
    }

    public void SFXMinus()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.ChangeSFX(-0.1f);
        RefreshUI();
    }

    public void SFXPlus()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.ChangeSFX(0.1f);
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (AudioManager.Instance == null) return;

        if (masterFill != null)
            masterFill.fillAmount = AudioManager.Instance.master;

        if (sfxFill != null)
            sfxFill.fillAmount = AudioManager.Instance.sfx;

        if (environmentFill != null)
            environmentFill.fillAmount = AudioManager.Instance.environment;
    }
}
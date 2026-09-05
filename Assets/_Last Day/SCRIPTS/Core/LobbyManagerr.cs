using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LobbyManagerr : MonoBehaviour
{
    public GameObject OptionPanel;
    public TMP_Text MoneyAmount;

    void Start()
    {
        if (OptionPanel != null)
        {
            OptionPanel.SetActive(false);
        }

        LoadMoney();
    }

    void LoadMoney()
    {
        int money = PlayerPrefs.GetInt("Money", 0);

        if (MoneyAmount != null)
        {
            MoneyAmount.text = money.ToString("N0");
        }
    }

    public void OpenOption()
    {
        if (OptionPanel != null)
            OptionPanel.SetActive(true);
    }

    public void CloseOption()
    {
        if (OptionPanel != null)
            OptionPanel.SetActive(false);
    }

    // 1. Chuyển sang Map / Chapter
    public void LoadChapter()
    {
        SceneManager.LoadScene("MapMenu");
    }

    // 2. Quay về màn hình Start Menu
    public void LoadStart()
    {
        SceneManager.LoadScene("Start");
    }

    // 3. Chuyển sang Store
    public void LoadStore()
    {
        SceneManager.LoadScene("Store");
    }

    // 4. Chuyển sang màn hình Nâng cấp (UPGRADE)
    public void LoadUpgrade()
    {
        SceneManager.LoadScene("Upgrade");
    }

    // 5. Chuyển sang Vòng quay may mắn (LUCKY WHEEL)
    public void LoadLuckyWheel()
    {
        SceneManager.LoadScene("Lucky");
    }
}
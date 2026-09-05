using TMPro;
using UnityEngine;

public class GrenadeUI : MonoBehaviour
{
    public GameObject grenadeUI;
    public TMP_Text grenadeText;

    public void UpdateGrenadeUI()
    {
        int grenadeCount =
            PlayerPrefs.GetInt("Grenade", 0);

        grenadeUI.SetActive(grenadeCount > 0);

        grenadeText.text =
            grenadeCount.ToString();
    }
}
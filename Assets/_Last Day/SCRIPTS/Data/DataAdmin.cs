using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DebugDataController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text dataText;
    public TMP_InputField valueInput;

    public GunData[] guns;

    const string MONEY_KEY = "Money";
    const string UPGRADE_SHARD_KEY = "UpgradeShard";
    const string TICKET_KEY = "Ticket";
    const string GRENADE_KEY = "Grenade";
    const string PROTECT_CARD_KEY = "ProtectCard";
    const string MUTANT_CRYSTAL_KEY = "MutantCrystal";

    void Start()
    {
        RefreshUI();
    }

    public void AddMoney() => AddValue(MONEY_KEY);
    public void SubtractMoney() => SubtractValue(MONEY_KEY);
    public void ResetMoney() => ResetValue(MONEY_KEY);

    public void AddUpgradeShard() => AddValue(UPGRADE_SHARD_KEY);
    public void SubtractUpgradeShard() => SubtractValue(UPGRADE_SHARD_KEY);
    public void ResetUpgradeShard() => ResetValue(UPGRADE_SHARD_KEY);

    public void AddTicket() => AddValue(TICKET_KEY);
    public void SubtractTicket() => SubtractValue(TICKET_KEY);
    public void ResetTicket() => ResetValue(TICKET_KEY);

    public void AddGrenade() => AddValue(GRENADE_KEY);
    public void SubtractGrenade() => SubtractValue(GRENADE_KEY);
    public void ResetGrenade() => ResetValue(GRENADE_KEY);

    public void AddProtectCard() => AddValue(PROTECT_CARD_KEY);
    public void SubtractProtectCard() => SubtractValue(PROTECT_CARD_KEY);
    public void ResetProtectCard() => ResetValue(PROTECT_CARD_KEY);

    public void AddMutantCrystal() => AddValue(MUTANT_CRYSTAL_KEY);
    public void SubtractMutantCrystal() => SubtractValue(MUTANT_CRYSTAL_KEY);
    public void ResetMutantCrystal() => ResetValue(MUTANT_CRYSTAL_KEY);

    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("Chưa nhập tên scene");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public void ResetAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        RefreshUI();
    }

    void AddValue(string key)
    {
        int value = PlayerPrefs.GetInt(key, 0);
        value += GetInputValue();

        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();

        RefreshUI();
    }

    void SubtractValue(string key)
    {
        int value = PlayerPrefs.GetInt(key, 0);
        value -= GetInputValue();
        value = Mathf.Max(0, value);

        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();

        RefreshUI();
    }

    void ResetValue(string key)
    {
        PlayerPrefs.SetInt(key, 0);
        PlayerPrefs.Save();

        RefreshUI();
    }

    public void ResetWeaponUpgradeData()
    {
        foreach (GunData gun in guns)
        {
            if (gun == null)
                continue;

            PlayerPrefs.DeleteKey(gun.gunName + "_Level");
        }

        PlayerPrefs.Save();

        Debug.Log("Reset toàn bộ Weapon Upgrade");
    }

    int GetInputValue()
    {
        if (valueInput == null)
            return 0;

        int.TryParse(valueInput.text, out int value);
        return Mathf.Max(0, value);
    }

    void RefreshUI()
    {
        int money = PlayerPrefs.GetInt(MONEY_KEY, 0);
        int shard = PlayerPrefs.GetInt(UPGRADE_SHARD_KEY, 0);
        int ticket = PlayerPrefs.GetInt(TICKET_KEY, 0);
        int grenade = PlayerPrefs.GetInt(GRENADE_KEY, 0);
        int protectCard = PlayerPrefs.GetInt(PROTECT_CARD_KEY, 0);
        int mutantCrystal = PlayerPrefs.GetInt(MUTANT_CRYSTAL_KEY, 0);

        if (dataText != null)
        {
            dataText.text =
                $"Money: {money:N0}\n" +
                $"Upgrade Shard: {shard:N0}\n" +
                $"Ticket: {ticket:N0}\n" +
                $"Grenade: {grenade:N0}\n" +
                $"Protect Card: {protectCard:N0}\n" +
                $"Mutant Crystal: {mutantCrystal:N0}";
        }
    }
}
using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [Header("Currency UI")]
    public TMP_Text moneyText;
    public TMP_Text crystalText;

    [Header("PlayerPrefs Keys")]
    public string moneyKey = "Money";
    public string crystalKey = "Crystal";

    public int Money { get; private set; }
    public int Crystal { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LoadCurrency();
        UpdateCurrencyUI();
    }

    public void LoadCurrency()
    {
        Money = PlayerPrefs.GetInt(moneyKey, 0);
        Crystal = PlayerPrefs.GetInt(crystalKey, 0);
    }

    public void UpdateCurrencyUI()
    {
        moneyText.text = Money.ToString();
        crystalText.text = Crystal.ToString();
    }

    public bool CanBuy(GunData gun)
    {
        if (gun.currencyType == CurrencyType.Money)
            return Money >= gun.Price;

        if (gun.currencyType == CurrencyType.Crystal)
            return Crystal >= gun.Price;

        return false;
    }

    public bool BuyGun(GunData gun)
    {
        if (!CanBuy(gun))
        {
            Debug.Log("Không đủ tiền mua: " + gun.gunName);
            return false;
        }

        if (gun.currencyType == CurrencyType.Money)
        {
            Money -= gun.Price;
            PlayerPrefs.SetInt(moneyKey, Money);
        }
        else if (gun.currencyType == CurrencyType.Crystal)
        {
            Crystal -= gun.Price;
            PlayerPrefs.SetInt(crystalKey, Crystal);
        }

        PlayerPrefs.Save();
        UpdateCurrencyUI();

        Debug.Log("Đã mua: " + gun.gunName);
        return true;
    }

    public void AddMoney(int amount)
    {
        Money += amount;
        PlayerPrefs.SetInt(moneyKey, Money);
        PlayerPrefs.Save();
        UpdateCurrencyUI();
    }

    public void AddCrystal(int amount)
    {
        Crystal += amount;
        PlayerPrefs.SetInt(crystalKey, Crystal);
        PlayerPrefs.Save();
        UpdateCurrencyUI();
    }
}
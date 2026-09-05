using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeaponUpgradeUI : MonoBehaviour
{
    [Header("Gun List")]
    public GunData[] guns;
    private int currentGunIndex;

    [Header("Gun UI")]
    public TMP_Text gunNameText;

    [Header("Stat Table")]
    public TMP_Text[] damageTexts;
    public TMP_Text[] recoilTexts;
    public TMP_Text[] magTexts;
    public TMP_Text[] reserveTexts;
    public TMP_Text[] reloadTexts;

    [Header("Level Outline")]
    public RectTransform[] levelColumns;
    public RectTransform levelOutline;

    [Header("Shard Slots")]
    public GameObject[] shardIcons;
    public TMP_Text shardAmountText;

    [Header("Money UI")]
    public TMP_Text moneyText;
    public TMP_Text costText;

    [Header("Upgrade Setting")]
    public int maxInsertShard = 5;
    public int upgradeCost = 150;

    private int insertedShard;
    private int money;
    private int shard;

    [Header("Weapon Preview")]
    public WeaponPreviewUI weaponPreviewUI;

    [Header("Text Popup")]
    public TextPopup textPopup;

    [Header("Upgrade Shake")]
    public RectTransform[] shakeSlots;
    public float shakeDuration = 1f;
    public float shakeStrength = 10f;

    private bool isUpgrading;
    private Vector2[] slotStartPositions;

    [Header("Protect Card")]
    public GameObject protectShieldPanel; // màng bảo vệ 
    public Button protectButton;
    public TMP_Text protectCardText;

    private int protectCard;
    private bool isProtected;

    [Header("Mutant Crystal")]
    public Button mutantCrystalButton;
    public TMP_Text mutantCrystalText;

    public GameObject mutantCrystalPanel; // hiệu ứng tím

    private int mutantCrystal;
    private bool isMutantCrystalActive;


    void Start()
    {   
        if (protectShieldPanel != null)
            protectShieldPanel.SetActive(false);

        insertedShard = 0;

        slotStartPositions = new Vector2[shakeSlots.Length];

        for (int i = 0; i < shakeSlots.Length; i++)
        {
            if (shakeSlots[i] != null)
                slotStartPositions[i] = shakeSlots[i].anchoredPosition;
        }

        SelectGun(0);

        RefreshAllUI();
    }

    public void SelectNextGun()
    {
        currentGunIndex++;

        if (currentGunIndex >= guns.Length)
            currentGunIndex = 0;

        insertedShard = 0;
        RefreshAllUI();
    }

    public void UseProtectCard()
    {

        if (isProtected)
            return;

        if (isMutantCrystalActive)
        {
            Debug.Log("Mutant Crystal đang được kích hoạt");
            return;
        }

        protectCard = PlayerPrefs.GetInt("ProtectCard", 0);

        if (protectCard <= 0)
        {
            Debug.Log("Không có Protect Card");
            return;
        }

        protectCard--;

        PlayerPrefs.SetInt("ProtectCard", protectCard);
        PlayerPrefs.Save();

        isProtected = true;

        if (protectShieldPanel != null)
            protectShieldPanel.SetActive(true);

        if (protectButton != null)
            protectButton.interactable = false;

        RefreshAllUI();
    }

    public void UseMutantCrystal()
    {
        if (isMutantCrystalActive)
            return;

        if (isProtected)
        {
            Debug.Log("Protect Card đang được kích hoạt");
            return;
        }    

        mutantCrystal = PlayerPrefs.GetInt("MutantCrystal", 0);

        if (mutantCrystal <= 0)
            return;

        mutantCrystal--;

        PlayerPrefs.SetInt("MutantCrystal", mutantCrystal);
        PlayerPrefs.Save();

        isMutantCrystalActive = true;

        if (mutantCrystalPanel != null)
            mutantCrystalPanel.SetActive(true);

        if (mutantCrystalButton != null)
            mutantCrystalButton.interactable = false;

        RefreshAllUI();
    }

    public void SelectPreviousGun()
    {
        currentGunIndex--;

        if (currentGunIndex < 0)
            currentGunIndex = guns.Length - 1;

        insertedShard = 0;
        RefreshAllUI();
    }

    public void SelectGun(int index)
    {
        if (guns == null || guns.Length == 0)
            return;

        currentGunIndex = Mathf.Clamp(index, 0, guns.Length - 1);
        insertedShard = 0;
        RefreshAllUI();
    }

    public void InsertShard()
    {
        LoadCurrency();

        if (shard <= 0)
            return;

        if (insertedShard >= maxInsertShard)
            return;

        shard--;
        insertedShard++;

        SaveCurrency();
        RefreshAllUI();
    }

    //Removed Shard
    public void RemoveShard()
    {
        LoadCurrency();

        if (insertedShard <= 0)
            return;

        shard++;
        insertedShard--;

        SaveCurrency();
        RefreshAllUI();
    }

    public void UpgradeWeapon()
    {
        if (isUpgrading)
            return;

        StartCoroutine(UpgradeSequence());
    }

    IEnumerator UpgradeSequence()
    {  

        LoadCurrency();

        GunData gun = guns[currentGunIndex];
        int level = GetGunLevel(gun);

        if (gun.upgradeLevels == null || gun.upgradeLevels.Length == 0)
            yield break;

        if (level >= gun.upgradeLevels.Length - 1)
            yield break;

        if (insertedShard <= 0 || insertedShard > maxInsertShard)
            yield break;

        if (money < upgradeCost)
            yield break;

        isUpgrading = true;

        yield return StartCoroutine(ShakeSlots());

        money -= upgradeCost;

        bool success;

        if (isMutantCrystalActive)
        {
            success = true;
        }
        else
        {
            success = RollUpgradeSuccess(insertedShard);
        }

        if (success)
        {
            if (isMutantCrystalActive)
            {
                int targetLevel =
                    Mathf.Min(
                        level + 5,
                        gun.upgradeLevels.Length - 1
                    );

                SetGunLevel(gun, targetLevel);

                Debug.Log("Mutant Crystal +3 Level");
            }
            else
            {
                SetGunLevel(gun, level + 1);
            }

            if (textPopup != null)
                textPopup.ShowSuccess();

            Debug.Log("Upgrade Success");
        }
        else
        {
            if (isProtected)
            {
                Debug.Log("Upgrade Failed -> Protect Card cứu cấp độ");
            }
            else
            {
                SetGunLevel(gun, 0);

                Debug.Log("Upgrade Failed -> LV0");
            }

            if (textPopup != null)
                textPopup.ShowFail();
        }

        ConsumeProtectCard(); 
        ConsumeMutantCrystal();  

        insertedShard = 0;

        SaveCurrency();
        RefreshAllUI();

        isUpgrading = false;
    }

    void ConsumeProtectCard()
    {
        if (!isProtected)
            return;

        isProtected = false;

        if (protectShieldPanel != null)
            protectShieldPanel.SetActive(false);

        if (protectButton != null)
            protectButton.interactable =
                PlayerPrefs.GetInt("ProtectCard", 0) > 0;
    }

    void ConsumeMutantCrystal()
    {
        if (!isMutantCrystalActive)
            return;

        isMutantCrystalActive = false;

        if (mutantCrystalPanel != null)
            mutantCrystalPanel.SetActive(false);
    }

    IEnumerator ShakeSlots()
    {
        Vector2[] startPositions = new Vector2[shakeSlots.Length];

        for (int i = 0; i < shakeSlots.Length; i++)
        {
            if (shakeSlots[i] != null)
                startPositions[i] = shakeSlots[i].anchoredPosition;
        }

        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < shakeSlots.Length; i++)
            {
                if (shakeSlots[i] == null)
                    continue;

                Vector2 randomOffset =
                    Random.insideUnitCircle * shakeStrength;

                shakeSlots[i].anchoredPosition =
                    startPositions[i] + randomOffset;
            }

            yield return null;
        }

        for (int i = 0; i < shakeSlots.Length; i++)
        {
            if (shakeSlots[i] != null)
                shakeSlots[i].anchoredPosition = startPositions[i];
        }
    }

    void RefreshAllUI()
    {
        LoadCurrency();

        if (guns == null || guns.Length == 0)
            return;

        GunData gun = guns[currentGunIndex];

        if (gunNameText != null)
            gunNameText.text = gun.gunName;

        if (weaponPreviewUI != null)
            weaponPreviewUI.ShowWeapon(currentGunIndex);    

        RefreshStatTable(gun);
        RefreshOutline(gun);
        RefreshShardSlots();
        RefreshCurrencyUI();
       
    }

    void RefreshStatTable(GunData gun)
    {
        if (gun == null)
            return;

        if (gun.upgradeLevels == null || gun.upgradeLevels.Length == 0)
            return;

        int currentLevel = GetGunLevel(gun);
        int maxLevelCount = Mathf.Min(4, gun.upgradeLevels.Length);

        for (int i = 0; i < 4; i++)
        {
            bool hasLevelData = i < maxLevelCount;
            bool unlocked = hasLevelData && i <= currentLevel;

            if (!unlocked)
            {
                SetText(damageTexts, i, "--");
                SetText(recoilTexts, i, "--");
                SetText(magTexts, i, "--");
                SetText(reserveTexts, i, "--");
                SetText(reloadTexts, i, "--");
                continue;
            }

            float damage = gun.damage + gun.upgradeLevels[i].damageBonus;
            float recoil = GunUpgradeCalculator.GetRecoilDisplay(gun, i);
            int mag = gun.maxAmmo + gun.upgradeLevels[i].magBonus;

            int baseReserve = 0;

            if (gun.inventoryData != null)
                baseReserve = gun.inventoryData.maxReserveAmmo;

            int reserve = baseReserve + gun.upgradeLevels[i].reserveBonus;

            float reload = gun.reloadTime - gun.upgradeLevels[i].reloadBonus;

            SetText(damageTexts, i, damage.ToString("0"));
            SetText(recoilTexts, i, recoil.ToString("0.0"));
            SetText(magTexts, i, mag.ToString());
            SetText(reserveTexts, i, reserve.ToString());
            SetText(reloadTexts, i, reload.ToString("0.0") + "s");
        }
    }


    void RefreshOutline(GunData gun)
    {
        int level = GetGunLevel(gun);

        if (levelOutline == null)
            return;

        if (levelColumns == null || levelColumns.Length == 0)
            return;

        level = Mathf.Clamp(level, 0, levelColumns.Length - 1);

        RectTransform target = levelColumns[level];

        levelOutline.localPosition = target.localPosition;
    }

    void RefreshShardSlots()
    {
        for (int i = 0; i < shardIcons.Length; i++)
        {
            if (shardIcons[i] != null)
                shardIcons[i].SetActive(i < insertedShard);
        }
    }

    void RefreshCurrencyUI()
    {
        if (moneyText != null)
            moneyText.text = money.ToString();

        if (shardAmountText != null)
            shardAmountText.text = shard.ToString();

        if (costText != null)
            costText.text = upgradeCost.ToString();

        if (protectCardText != null)
            protectCardText.text = protectCard.ToString();

        if (protectButton != null)
        {
            protectButton.interactable =
                !isProtected &&
                !isMutantCrystalActive &&
                protectCard > 0;
        }

        if (mutantCrystalButton != null)
        {
            mutantCrystalButton.interactable =
                !isMutantCrystalActive &&
                !isProtected &&
                mutantCrystal > 0;
        }

        if (mutantCrystalText != null)
            mutantCrystalText.text = mutantCrystal.ToString();


    }

    void SetText(TMP_Text[] texts, int index, string value)
    {
        if (texts == null)
            return;

        if (index < 0 || index >= texts.Length)
            return;

        if (texts[index] != null)
            texts[index].text = value;
    }

    int GetGunLevel(GunData gun)
    {
        return PlayerPrefs.GetInt(gun.gunName + "_Level", 0);
    }

    void SetGunLevel(GunData gun, int level)
    {
        PlayerPrefs.SetInt(gun.gunName + "_Level", level);
        PlayerPrefs.Save();
    }

    void LoadCurrency()
    {
        money = PlayerPrefs.GetInt("Money", 0);
        shard = PlayerPrefs.GetInt("UpgradeShard", 0);
        protectCard = PlayerPrefs.GetInt("ProtectCard", 0);
        mutantCrystal = PlayerPrefs.GetInt("MutantCrystal", 0);
    }

    void SaveCurrency()
    {
        PlayerPrefs.SetInt("Money", money);
        PlayerPrefs.SetInt("UpgradeShard", shard);
        PlayerPrefs.Save();
    }


    bool RollUpgradeSuccess(int shardCount)
    {
        int randomNumber = 0;
        bool success = false;

        switch (shardCount)
        {
            case 1:
                randomNumber = Random.Range(1, 11); // 1 -> 10
                success = randomNumber == 4;
                break;

            case 2:
                randomNumber = Random.Range(1, 9); // 1 -> 8
                success = randomNumber == 1;
                break;

            case 3:
                randomNumber = Random.Range(1, 8); // 1 -> 7
                success = randomNumber == 3;
                break;

            case 4:
                randomNumber = Random.Range(1, 8); // 1 -> 7
                success = randomNumber == 2 || randomNumber == 5;
                break;

            case 5:
                randomNumber = Random.Range(1, 7); // 1 -> 6
                success = randomNumber == 1 || randomNumber == 3 || randomNumber == 5;
                break;
        }

        Debug.Log(
            "Shard: " + shardCount +
            " | Random: " + randomNumber +
            " | Success: " + success
        );

        return success;
    }
}
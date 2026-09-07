using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeaponUpgradeUI : MonoBehaviour
{
    [Header("Buttons To Lock")]
    public Button backButton;
    public Button upgradeButton;
    public Button insertShardButton;
    public Button recoveryButton;
    public Button nextGunButton;
    public Button prevGunButton;
    public Button protectButton;
    public Button mutantCrystalButton;

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
    public GameObject protectShieldPanel;
    public TMP_Text protectCardText;

    private int protectCard;
    private bool isProtected;

    [Header("Mutant Crystal")]
    public TMP_Text mutantCrystalText;
    public GameObject mutantCrystalPanel;

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

    private void Update()
    {
        // Nhấn F4 trong lúc Play để đưa súng hiện tại về LV 0
        if (Input.GetKeyDown(KeyCode.F4))
        {
            ResetCurrentGunLevel();
        }
    }

    // ==========================================
    // CÁC HÀM TƯƠNG TÁC (ĐÃ CHẶN KHI ĐANG NÂNG CẤP)
    // ==========================================

    public void SelectNextGun()
    {
        if (isUpgrading) return;

        currentGunIndex++;
        if (currentGunIndex >= guns.Length)
            currentGunIndex = 0;

        insertedShard = 0;
        RefreshAllUI();
    }

    public void SelectPreviousGun()
    {
        if (isUpgrading) return;

        currentGunIndex--;
        if (currentGunIndex < 0)
            currentGunIndex = guns.Length - 1;

        insertedShard = 0;
        RefreshAllUI();
    }

    public void SelectGun(int index)
    {
        if (guns == null || guns.Length == 0) return;

        currentGunIndex = Mathf.Clamp(index, 0, guns.Length - 1);
        insertedShard = 0;
        RefreshAllUI();
    }

    public void InsertShard()
    {
        if (isUpgrading) return;

        LoadCurrency();
        if (shard <= 0 || insertedShard >= maxInsertShard) return;

        shard--;
        insertedShard++;

        SaveCurrency();
        RefreshAllUI();
    }

    public void RemoveShard()
    {
        if (isUpgrading) return;

        LoadCurrency();
        if (insertedShard <= 0) return;

        shard++;
        insertedShard--;

        SaveCurrency();
        RefreshAllUI();
    }

    public void UseProtectCard()
    {
        if (isUpgrading || isProtected || isMutantCrystalActive) return;

        protectCard = PlayerPrefs.GetInt("ProtectCard", 0);
        if (protectCard <= 0) return;

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
        if (isUpgrading || isMutantCrystalActive || isProtected) return;

        mutantCrystal = PlayerPrefs.GetInt("MutantCrystal", 0);
        if (mutantCrystal <= 0) return;

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

    public void UpgradeWeapon()
    {
        if (isUpgrading) return;
        StartCoroutine(UpgradeSequence());
    }

    // ==========================================
    // TIẾN TRÌNH NÂNG CẤP VŨ KHÍ
    // ==========================================

    IEnumerator UpgradeSequence()
    {
        LoadCurrency();

        GunData gun = guns[currentGunIndex];
        int level = GetGunLevel(gun);

        if (gun.upgradeLevels == null || gun.upgradeLevels.Length == 0) yield break;
        if (level >= gun.upgradeLevels.Length - 1) yield break;
        if (insertedShard <= 0 || insertedShard > maxInsertShard) yield break;
        if (money < upgradeCost) yield break;

        // 1. KHÓA TOÀN BỘ NÚT BẤM KHI BẮT ĐẦU
        isUpgrading = true;
        SetAllButtonsInteractable(false);

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
                int targetLevel = Mathf.Min(level + 5, gun.upgradeLevels.Length - 1);
                SetGunLevel(gun, targetLevel);
            }
            else
            {
                SetGunLevel(gun, level + 1);
            }

            if (textPopup != null) textPopup.ShowSuccess();
            Debug.Log("<color=green>Upgrade Success</color>");
        }
        else
        {
            if (isProtected)
            {
                Debug.Log("<color=yellow>Upgrade Failed -> Protect Card cứu cấp độ</color>");
            }
            else
            {
                SetGunLevel(gun, 0);
                Debug.Log("<color=red>Upgrade Failed -> Rớt về LV 0</color>");
            }

            if (textPopup != null) textPopup.ShowFail();
        }

        ConsumeProtectCard();
        ConsumeMutantCrystal();

        insertedShard = 0;
        SaveCurrency();
        RefreshAllUI();

        // 2. MỞ LẠI TOÀN BỘ NÚT BẤM SAU KHI NÂNG CẤP XONG
        isUpgrading = false;
        SetAllButtonsInteractable(true);
    }

    void SetAllButtonsInteractable(bool state)
    {
        if (backButton != null) backButton.interactable = state;
        if (upgradeButton != null) upgradeButton.interactable = state;
        if (insertShardButton != null) insertShardButton.interactable = state;
        if (recoveryButton != null) recoveryButton.interactable = state;
        if (nextGunButton != null) nextGunButton.interactable = state;
        if (prevGunButton != null) prevGunButton.interactable = state;

        if (!state)
        {
            if (protectButton != null) protectButton.interactable = false;
            if (mutantCrystalButton != null) mutantCrystalButton.interactable = false;
        }
        else
        {
            if (protectButton != null)
                protectButton.interactable = !isProtected && !isMutantCrystalActive && protectCard > 0;

            if (mutantCrystalButton != null)
                mutantCrystalButton.interactable = !isMutantCrystalActive && !isProtected && mutantCrystal > 0;
        }
    }

    void ConsumeProtectCard()
    {
        if (!isProtected) return;
        isProtected = false;

        if (protectShieldPanel != null)
            protectShieldPanel.SetActive(false);

        if (protectButton != null)
            protectButton.interactable = PlayerPrefs.GetInt("ProtectCard", 0) > 0;
    }

    void ConsumeMutantCrystal()
    {
        if (!isMutantCrystalActive) return;
        isMutantCrystalActive = false;

        if (mutantCrystalPanel != null)
            mutantCrystalPanel.SetActive(false);

        if (mutantCrystalButton != null)
            mutantCrystalButton.interactable = PlayerPrefs.GetInt("MutantCrystal", 0) > 0;
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
                if (shakeSlots[i] == null) continue;
                Vector2 randomOffset = Random.insideUnitCircle * shakeStrength;
                shakeSlots[i].anchoredPosition = startPositions[i] + randomOffset;
            }
            yield return null;
        }

        for (int i = 0; i < shakeSlots.Length; i++)
        {
            if (shakeSlots[i] != null)
                shakeSlots[i].anchoredPosition = startPositions[i];
        }
    }

    // ==========================================
    // VẼ LẠI GIAO DIỆN & CÔNG CỤ TEST
    // ==========================================

    [ContextMenu("DEBUG: Đưa súng hiện tại về LV 0 (F4)")]
    public void ResetCurrentGunLevel()
    {
        if (guns == null || guns.Length == 0) return;

        GunData gun = guns[currentGunIndex];
        SetGunLevel(gun, 0);
        RefreshAllUI();

        Debug.Log($"<color=cyan>[Test] Đã reset súng {gun.gunName} về LV 0 thành công!</color>");
    }

    [ContextMenu("DEBUG: Đưa TẤT CẢ súng về LV 0")]
    public void ResetAllGunsLevel()
    {
        if (guns == null) return;

        for (int i = 0; i < guns.Length; i++)
        {
            if (guns[i] != null)
            {
                SetGunLevel(guns[i], 0);
            }
        }

        RefreshAllUI();
        Debug.Log("<color=cyan>[Test] Đã reset toàn bộ súng trong danh sách về LV 0!</color>");
    }

    void RefreshAllUI()
    {
        LoadCurrency();

        if (guns == null || guns.Length == 0) return;

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
        if (gun == null || gun.upgradeLevels == null || gun.upgradeLevels.Length == 0) return;

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

        if (levelOutline == null || levelColumns == null || levelColumns.Length == 0) return;

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
        if (moneyText != null) moneyText.text = money.ToString();
        if (shardAmountText != null) shardAmountText.text = shard.ToString();
        if (costText != null) costText.text = upgradeCost.ToString();
        if (protectCardText != null) protectCardText.text = protectCard.ToString();
        if (mutantCrystalText != null) mutantCrystalText.text = mutantCrystal.ToString();

        if (protectButton != null)
        {
            protectButton.interactable = !isProtected && !isMutantCrystalActive && protectCard > 0;
        }

        if (mutantCrystalButton != null)
        {
            mutantCrystalButton.interactable = !isMutantCrystalActive && !isProtected && mutantCrystal > 0;
        }
    }

    void SetText(TMP_Text[] texts, int index, string value)
    {
        if (texts == null || index < 0 || index >= texts.Length) return;
        if (texts[index] != null) texts[index].text = value;
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
                randomNumber = Random.Range(1, 11);
                success = randomNumber == 4;
                break;
            case 2:
                randomNumber = Random.Range(1, 9);
                success = randomNumber == 1;
                break;
            case 3:
                randomNumber = Random.Range(1, 8);
                success = randomNumber == 3;
                break;
            case 4:
                randomNumber = Random.Range(1, 8);
                success = randomNumber == 2 || randomNumber == 5;
                break;
            case 5:
                randomNumber = Random.Range(1, 7);
                success = randomNumber == 1 || randomNumber == 3 || randomNumber == 5;
                break;
        }

        Debug.Log($"Shard: {shardCount} | Random: {randomNumber} | Success: {success}");
        return success;
    }
}
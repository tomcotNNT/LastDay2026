using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestDataUpgrade : MonoBehaviour
{
    [Header("--- 1. CÀI ĐẶT TỰ ĐỘNG ---")]
    [Tooltip("Tích chọn: Khi bấm Play sẽ lấy ngay số bạn gõ bên dưới. Bỏ tích: Đọc PlayerPrefs cũ.")]
    public bool applyOnStart = true;

    [Header("--- 2. RESET CẤP ĐỘ VŨ KHÍ ---")]
    [Tooltip("Tên biến lưu cấp độ súng trong PlayerPrefs (mặc định thử các mẫu phổ biến)")]
    public string weaponLevelKey = "AK47_Level";
    [Tooltip("Tự động tải lại Scene khi Reset cấp độ để khung vàng nhảy về LV 0 ngay lập tức")]
    public bool reloadSceneOnReset = true;

    [Header("--- 3. NHẬP SỐ LƯỢNG ĐỂ TEST ---")]
    public int testMoney = 9999;
    public int testUpgradeShard = 500;
    public int testProtectCard = 50;
    public int testMutantCrystal = 20;

    [Header("--- 4. CÁC TEXT AMOUNT ---")]
    public TMP_Text moneyText;
    public TMP_Text shardText;
    public TMP_Text protectCardText;
    public TMP_Text crystalText;

    private void Start()
    {
        if (applyOnStart)
        {
            ApplyCustomValues();
        }
        else
        {
            LoadDataToUI();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ApplyCustomValues();
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            SetAllToZero();
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            SetAllToMax();
        }
        // Bấm F4: Reset cấp vũ khí từ LV 3 về lại LV 0
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            ResetWeaponLevelToZero();
        }
    }

    // =========================================================
    // TÍNH NĂNG MỚI: RESET CẤP ĐỘ VŨ KHÍ VỀ LV 0
    // =========================================================
    [ContextMenu("4. RESET CẤP ĐỘ VŨ KHÍ VỀ LV 0 (F4)")]
    public void ResetWeaponLevelToZero()
    {
        // 1. Reset các key phổ biến thường dùng để lưu Level súng
        PlayerPrefs.SetInt(weaponLevelKey, 0);
        PlayerPrefs.SetInt("AK47_Level", 0);
        PlayerPrefs.SetInt("Gun_AK47_Level", 0);
        PlayerPrefs.SetInt("Weapon_AK47_Level", 0);
        PlayerPrefs.SetInt("gun_ak47_Level", 0);
        PlayerPrefs.SetInt("CurrentGunLevel", 0);
        PlayerPrefs.SetInt("WeaponLevel", 0);

        PlayerPrefs.Save();
        Debug.Log("<color=cyan>[Test Data] Đã đưa cấp độ vũ khí về LV 0!</color>");

        // 2. Tải lại Scene để bảng thông số và khung viền vàng lập tức nhảy về cột LV 0
        if (reloadSceneOnReset)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    [ContextMenu("1. Áp dụng số vừa nhập (F1)")]
    public void ApplyCustomValues()
    {
        PlayerPrefs.SetInt("Money", testMoney);
        PlayerPrefs.SetInt("UpgradeShard", testUpgradeShard);
        PlayerPrefs.SetInt("ProtectCard", testProtectCard);
        PlayerPrefs.SetInt("MutantCrystal", testMutantCrystal);
        PlayerPrefs.Save();

        UpdateAllUI(testMoney, testUpgradeShard, testProtectCard, testMutantCrystal);
        Debug.Log("<color=green>[Test Data] Đã nạp thành công thông số từ Inspector!</color>");
    }

    [ContextMenu("2. Đặt tất cả về 0 (F2)")]
    public void SetAllToZero()
    {
        testMoney = 0;
        testUpgradeShard = 0;
        testProtectCard = 0;
        testMutantCrystal = 0;

        PlayerPrefs.SetInt("Money", 0);
        PlayerPrefs.SetInt("UpgradeShard", 0);
        PlayerPrefs.SetInt("ProtectCard", 0);
        PlayerPrefs.SetInt("MutantCrystal", 0);
        PlayerPrefs.Save();

        UpdateAllUI(0, 0, 0, 0);
    }

    [ContextMenu("3. Bơm MAX 9999 (F3)")]
    public void SetAllToMax()
    {
        testMoney = 99999;
        testUpgradeShard = 9999;
        testProtectCard = 999;
        testMutantCrystal = 999;

        PlayerPrefs.SetInt("Money", testMoney);
        PlayerPrefs.SetInt("UpgradeShard", testUpgradeShard);
        PlayerPrefs.SetInt("ProtectCard", testProtectCard);
        PlayerPrefs.SetInt("MutantCrystal", testMutantCrystal);
        PlayerPrefs.Save();

        UpdateAllUI(testMoney, testUpgradeShard, testProtectCard, testMutantCrystal);
    }

    public void LoadDataToUI()
    {
        testMoney = PlayerPrefs.GetInt("Money", 0);
        testUpgradeShard = PlayerPrefs.GetInt("UpgradeShard", 0);
        testProtectCard = PlayerPrefs.GetInt("ProtectCard", 0);
        testMutantCrystal = PlayerPrefs.GetInt("MutantCrystal", 0);

        UpdateAllUI(testMoney, testUpgradeShard, testProtectCard, testMutantCrystal);
    }

    private void UpdateAllUI(int m, int s, int p, int c)
    {
        if (moneyText != null) moneyText.text = m.ToString();
        if (shardText != null) shardText.text = s.ToString();
        if (protectCardText != null) protectCardText.text = p.ToString();
        if (crystalText != null) crystalText.text = c.ToString();
    }
}
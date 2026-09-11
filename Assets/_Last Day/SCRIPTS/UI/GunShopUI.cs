using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GunShopUI : MonoBehaviour
{
    [Header("--- SÚNG MẶC ĐỊNH KHI VÀO SHOP ---")]
    public WeaponData defaultWeapon; // Kéo Data_AK47 vào đây

    [Header("--- HIỂN THỊ CHÍNH ---")]
    public Image gunPreviewImage;    // Kéo WeaponPreviewImage vào đây
    public TMP_Text gunNameText;      // Kéo Name vào đây
    public TMP_Text statsText;        // Kéo Stats vào đây

    [Header("--- KHUNG GIÁ TIỀN & MUA SÚNG ---")]
    public TMP_Text priceText;        // Kéo Price vào đây
    public Button buyButton;          // Kéo GameObject nút Buy vào đây
    public GameObject ownerObject;    // Kéo GameObject Owner vào đây

    [Header("--- THANH TIỀN TỆ TRÊN CÙNG ---")]
    public TMP_Text playerMoneyText;  // Kéo Text số tiền ở thanh bar góc trên

    private WeaponData currentWeapon;

    void Start()
    {
        // 1. Khởi tạo tiền mặc định nếu chưa từng lưu (10,000$)
        if (!PlayerPrefs.HasKey("Money"))
        {
            PlayerPrefs.SetInt("Money", 10000);
            PlayerPrefs.Save();
        }

        // 2. Mặc định mở khóa sẵn AK47 vĩnh viễn
        PlayerPrefs.SetInt("AK47_Unlocked", 1);
        PlayerPrefs.Save();

        // 3. Cập nhật số tiền hiện có lên thanh bar
        UpdateMoneyDisplay();

        // 4. Tự động hiển thị khẩu súng đầu tiên
        if (defaultWeapon != null)
        {
            SelectWeapon(defaultWeapon);
        }
    }

    // =========================================================================
    // 1. XỬ LÝ CHỌN SÚNG TỪ WEAPONDATA
    // =========================================================================
    public void SelectWeapon(WeaponData weapon)
    {
        if (weapon == null) return;
        currentWeapon = weapon;

        // Cập nhật tên súng
        if (gunNameText != null) 
            gunNameText.text = weapon.weaponName;

        // Cập nhật ảnh 2D to
        if (gunPreviewImage != null)
        {
            Sprite preview = (weapon.weaponPreview != null) ? weapon.weaponPreview : weapon.weaponIcon;
            if (preview != null)
            {
                gunPreviewImage.sprite = preview;
                gunPreviewImage.color = Color.white;
                gunPreviewImage.preserveAspect = true;
                gunPreviewImage.gameObject.SetActive(true);
            }
        }

        // Cập nhật thông số (không còn dấu %)
        if (statsText != null)
        {
            statsText.text = 
                $"DAMAGE :   {weapon.damage}\n" +
                $"RECOIL :   {weapon.recoil}\n" +
                $"MAG    :   {weapon.magSize}\n" +
                $"RESERVE:   {weapon.reserveAmmo}\n" +
                $"RELOAD :   {weapon.reloadTime}s";
        }

        // Cập nhật trạng thái hiển thị giữa Nút BUY và Image OWNER
        RefreshBuyButtonState();
    }

    // =========================================================================
    // 2. HÀM GẮN CHO ONCLICK CỦA BUTTON BUY
    // =========================================================================
    public void BuyCurrentGun()
    {
        if (currentWeapon == null) return;

        string unlockKey = currentWeapon.weaponName + "_Unlocked";
        bool isUnlocked = PlayerPrefs.GetInt(unlockKey, 0) == 1 || currentWeapon.price == 0;

        if (isUnlocked)
        {
            Debug.Log($"[Shop] Súng {currentWeapon.weaponName} đã sở hữu rồi!");
            return;
        }

        int currentMoney = PlayerPrefs.GetInt("Money", 0);

        if (currentMoney >= currentWeapon.price)
        {
            // Trừ tiền và lưu trạng thái sở hữu
            currentMoney -= currentWeapon.price;
            PlayerPrefs.SetInt("Money", currentMoney);
            PlayerPrefs.SetInt(unlockKey, 1);
            PlayerPrefs.Save();

            Debug.Log($"<color=green>[Shop] Mua thành công {currentWeapon.weaponName}! Tiền còn lại: {currentMoney}</color>");

            UpdateMoneyDisplay();
            RefreshBuyButtonState();
        }
        else
        {
            Debug.LogWarning("[Shop] Không đủ tiền để mua súng này!");
        }
    }

    // =========================================================================
    // 3. CHUYỂN ĐỔI TRẠNG THÁI HIỂN THỊ NÚT BUY / IMAGE OWNER
    // =========================================================================
    void RefreshBuyButtonState()
    {
        if (currentWeapon == null) return;

        string unlockKey = currentWeapon.weaponName + "_Unlocked";
        bool isUnlocked = PlayerPrefs.GetInt(unlockKey, 0) == 1 || currentWeapon.price == 0;

        if (isUnlocked)
        {
            // Đã sở hữu: Tắt nút BUY, Bật Image OWNER
            if (buyButton != null) buyButton.gameObject.SetActive(false);
            if (ownerObject != null) ownerObject.SetActive(true);
            if (priceText != null) priceText.text = "OWNED";
        }
        else
        {
            // Chưa sở hữu: Bật nút BUY, Tắt Image OWNER
            if (buyButton != null)
            {
                buyButton.gameObject.SetActive(true);
                int currentMoney = PlayerPrefs.GetInt("Money", 0);
                buyButton.interactable = (currentMoney >= currentWeapon.price);
            }

            if (ownerObject != null) ownerObject.SetActive(false);
            if (priceText != null) priceText.text = currentWeapon.price.ToString("N0");
        }
    }

    // Cập nhật hiển thị số tiền trên thanh Bar
    public void UpdateMoneyDisplay()
    {
        if (playerMoneyText != null)
        {
            playerMoneyText.text = PlayerPrefs.GetInt("Money", 0).ToString("N0");
        }
    }

    // =========================================================================
    // CÁC HÀM CẦU NỐI ĐỒNG BỘ DỮ LIỆU CŨ
    // =========================================================================
    public void SelectGun(WeaponData weapon)
    {
        SelectWeapon(weapon);
    }

    public void SelectGun(GunData gun)
    {
        if (gun == null) return;

        if (gunNameText != null) 
            gunNameText.text = gun.gunName;

        if (gunPreviewImage != null && gun.gunPreviewSprite != null)
        {
            gunPreviewImage.sprite = gun.gunPreviewSprite;
            gunPreviewImage.color = Color.white;
            gunPreviewImage.preserveAspect = true;
            gunPreviewImage.gameObject.SetActive(true);
        }

        if (statsText != null)
        {
            statsText.text = 
                $"DAMAGE :   {gun.damage}\n" +
                $"RECOIL :   {gun.recoilX}/{gun.recoilY}\n" +
                $"MAG    :   {gun.maxAmmo}\n" +
                $"RESERVE:   {gun.maxReserveAmmo}\n" +
                $"RELOAD :   {gun.reloadTime}s";
        }

        if (priceText != null)
            priceText.text = gun.Price.ToString("N0");
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GunShopUI : MonoBehaviour
{
    [Header("--- HIỂN THỊ CHÍNH ---")]
    public Image gunPreviewImage;   // Kéo Border Gun/RawImage vào đây
    public TMP_Text gunNameText;     // Kéo Name vào đây
    public TMP_Text statsText;       // Kéo Stats vào đây
    public TMP_Text priceText;       // Kéo Price vào đây

    // 1. Nhận dữ liệu từ WeaponData (cho 5 file mới trong WeaponsData)
    public void SelectWeapon(WeaponData weapon)
    {
        if (weapon == null) return;

        if (gunNameText != null) 
            gunNameText.text = weapon.weaponName;

        if (gunPreviewImage != null && weapon.weaponPreview != null)
        {
            gunPreviewImage.sprite = weapon.weaponPreview;
            gunPreviewImage.preserveAspect = true;
            gunPreviewImage.gameObject.SetActive(true);
        }

        if (statsText != null)
        {
            statsText.text = 
                $"DAMAGE :   {weapon.damage}\n" +
                $"RECOIL :   {weapon.recoil}%\n" +
                $"MAG    :   {weapon.magSize}\n" +
                $"RESERVE:   {weapon.reserveAmmo}\n" +
                $"RELOAD :   {weapon.reloadTime}s";
        }

        if (priceText != null)
            priceText.text = weapon.price.ToString("N0") + " $";
    }

    // 2. Hàm cầu nối xử lý lỗi cho GunItemUI (khi truyền WeaponData)
    public void SelectGun(WeaponData weapon)
    {
        SelectWeapon(weapon);
    }

    // 3. Hàm cầu nối xử lý lỗi cho GunItemUI (nếu truyền GunData cũ)
    public void SelectGun(GunData gun)
    {
        if (gun == null) return;

        if (gunNameText != null) 
            gunNameText.text = gun.gunName;

        if (gunPreviewImage != null && gun.gunPreviewSprite != null)
        {
            gunPreviewImage.sprite = gun.gunPreviewSprite;
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
            priceText.text = gun.Price.ToString("N0") + " $";
    }
}
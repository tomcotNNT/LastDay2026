using UnityEngine;

public class AmmoInventory : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Inventory")]
    [SerializeField] private InventoryData inventoryData;

    private int[] reserveAmmoByWeapon;
    private WeaponManager weaponManager;

    [Header("Grenade Data")]
    public int grenadeCount; // Số lượng lựu đạn lưu trữ trực tiếp trên RAM

    void Start()
    {
        grenadeCount = PlayerPrefs.GetInt("Grenade", 3);
        
        weaponManager = GetComponentInChildren<WeaponManager>();

        if (weaponManager == null)
        {
            Debug.LogError("Không tìm thấy WeaponManager");
            return;
        }

        reserveAmmoByWeapon = new int[weaponManager.weapons.Length];

        for (int i = 0; i < reserveAmmoByWeapon.Length; i++)
        {
            reserveAmmoByWeapon[i] = GetMaxReserveAmmo(i);
        }
    }

    public bool HasGrenade()
    {
        return grenadeCount > 0;
    }
    public void UseGrenade()
    {
        if (grenadeCount > 0)
        {
            grenadeCount--;
            // Cập nhật ngược lại PlayerPrefs để đồng bộ
            PlayerPrefs.SetInt("Grenade", grenadeCount);
        }
    }
// Hàm phục vụ tính năng nhặt đồ (Pickup) sau này
    public void AddGrenade(int amount)
    {
        grenadeCount += amount;
        PlayerPrefs.SetInt("Grenade", grenadeCount);
    }
    public void AddAmmo(int weaponIndex, int amount)
    {
        if (!IsValidWeaponIndex(weaponIndex))
            return;

        reserveAmmoByWeapon[weaponIndex] += amount;

        reserveAmmoByWeapon[weaponIndex] = Mathf.Min(
            reserveAmmoByWeapon[weaponIndex],
            GetMaxReserveAmmo(weaponIndex)
        );

        if (pickupSound != null && audioSource != null)
            audioSource.PlayOneShot(pickupSound);
    }

    public int GetAmmo(int weaponIndex)
    {
        if (!IsValidWeaponIndex(weaponIndex))
            return 0;

        return reserveAmmoByWeapon[weaponIndex];
    }

    public void UseAmmo(int weaponIndex, int amount)
    {
        if (!IsValidWeaponIndex(weaponIndex))
            return;

        reserveAmmoByWeapon[weaponIndex] -= amount;
        reserveAmmoByWeapon[weaponIndex] = Mathf.Max(reserveAmmoByWeapon[weaponIndex], 0);
    }

    public int GetMaxReserveAmmo(int weaponIndex)
    {
        if (weaponManager == null)
            return inventoryData != null ? inventoryData.maxReserveAmmo : 0;

        if (weaponIndex < 0 || weaponIndex >= weaponManager.weapons.Length)
            return inventoryData != null ? inventoryData.maxReserveAmmo : 0;

        GunSystem gunSystem = weaponManager.weapons[weaponIndex].GetComponent<GunSystem>();

        if (gunSystem == null || gunSystem.gunData == null)
            return inventoryData != null ? inventoryData.maxReserveAmmo : 0;

        return GunUpgradeCalculator.GetMaxReserveAmmo(gunSystem.gunData);
    }

    public void ClampAmmoToMax(int weaponIndex)
    {
        if (!IsValidWeaponIndex(weaponIndex))
            return;

        reserveAmmoByWeapon[weaponIndex] = Mathf.Min(
            reserveAmmoByWeapon[weaponIndex],
            GetMaxReserveAmmo(weaponIndex)
        );
    }

    private bool IsValidWeaponIndex(int index)
    {
        return reserveAmmoByWeapon != null &&
               index >= 0 &&
               index < reserveAmmoByWeapon.Length;
    }
}
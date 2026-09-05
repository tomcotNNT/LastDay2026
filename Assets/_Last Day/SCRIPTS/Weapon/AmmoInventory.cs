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

    void Start()
    {
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
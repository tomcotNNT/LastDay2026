using UnityEngine;

[CreateAssetMenu(fileName = "NewGun", menuName = "Gun/Gun Data")]
public class GunData : ScriptableObject
{   
    [Header("Inventory Data")]
    public InventoryData inventoryData; // Chứa dữ liệu kho đạn tham chiếu

    [Header("Info")]
    public string gunName;
    
    [Header("Shop")]
    public int previewIndex;
    public CurrencyType currencyType;
    public int Price;

    [Header("Damage")]
    public float damage = 20f;

    [Header("Fire")]
    public float fireRate = 0.1f;
    public bool isAutomatic = true;

    [Header("Reload")]
    public float reloadTime = 2f;

    [Header("Stability")]
    [Range(0, 100)]
    public float stability = 80f;
    public float range = 100f;

    [Header("Ammo")]
    public int maxAmmo = 30;
    public int maxReserveAmmo = 90; // <-- Đã bổ sung biến đạn dự trữ tại đây

    [Header("Bullet")]
    public float bulletSpeed = 100f;

    [Header("Audio")]
    public AudioClip singleShotClip;
    public AudioClip autoShotClip;
    public AudioClip reloadclip;

    [Header("Recoil")]
    public float recoilX = 1f;
    public float recoilY = 1f;

    [Header("Upgrade Per Level")]
    public GunUpgradeLevel[] upgradeLevels = new GunUpgradeLevel[4];
}
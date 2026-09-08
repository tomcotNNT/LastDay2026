using UnityEngine;

[CreateAssetMenu(fileName = "NewGunData", menuName = "Shop/Gun Data")]
public class GunData : ScriptableObject
{
    [Header("--- 1. THÔNG TIN SHOP (2D) ---")]
    public string gunName = "TÊN SÚNG";
    public Sprite gunPreviewSprite; // Kéo ảnh 2D to ở giữa vào đây
    public Sprite gunIcon;          // Kéo ảnh icon nhỏ trên thẻ súng vào đây
    public int Price = 1000;
    public CurrencyType currencyType = CurrencyType.Money;

    [Header("--- 2. CHỈ SỐ STATS TƯỢNG TRƯNG ---")]
    public float damage = 35f;
    public float recoilX = 1.2f;
    public float recoilY = 1.8f;
    public int maxAmmo = 30;
    public int maxReserveAmmo = 90;
    public float reloadTime = 2.5f;

    [Header("--- 3. GIỮ CHỖ HỆ THỐNG GỐC (DÙNG ĐÚNG TYPE CỦA PROJECT) ---")]
    public bool isAutomatic = true;
    public float range = 100f;
    public float bulletSpeed = 100f;
    public float fireRate = 0.1f;
    public AudioClip singleShotClip;
    public AudioClip autoShotClip;
    public AudioClip reloadclip;
    public AudioClip reloadClip => reloadclip;

    // Sử dụng class gốc đã có sẵn trong dự án:
    public InventoryData inventoryData;
    public GunUpgradeLevel[] upgradeLevels;

    [HideInInspector] public int previewIndex;
}
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Shop/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("General Info")]
    public string weaponName;
    public Sprite weaponIcon;      // Ảnh icon hiển thị ở nút bên trái
    public Sprite weaponPreview;   // Ảnh phóng to nét căng ở khung giữa

    [Header("Stats")]
    public float damage;
    public float recoil;
    public int magSize;
    public int reserveAmmo;
    public float reloadTime;

    [Header("Economy")]
    public int price;
    public bool isUnlocked;
}
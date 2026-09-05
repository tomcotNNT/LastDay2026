using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GunShopUI : MonoBehaviour
{
    public TMP_Text gunNameText;
    public TMP_Text statsText;
    public TMP_Text priceText;
    public Image currencyIcon;
    public Sprite moneySprite;
    public Sprite crystalSprite;
    public WeaponPreviewUI weaponPreviewUI;
    private GunData currentGun;

    public void SelectGun(GunData gun)
    {   
        currentGun = gun;
        gunNameText.text = gun.gunName;
        priceText.text = gun.Price.ToString();

        statsText.text =
            $"DAMAGE :    {gun.damage}\n" +
            $"RECOIL :    {gun.recoilX}/{gun.recoilY}\n" +
            $"MAG    :    {gun.maxAmmo}\n" +
            $"RESERVE :   {gun.inventoryData.maxReserveAmmo}\n" +
            $"RELOAD :    {gun.reloadTime}s";

        ShowCost(gun);

        weaponPreviewUI.ShowWeapon(gun.previewIndex);
    }

    void ShowCost(GunData gun)
    {
        priceText.text = gun.Price.ToString();

        switch (gun.currencyType)
        {
            case CurrencyType.Money:
                currencyIcon.sprite = moneySprite;
                Debug.Log("money");
                break;

            case CurrencyType.Crystal:
                currencyIcon.sprite = crystalSprite;
                Debug.Log("crystal");
                break;
        }
    }

    public void BuyCurrentGun()
    {
        if (currentGun == null) return;

        ShopManager.Instance.BuyGun(currentGun);
    }
}
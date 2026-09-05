using UnityEngine;
using UnityEngine.UI;

public class GunItemUI : MonoBehaviour
{
    public Button button;

    private GunData gunData;
    private GunShopUI shopUI;

    public void Setup(GunData gun, GunShopUI shop)
    {
        gunData = gun;
        shopUI = shop;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        shopUI.SelectGun(gunData);
    }
}
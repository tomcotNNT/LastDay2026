using UnityEngine;
public class GunButton : MonoBehaviour
{
    public GunData gunData;
    public GunShopUI shopUI;

    public void SelectGun()
    {
        shopUI.SelectGun(gunData);
    }
}
using UnityEngine;

public static class GunUpgradeCalculator
{
    public static int GetLevel(GunData gun)
    {
        return PlayerPrefs.GetInt(gun.gunName + "_Level", 0);
    }

    public static float GetDamage(GunData gun)
    {
        int level = GetLevel(gun);
        return gun.damage + gun.upgradeLevels[level].damageBonus;
    }

    public static float GetFireRate(GunData gun)
    {
        int level = GetLevel(gun);
        return gun.fireRate + gun.upgradeLevels[level].fireRateBonus;
    }

    public static int GetMaxAmmo(GunData gun)
    {
        int level = GetLevel(gun);
        return gun.maxAmmo + gun.upgradeLevels[level].magBonus;
    }

    public static int GetMaxReserveAmmo(GunData gun)
    {
        int level = GetLevel(gun);
        return gun.inventoryData.maxReserveAmmo + gun.upgradeLevels[level].reserveBonus;
    }

    public static float GetReloadTime(GunData gun)
    {
        int level = GetLevel(gun);
        return gun.reloadTime - gun.upgradeLevels[level].reloadBonus;
    }

    public static float GetRecoilX(GunData gun)
    {
        int level = GetLevel(gun);

        float value = gun.recoilX - gun.upgradeLevels[level].recoilBonus;

        return Mathf.Max(0f, value);
    }

    public static float GetRecoilY(GunData gun)
    {
        int level = GetLevel(gun);

        float value = gun.recoilY - gun.upgradeLevels[level].recoilBonus;

        return Mathf.Max(0f, value);
    }

    public static float GetRecoilDisplay(GunData gun, int level)
    {
        float x = gun.recoilX - gun.upgradeLevels[level].recoilBonus;
        float y = gun.recoilY - gun.upgradeLevels[level].recoilBonus;

        x = Mathf.Max(0f, x);
        y = Mathf.Max(0f, y);

        return (x + y) * 0.5f;
    }
}
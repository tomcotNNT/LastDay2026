using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    public FPSController fps;

    public void Fire(GunData gunData)
    {
        if (gunData == null || fps == null)
            return;

        float recoilX =
            GunUpgradeCalculator.GetRecoilX(gunData);

        float recoilY =
            GunUpgradeCalculator.GetRecoilY(gunData);

        float recoilHorizontal =
            Random.Range(-recoilY, recoilY);

        fps.wantedCameraXRotation -= recoilX;
        fps.wantedYRotation += recoilHorizontal;
    }
}
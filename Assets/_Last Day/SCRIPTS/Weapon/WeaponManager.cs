using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapons")]
    public GameObject[] weapons;

    [Header("Grenade")]
    public GrenadeSystem grenadeSystem;

    public int currentWeapon = -1;

    private PlayerInputActions input;

    void Awake()
    {
        input = new PlayerInputActions();
    }

    void OnEnable()
    {
        input.Enable();

        input.Player.Weapon1.performed += OnWeapon1;
        input.Player.Weapon2.performed += OnWeapon2;
        input.Player.Weapon3.performed += OnWeapon3;
        input.Player.ScrollWeapon.performed += OnScrollWeapon;
        input.Player.Grenade.performed += OnGrenade;
    }

    void OnDisable()
    {
        input.Player.Weapon1.performed -= OnWeapon1;
        input.Player.Weapon2.performed -= OnWeapon2;
        input.Player.Weapon3.performed -= OnWeapon3;
        input.Player.ScrollWeapon.performed -= OnScrollWeapon;
        input.Player.Grenade.performed -= OnGrenade;

        input.Disable();
    }

    void Start()
    {
        SelectWeapon(0);
    }

    public void SelectWeapon(int index)
    {
        if (Time.timeScale == 0f)
            return;

        if (index < 0 || index >= weapons.Length)
            return;

        if (index == currentWeapon)
        {
            GunSystem sameGun = weapons[index].GetComponentInChildren<GunSystem>(true);
            if (sameGun != null)
            {
                sameGun.SetWeaponIndex(index);
                sameGun.UpdateAmmoUI();
            }
            return;
        }

        if (IsCurrentWeaponReloading())
        {
            Debug.Log("Đang reload, không đổi súng");
            return;
        }

        for (int i = 0; i < weapons.Length; i++)
        {
            bool isSelected = i == index;
            weapons[i].SetActive(isSelected);

            GunSystem gun = weapons[i].GetComponentInChildren<GunSystem>(true);

            if (gun != null)
            {
                gun.SetWeaponIndex(i);

                if (!isSelected)
                    gun.CancelFire();
                else
                    gun.SetUIActive(true);
                    gun.UpdateAmmoUI();
            }
        }

        currentWeapon = index;

        if (grenadeSystem != null)
        {
            grenadeSystem.SetEquip(false);
        }
    }
    private bool IsCurrentWeaponReloading()
    {
        if (currentWeapon < 0 || currentWeapon >= weapons.Length)
            return false;

        GunSystem currentGun =
            weapons[currentWeapon].GetComponentInChildren<GunSystem>(true);

        return currentGun != null && currentGun.IsReloading();
    }

    void OnScrollWeapon(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0f)
            return;


        if (grenadeSystem != null)
        {
            grenadeSystem.SetEquip(false);
        }    

        Vector2 scroll = ctx.ReadValue<Vector2>();

        if (scroll.y == 0)
            return;

        int nextWeapon = currentWeapon;

        if (scroll.y > 0)
            nextWeapon++;
        else
            nextWeapon--;

        if (nextWeapon >= weapons.Length)
            nextWeapon = 0;

        if (nextWeapon < 0)
            nextWeapon = weapons.Length - 1;

        SelectWeapon(nextWeapon);
    }

    void OnWeapon1(InputAction.CallbackContext ctx)
    {
        SelectWeapon(0);
    }

    void OnWeapon2(InputAction.CallbackContext ctx)
    {
        SelectWeapon(1);
    }

    void OnWeapon3(InputAction.CallbackContext ctx)
    {
        SelectWeapon(2);
    }

    void OnGrenade(InputAction.CallbackContext context)
    {
        EquipGrenade();
    }

    void EquipGrenade()
    {
        if (Time.timeScale == 0) return;

        int grenadeAmount =
            PlayerPrefs.GetInt(
                "Grenade",
                0
            );

        if(grenadeAmount <=0)
            return;

        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(false);
        }

        currentWeapon = -1;

        if (grenadeSystem != null)
        {
            grenadeSystem.SetEquip(true);
        }
    }
}
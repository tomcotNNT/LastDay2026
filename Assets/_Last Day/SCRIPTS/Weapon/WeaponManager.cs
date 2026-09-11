using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [Header("3 Weapon Slots (1: Primary | 2: Secondary | 3: Melee)")]
    public GameObject[] weapons = new GameObject[3];

    [Header("Sub-Weapons & Equipments")]
    public GrenadeSystem grenadeSystem; // Phím 5
    public AmmoInventory ammoInventory;

    // Chỉ số súng đang cầm: 0 = Chính, 1 = Phụ, 2 = Cận chiến, -1 = Cầm đồ phụ trợ (Lựu đạn/Mìn)
    public int currentWeapon = -1;

    private PlayerInputActions input;
    private GunSystem[] cachedGuns;

    void Awake()
    {
        input = new PlayerInputActions();

        // Tự động tìm kho đạn trên Player nếu chưa kéo vào Inspector
        if (ammoInventory == null)
            ammoInventory = GetComponentInParent<AmmoInventory>();

        // Cache trước toàn bộ GunSystem để đạt tốc độ truy xuất O(1)
        if (weapons != null)
        {
            cachedGuns = new GunSystem[weapons.Length];
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null)
                    cachedGuns[i] = weapons[i].GetComponentInChildren<GunSystem>(true);
            }
        }
    }

    void OnEnable()
    {
        input.Enable();

        // Đăng ký trực tiếp cực kỳ gọn gàng, không cần viết hàm callback rác
        input.Player.Weapon1.performed += ctx => SelectWeapon(0); // Phím 1: Súng chính
        input.Player.Weapon2.performed += ctx => SelectWeapon(1); // Phím 2: Súng phụ
        input.Player.Weapon3.performed += ctx => SelectWeapon(2); // Phím 3: Cận chiến

        // input.Player.Weapon4.performed += ctx => EquipMine();    // Phím 4: Mở cmt khi cài Input Actions
        // input.Player.Weapon5.performed += ctx => EquipGrenade(); // Phím 5: Mở cmt khi cài Input Actions
        
        input.Player.Grenade.performed += ctx => EquipGrenade();  // Phím ném lựu đạn hiện tại
        input.Player.ScrollWeapon.performed += OnScrollWeapon;     // Cuộn chuột
    }

    void OnDisable()
    {
        input.Disable();
    }

    void Start()
    {
        SelectWeapon(0); // Mặc định rút súng chính khi vào trận
    }

    // ================= KHU VỰC CHUYỂN ĐỔI VŨ KHÍ CHÍNH (SLOT 1, 2, 3) =================
    public void SelectWeapon(int index)
    {
        if (Time.timeScale == 0f) return;
        if (index < 0 || index >= weapons.Length) return;

        // Nếu bấm đúng khẩu đang cầm thì chỉ cập nhật đạn, không chạy lại hoạt cảnh rút súng
        if (index == currentWeapon)
        {
            GunSystem sameGun = GetCachedGun(index);
            if (sameGun != null)
            {
                sameGun.SetWeaponIndex(index);
                sameGun.UpdateAmmoUI();
            }
            return;
        }

        // Chốt chặn an toàn: Đang nạp đạn thì khóa cứng, cấm đổi súng
        if (IsCurrentWeaponReloading())
        {
            Debug.Log("Đang nạp đạn, không thể đổi vũ khí!");
            return;
        }

        // Bật vũ khí được chọn, tắt các vũ khí còn lại
        for (int i = 0; i < weapons.Length; i++)
        {
            bool isSelected = (i == index);
            if (weapons[i] != null)
                weapons[i].SetActive(isSelected);

            GunSystem gun = GetCachedGun(i);
            if (gun != null)
            {
                gun.SetWeaponIndex(i);

                if (!isSelected)
                    gun.CancelFire();
                else
                {
                    gun.SetUIActive(true);
                    gun.UpdateAmmoUI();
                }
            }
        }

        currentWeapon = index;

        // Đổi súng thành công thì tự động cất lựu đạn vào túi
        if (grenadeSystem != null)
            grenadeSystem.SetEquip(false);
    }

    // ================= KHU VỰC TRANG BỊ PHỤ TRỢ (PHÍM 4 & PHÍM 5) =================

    // PHÍM 4: Ô CHỜ CHO MÌN / TRANG BỊ ĐẶT BẪY (Sẽ tích hợp sau)
    public void EquipMine()
    {
        if (Time.timeScale == 0f) return;
        if (IsCurrentWeaponReloading()) return;

        Debug.Log("Phím 4: Tính năng đặt mìn chưa trang bị!");
        // TODO: Sau này làm hệ thống mìn thì viết logic rút mìn tại đây
    }

    // PHÍM 5: RÚT LỰU ĐẠN RA TAY
    public void EquipGrenade()
    {
        if (Time.timeScale == 0f) return;

        // Chặn nếu đang bận nạp đạn
        if (IsCurrentWeaponReloading())
        {
            Debug.Log("Đang nạp đạn, không thể rút lựu đạn!");
            return;
        }

        // Kiểm tra túi đạn trên RAM
        if (ammoInventory != null && !ammoInventory.HasGrenade())
        {
            Debug.Log("Đã hết lựu đạn trong túi!");
            return;
        }

        // Cất toàn bộ súng chính/phụ/dao vào bao
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
                weapons[i].SetActive(false);
        }

        currentWeapon = -1; // Đặt về -1 để báo trạng thái đang cầm đồ phụ trợ

        if (grenadeSystem != null)
            grenadeSystem.SetEquip(true);
    }

    // ================= TIỆN ÍCH HỖ TRỢ =================
    private bool IsCurrentWeaponReloading()
    {
        if (currentWeapon < 0 || currentWeapon >= weapons.Length)
            return false;

        GunSystem currentGun = GetCachedGun(currentWeapon);
        return currentGun != null && currentGun.IsReloading();
    }

    private GunSystem GetCachedGun(int index)
    {
        if (cachedGuns != null && index >= 0 && index < cachedGuns.Length)
            return cachedGuns[index];
        return null;
    }

    void OnScrollWeapon(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0f) return;
        if (IsCurrentWeaponReloading()) return;

        Vector2 scroll = ctx.ReadValue<Vector2>();
        if (scroll.y == 0) return;

        int nextWeapon = currentWeapon;

        // Nếu vừa ném lựu đạn xong mà lăn chuột -> tự động quay về súng chính (Slot 0)
        if (nextWeapon == -1)
        {
            nextWeapon = 0;
        }
        else
        {
            if (scroll.y > 0) nextWeapon++;
            else nextWeapon--;

            // Vòng tròn lặp qua đúng 3 món: 0 -> 1 -> 2 -> 0
            if (nextWeapon >= weapons.Length) nextWeapon = 0;
            if (nextWeapon < 0) nextWeapon = weapons.Length - 1;
        }

        SelectWeapon(nextWeapon);
    }
}
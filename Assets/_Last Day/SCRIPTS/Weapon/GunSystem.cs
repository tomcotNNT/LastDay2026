using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GunSystem : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public AimSystem aimSystem;
    public Animator animator;
    public ParticleSystem muzzleFlash;

    private PlayerInputActions input;
    private bool isFiring;
    private bool isReloading;
    private float nextFireTime;

    public AudioSource gunAudio;
    public GunRecoil gunRecoil;

    // Loại bỏ biến dư thừa bulletPrefab (Tối ưu nạp bộ nhớ Scene):
    //[Header("Bullet")]
    //public GameObject bulletPrefab;

    [Header("Blood")]
    public ParticleSystem bloodPrefab;
    public Transform muzzle;

    [Header("Gun Data")]
    public GunData gunData;
    public int currentAmmo;

    private WeaponManager weaponManager;
    private int weaponIndex;
    private AmmoInventory inventory;

    [Header("Animation")]
    public AnimationClip reloadClip;

    [Header("UI")]
    public TMP_Text ammoText;
    public TMP_Text reserveAmmoText;
    public TMP_Text reloadTimeText;
    public GameObject bulletUI;
    public GameObject crosshairUI;

    [Header("Crosshair")]
    public GameObject normalCrosshair;
    public GameObject hitCrosshair;
    public float hitCrosshairTime = 0.1f;

    private Coroutine hitRoutine;

    void Awake()
    {
        input = new PlayerInputActions();
    }

    void Start()
    {
        if (gunAudio != null)
            gunAudio.playOnAwake = false;

        inventory = GetComponentInParent<AmmoInventory>();
        weaponManager = GetComponentInParent<WeaponManager>();

        currentAmmo = GetMaxAmmo();

        if (ammoText == null)
        {
            GameObject obj = GameObject.Find("AmmoText");
            if (obj != null)
                ammoText = obj.GetComponent<TMP_Text>();
        }

        if (reserveAmmoText == null)
        {
            GameObject obj = GameObject.Find("ReserveAmmoText");
            if (obj != null)
                reserveAmmoText = obj.GetComponent<TMP_Text>();
        }
        if (reloadTimeText != null)
            reloadTimeText.gameObject.SetActive(false);

        SetUIActive(true);
        UpdateAmmoUI();
    }

    void OnEnable()
    {
        EnsureUIReferences();
        SetUIActive(true);

        isReloading = false;
        isFiring = false;

        input.Enable();

        input.Player.Fire.performed += OnFirePressed;
        input.Player.Fire.canceled += OnFireReleased;
        input.Player.Reload.performed += OnReloadPressed;
    }

    void OnDisable()
    {
        input.Player.Fire.performed -= OnFirePressed;
        input.Player.Fire.canceled -= OnFireReleased;
        input.Player.Reload.performed -= OnReloadPressed;

        input.Disable();

        CancelFire();

        if (isReloading)
        {
            isReloading = false;

            if (reloadTimeText != null)
                reloadTimeText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        if (isReloading)
            return;

        if (gunData == null)
            return;

        if (gunData.isAutomatic && isFiring)
        {
            if (Time.time >= nextFireTime)
            {
                if (currentAmmo > 0)
                    Shoot();

                nextFireTime = Time.time + GetFireRate();
            }
        }

    }

    void OnFirePressed(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0f)
            return;

        if (isReloading)
            return;

        if (gunData == null)
            return;

        isFiring = true;

        if (!gunData.isAutomatic && currentAmmo > 0)
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + GetFireRate();
            }
        }
    }

    void OnFireReleased(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0f)
            return;

        CancelFire();
    }

    void OnReloadPressed(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0f || isReloading || gunData == null)
            return;

        // Không nạp nếu băng đạn đã đầy
        if (currentAmmo >= GetMaxAmmo())
            return;

        // Chặn không cho nạp nếu hết đạn dự trữ trong túi
        int reserveAmmo = (inventory != null) ? inventory.GetAmmo(weaponIndex) : 0;
        if (reserveAmmo <= 0)
        {
            Debug.Log("Hết đạn dự trữ, không thể nạp!");
            return;
        }

        StartCoroutine(Reload());
    }

    void Shoot()
    {
        if (!CanShoot())
            return;

        currentAmmo--;

        PlayShootEffect();
        ApplyRecoil();
        HandleHit();
        UpdateAmmoUI();
    }

    bool CanShoot()
    {
        return Time.timeScale > 0f &&
               !isReloading &&
               currentAmmo > 0 &&
               gunData != null;
    }

    void PlayShootEffect()
    {
        if (muzzleFlash != null)
            muzzleFlash.Play();

        if (gunAudio != null)
        {
            AudioClip clip = gunData.isAutomatic
                ? gunData.autoShotClip
                : gunData.singleShotClip;

            if (clip != null)
                gunAudio.PlayOneShot(clip);
        }

        if (animator != null)
        {
            if (gunData.isAutomatic)
                animator.SetBool("Auto", true);
            else
                animator.SetTrigger("Fire");
        }
    }

    void ApplyRecoil()
    {
        if (gunRecoil != null)
            gunRecoil.Fire(gunData);
    }

    void HandleHit()
    {
        if (cam == null)
            return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (!Physics.Raycast(ray, out RaycastHit hit, gunData.range))
            return;

        bool isHeadshot =
            hit.collider.CompareTag("EnemyHead") ||
            hit.collider.CompareTag("MutantHead");

        bool isBody =
            hit.collider.CompareTag("EnemyBody") ||
            hit.collider.CompareTag("MutantBody");

        if (!isHeadshot && !isBody)
            return;

        float damage = GetDamage();

        float finalDamage = isHeadshot
            ? damage * 3f
            : damage;

        EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();

        if (enemy != null)
        {
            enemy.TakeDamage(finalDamage, isHeadshot);
            SpawnBlood(hit);
            ShowHitMarker();
            return;
        }

        MutantHealth mutant = hit.collider.GetComponentInParent<MutantHealth>();

        if (mutant != null)
        {
            mutant.TakeDamage(finalDamage, isHeadshot);
            SpawnBlood(hit);
            ShowHitMarker();
        }
    }

    IEnumerator Reload()
    {
        if (isReloading || gunData == null) yield break;

        int maxAmmo = GetMaxAmmo();
        int reserve = (inventory != null) ? inventory.GetAmmo(weaponIndex) : 0;

        if (currentAmmo >= maxAmmo || reserve <= 0)
            yield break;

        isReloading = true;
        isFiring = false;

        if (reloadTimeText != null)
            reloadTimeText.gameObject.SetActive(true);

        float reloadTime = GetReloadTime();

        if (animator != null && reloadClip != null)
        {
            float reloadSpeed = reloadClip.length / reloadTime;
            animator.SetFloat("ReloadSpeed", reloadSpeed);
            animator.SetTrigger("Reload");
        }

        float timer = reloadTime;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (reloadTimeText != null)
                reloadTimeText.text = $"Reloading... {timer:F1}s";

            yield return null;
        }

        // Logic nạp đạn chuẩn: Chỉ nạp tối đa số đạn còn lại trong kho dự trữ
        int needAmmo = maxAmmo - currentAmmo;
        int ammoToAdd = Mathf.Min(needAmmo, reserve);

        currentAmmo += ammoToAdd;

        if (inventory != null)
        {
            inventory.UseAmmo(weaponIndex, ammoToAdd);
        }

        if (reloadTimeText != null)
            reloadTimeText.gameObject.SetActive(false);

        isReloading = false;

        UpdateAmmoUI();
    }

    void SpawnBlood(RaycastHit hit)
    {
        if (bloodPrefab == null)
            return;

        ParticleSystem blood = Instantiate(
            bloodPrefab,
            hit.point + hit.normal * 0.01f,
            Quaternion.LookRotation(hit.normal)
        );

        blood.transform.SetParent(hit.collider.transform);
        Destroy(blood.gameObject, 2f);
    }

    void ShowHitMarker()
    {
        if (aimSystem != null && aimSystem.isAiming)
            return;

        if (normalCrosshair == null || hitCrosshair == null)
            return;

        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        hitRoutine = StartCoroutine(ShowHitCrosshair());
    }

    IEnumerator ShowHitCrosshair()
    {
        normalCrosshair.SetActive(false);
        hitCrosshair.SetActive(true);

        yield return new WaitForSeconds(hitCrosshairTime);

        hitCrosshair.SetActive(false);
        normalCrosshair.SetActive(true);
    }

    public void SetWeaponIndex(int index)
    {
        weaponIndex = index;
    }

    public bool IsReloading()
    {
        return isReloading;
    }

    public void CancelFire()
    {
        isFiring = false;

        if (animator != null)
            animator.SetBool("Auto", false);
    }

    public void PlayReloadSound()
    {
        if (gunAudio != null && gunData != null && gunData.reloadclip != null)
            gunAudio.PlayOneShot(gunData.reloadclip);
    }

    public void UpdateAmmoUI()
    {
        if (gunData == null)
            return;

        if (ammoText != null)
            ammoText.text = $"{currentAmmo} / {GetMaxAmmo()}";

        if (reserveAmmoText != null && inventory != null)
            reserveAmmoText.text = inventory.GetAmmo(weaponIndex).ToString();
    }

    void EnsureUIReferences()
    {
        if (bulletUI == null)
        {
            GameObject obj = GameObject.Find("BulletUI");
            if (obj != null)
                bulletUI = obj;
        }

        if (crosshairUI == null)
        {
            GameObject obj = GameObject.Find("Crosshair");

            if (obj == null)
                obj = GameObject.Find("CrossHair");

            if (obj != null)
                crosshairUI = obj;
        }
    }

    public void SetUIActive(bool active)
    {
        if (bulletUI != null)
            bulletUI.SetActive(active);

        if (crosshairUI != null)
            crosshairUI.SetActive(active);
        else
        {
            if (normalCrosshair != null)
                normalCrosshair.SetActive(active);

            if (!active && hitCrosshair != null)
                hitCrosshair.SetActive(false);
        }
    }

    float GetDamage()
    {
        return GunUpgradeCalculator.GetDamage(gunData);
    }

    float GetFireRate()
    {
        return GunUpgradeCalculator.GetFireRate(gunData);
    }

    int GetMaxAmmo()
    {
        return GunUpgradeCalculator.GetMaxAmmo(gunData);
    }

    float GetReloadTime()
    {
        return GunUpgradeCalculator.GetReloadTime(gunData);
    }
}

/* Script GunSystem đóng vai trò là 
khối điều khiển vũ khí đơn lẻ (Weapon Controller),
 chịu trách nhiệm nhận lệnh bấm chuột/phím từ người chơi, 
 bắn đạn tức thời bằng tia Raycast, 
 trừ đạn, tính độ giật 
 và đồng bộ toàn bộ phản hồi nghe – nhìn (âm thanh, hoạt ảnh, tia lửa, giao diện). */

 // Bắn đạn tức thời (Hitscan Shooting):
 // Phân biệt sát thương & Headshot:
 // Chế độ bắn kép:
 // Nạp đạn theo thời gian thực:
 // Phản hồi nghe - nhìn & Hitmarker:

 // Vị trí gắn: Gắn trực tiếp trên từng GameObject súng cụ thể

 /* Nguồn Nhận Dữ Liệu
GunData (ScriptableObject): Chứa thông số nguyên bản của súng (băng đạn gốc, tốc độ bắn, độ giật, tầm xa, âm thanh nạp đạn).
GunUpgradeCalculator (Static Class): Nhận GunData và tính toán lại các chỉ số sau khi người chơi đã nâng cấp tại xưởng Store (sát thương thực tế, thời gian nạp nhanh hơn, băng đạn dài hơn).
PlayerInputActions (New Input System): Đọc tín hiệu bấm chuột trái (Fire) và phím R (Reload).
AmmoInventory: Cung cấp số lượng đạn dự trữ đang còn trong túi đồ của người chơi.
 */
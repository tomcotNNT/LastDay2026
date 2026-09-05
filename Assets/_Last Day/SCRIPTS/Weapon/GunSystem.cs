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

    [Header("Bullet")]
    public GameObject bulletPrefab;

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

        if (reloadTimeText == null)
        {
            TMP_Text[] allTexts = Resources.FindObjectsOfTypeAll<TMP_Text>();

            foreach (TMP_Text text in allTexts)
            {
                if (text.name == "ReloadTimeText")
                {
                    reloadTimeText = text;
                    break;
                }
            }
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

        UpdateAmmoUI();
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

    // --- TẠO BUG: Bỏ qua kiểm tra kho đạn dự trữ, cho phép nhấn R chạy nạp đạn vô điều kiện ---
    void OnReloadPressed(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0f)
            return;

        if (isReloading)
            return;

        // Chỉ cần đạn hiện tại ít hơn đạn tối đa là cho phép nạp, không quan tâm kho đạn dự trữ còn hay hết
        if (currentAmmo < GetMaxAmmo())
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

    // --- TẠO BUG: Sửa logic Coroutine để ép súng tự sinh đạn vô hạn khi kết thúc hoạt ảnh ---
    IEnumerator Reload()
    {
        if (isReloading) yield break;
        if (gunData == null) yield break;

        int maxAmmo = GetMaxAmmo();

        if (currentAmmo >= maxAmmo)
            yield break;

        // Bỏ qua kiểm tra dòng lệnh inventory.GetAmmo <= 0 để hoạt ảnh và text chạy bất kể kho đạn trống
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

        // BIẾN ĐỔI GÂY BUG: Ép súng tự động hồi đầy băng đạn (refill) mà không cần trừ đạn từ kho dự trữ
        int needAmmo = maxAmmo - currentAmmo;
        
        // Tự gán đầy băng đạn luôn (Hành vi hack đạn ảo từ hư vô)
        currentAmmo += needAmmo; 

        // Nếu có inventory thì chỉ trừ theo logic toán học cũ nhưng không chặn dòng trên, 
        // dẫn đến việc kho dự trữ bằng 0 vẫn nạp đầy súng thành công (Bug đạn vô hạn)
        if (inventory != null)
        {
            int reserve = inventory.GetAmmo(weaponIndex);
            int ammoToSubtract = Mathf.Min(needAmmo, reserve);
            inventory.UseAmmo(weaponIndex, ammoToSubtract);
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
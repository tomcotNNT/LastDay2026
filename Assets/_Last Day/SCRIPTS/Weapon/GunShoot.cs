using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunShoot : MonoBehaviour
{
    [Header("References")]
    // camera FPS
    public Camera cam;
    public AimSystem aimSystem;

    // animator súng
    public Animator animator;

    // muzzle flash
    public ParticleSystem muzzleFlash;

    // input system
    private PlayerInputActions input;

    // đang giữ chuột
    private bool isFiring;

    // đang thay đạn
    private bool isReloading;

    // cooldown bắn
    private float nextFireTime;
    public AudioSource gunAudio;
    public GunRecoil gunRecoil;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform muzzle;

    [Header("Gun Data")]
    public GunData gunData;
    public int currentAmmo;
    public int reserveAmmo;

    [Header("Animation")]
    public AnimationClip reloadClip;

    [Header("Impact")]
    public GameObject impactPrefab;   // Prefab Decal/Vết đạn Quad
    public float impactLifetime = 2f; // Biến mất sau bao giây

    void Awake()
    {
        input = new PlayerInputActions();
        gunRecoil = GetComponent<GunRecoil>();
    }

    void Start()
    {
        if (gunAudio != null)
        {
            gunAudio.playOnAwake = false;
        }

        if (gunData != null)
        {
            currentAmmo = gunData.maxAmmo;
            reserveAmmo = gunData.maxReserveAmmo;
        }
    }

    void OnEnable()
    {
        input.Enable();

        // Nhấn chuột / phím
        input.Player.Fire.performed += OnFirePressed;
        input.Player.Reload.performed += OnReloadPressed;

        // Thả chuột
        input.Player.Fire.canceled += OnFireReleased;
    }

    void OnDisable()
    {
        input.Player.Fire.performed -= OnFirePressed;
        input.Player.Reload.performed -= OnReloadPressed;
        input.Player.Fire.canceled -= OnFireReleased;
        input.Disable();
    }

    void Update()
    {
        if (isReloading)
        {
            return;
        }
        
        // Bắn liên thanh (Auto)
        if (gunData != null && gunData.isAutomatic && isFiring)
        {
            if (Time.time >= nextFireTime)
            {
                if (currentAmmo > 0)
                {
                    Shoot();
                }

                nextFireTime = Time.time + gunData.fireRate;
            }
        }
    }

    void OnFirePressed(InputAction.CallbackContext ctx)
    {
        if (isReloading) return;

        isFiring = true;

        // Bắn phát một (Single)
        if (gunData != null && !gunData.isAutomatic && currentAmmo > 0)
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + gunData.fireRate;
            }
        }
    }

    void OnReloadPressed(InputAction.CallbackContext ctx)
    {
        if (gunData == null) return;

        if (currentAmmo < gunData.maxAmmo && !isReloading && reserveAmmo > 0)
        {
            StartCoroutine(Reload());
        }
    }

    void OnFireReleased(InputAction.CallbackContext ctx)
    {
        isFiring = false;

        if (animator != null)
        {
            animator.SetBool("Auto", false);
        }
    }

    void Shoot()
    {
        if (gunData == null) return;

        RaycastHit hit;

        // 1. Sửa cam.position / cam.forward thành cam.transform và dùng gunData.range
        if (cam != null && Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, gunData.range))
        {
            if (impactPrefab != null)
            {
                GameObject impact = Instantiate(
                    impactPrefab,
                    hit.point,
                    Quaternion.LookRotation(hit.normal)
                );
                Destroy(impact, impactLifetime);
            }
        }

        // Trừ đạn
        currentAmmo--;

        // Muzzle Flash
        if (muzzleFlash != null) muzzleFlash.Play();

        // Âm thanh
        if (gunAudio != null)
        {
            AudioClip clipToPlay = gunData.isAutomatic ? gunData.autoShotClip : gunData.singleShotClip;
            if (clipToPlay != null) gunAudio.PlayOneShot(clipToPlay);
        }

        // Animation
        if (animator != null)
        {
            if (gunData.isAutomatic) 
            {
                animator.SetBool("Auto", true);
            }
            else
            {
                animator.SetTrigger("Fire");
            }
        }

        // 2. Sửa truyền tham số gunData vào hàm Fire() của GunRecoil
        if (gunRecoil != null)
        {
            gunRecoil.Fire(gunData);
        }

        // Hướng bắn
        Vector3 fireOrigin;
        Vector3 fireDirection;

        if (aimSystem != null)
        {
            fireOrigin    = aimSystem.FirePoint;
            fireDirection = aimSystem.FireDirection;
        }
        else if (cam != null && muzzle != null)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            
            // 3. Sửa trùng khai báo 'RaycastHit hit' bằng cách dùng out RaycastHit rayHit
            Vector3 target = Physics.Raycast(ray, out RaycastHit rayHit, 1000f)
                ? rayHit.point
                : ray.GetPoint(1000f);

            fireOrigin    = muzzle.position;
            fireDirection = (target - muzzle.position).normalized;
        }
        else
        {
            return;
        }

        // Spawn đạn
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(
                bulletPrefab,
                fireOrigin,
                Quaternion.LookRotation(fireDirection)
            );

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = fireDirection * gunData.bulletSpeed;
            }
        }
    }

    IEnumerator Reload()
    {
        if (isReloading || gunData == null)
            yield break;

        if (currentAmmo >= gunData.maxAmmo || reserveAmmo <= 0)
            yield break;

        isReloading = true;

        if (animator != null && reloadClip != null)
        {
            float animLength = reloadClip.length;
            float reloadSpeed = animLength / gunData.reloadTime;
            animator.SetFloat("ReloadSpeed", reloadSpeed);
            animator.SetTrigger("Reload");
        }

        yield return new WaitForSeconds(gunData.reloadTime);

        int needAmmo = gunData.maxAmmo - currentAmmo;
        int ammoToLoad = Mathf.Min(needAmmo, reserveAmmo);

        currentAmmo += ammoToLoad;
        reserveAmmo -= ammoToLoad;

        isReloading = false;
    }

    public void PlayReloadSound()
    {
        if (gunAudio != null && gunData != null && gunData.reloadclip != null)
        {
            gunAudio.PlayOneShot(gunData.reloadclip);
        }
    }
}
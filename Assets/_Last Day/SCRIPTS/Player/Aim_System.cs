using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AimSystem : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Camera CamReddot;

    [Header("Aim Point")]
    public Transform aimPoint; // đặt ở tâm ống ngắm
    public Animator animator;
    public Transform muzzle;           // đầu nòng súng
    public Image crosshairImage;       // UI crosshair

    [Header("Settings")]
    public float range = 1000f;

    private PlayerInputActions input;
    public bool isAiming;

    // điểm bắn — các script khác lấy từ đây
    public Vector3 FirePoint { get; private set; }
    public Vector3 FireDirection { get; private set; }

    // =====================================================
    // AWAKE
    // =====================================================
    void Awake()
    {
        input = new PlayerInputActions();
    }

    // =====================================================
    // ENABLE / DISABLE
    // =====================================================
    void OnEnable()
    {
        input.Enable();
        input.Player.Aim.performed += StartAim;
        input.Player.Aim.canceled  += StopAim;
    }

    void OnDisable()
    {
        input.Player.Aim.performed -= StartAim;
        input.Player.Aim.canceled  -= StopAim;
        input.Disable();
    }

    // =====================================================
    // UPDATE
    // =====================================================
    void Update()
    {
        UpdateFireData();
    }

    // =====================================================
    // FIRE DATA
    // tính điểm bắn theo chế độ hip / aim
    // =====================================================
   
    void UpdateFireData()
    {
        Ray camRay = cam.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 targetPoint;

        if (Physics.Raycast(camRay, out RaycastHit hit, range))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = camRay.GetPoint(range);
        }

        FirePoint = muzzle.position;

        FireDirection =
            (targetPoint - muzzle.position).normalized;

        Debug.DrawRay(
            FirePoint,
            FireDirection * 100f,
            Color.red
        );
    }

    // =====================================================
    // START AIM
    // =====================================================
    void StartAim(InputAction.CallbackContext ctx)
    {
        isAiming = true;

        // ẩn crosshair khi ADS
        if (crosshairImage != null)
            crosshairImage.enabled = false;

        if (animator != null)
            animator.SetBool("Aim", true);

        //fov 40
        if (CamReddot != null)
            CamReddot.fieldOfView = 40f;
    }

    // =====================================================
    // STOP AIM
    // =====================================================
    void StopAim(InputAction.CallbackContext ctx)
    {
        isAiming = false;

        // hiện crosshair khi thôi ADS
        if (crosshairImage != null)
            crosshairImage.enabled = true;

        if (animator != null)
            animator.SetBool("Aim", false);

        //fov 60
        if (CamReddot != null)
            CamReddot.fieldOfView = 60f;
    }


}
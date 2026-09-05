using UnityEngine;
using UnityEngine.InputSystem;

public class GrenadeSystem : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform throwPoint;
    public GameObject grenadePrefab;

    public GrenadeUI UI;

    [Header("Throw Setting")]
    public float throwForce = 15f;
    public float upwardForce = 3f;

    [Header("Hand Grenade")]
    public GameObject grenadeInHand;

    private PlayerInputActions input;
    private bool isEquipped;

    private int grenadeAmount;

    void Awake()
    {
        input = new PlayerInputActions();
    }

    void Start()
    {
        grenadeAmount =
            PlayerPrefs.GetInt(
                "Grenade",
                0
            );

        UI.UpdateGrenadeUI();   

        UpdateHandGrenade();
    }

    void OnEnable()
    {
        input.Enable();
        input.Player.Fire.performed += OnThrow;
    }

    void OnDisable()
    {
        input.Player.Fire.performed -= OnThrow;
        input.Disable();
    }

    public void SetEquip(bool value)
    {
        isEquipped = value;
        gameObject.SetActive(value);
        UpdateHandGrenade();
    }

    void UpdateHandGrenade()
    {
        int grenadeCount = PlayerPrefs.GetInt("Grenade", 0);

        if (grenadeInHand != null)
        {
            grenadeInHand.SetActive(
                isEquipped && grenadeCount > 0
            );
        }
    }

    void OnThrow(InputAction.CallbackContext context)
    {
        if (!isEquipped) return;
        if (grenadeAmount <= 0) return;
        if (Time.timeScale == 0) return;

        ThrowGrenade();
        
    }

    void ThrowGrenade()
    {
        grenadeAmount--;

        UI.UpdateGrenadeUI();

        PlayerPrefs.SetInt("Grenade", grenadeAmount);

        if(UI != null)
        {
            UI.UpdateGrenadeUI();
        }

        GameObject grenade = Instantiate(
            grenadePrefab,
            throwPoint.position,
            throwPoint.rotation
        );

        Rigidbody rb = grenade.GetComponent<Rigidbody>();

        Vector3 throwDir = cam.transform.forward;
        Vector3 force = throwDir * throwForce + Vector3.up * upwardForce;

        rb.AddForce(force, ForceMode.Impulse);
    }
}
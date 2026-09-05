using UnityEngine;
using UnityEngine.InputSystem;

public class MinimapManager : MonoBehaviour
{
    public GameObject minimapSmall;
    public GameObject mapBig;

    private bool isBigMapOpen;
    private PlayerInputActions input;

    void Awake()
    {
        input = new PlayerInputActions();

        //tắt map big
        mapBig.SetActive(false);
    }

    void OnEnable()
    {
        input.Enable();
        input.Player.Map.performed += ToggleMap;
    }

    void OnDisable()
    {
        input.Player.Map.performed -= ToggleMap;
        input.Disable();
    }

    void ToggleMap(InputAction.CallbackContext ctx)
    {
        isBigMapOpen = !isBigMapOpen;

        minimapSmall.SetActive(!isBigMapOpen);
        mapBig.SetActive(isBigMapOpen);

        Cursor.visible = isBigMapOpen;
        Cursor.lockState = isBigMapOpen
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }
}
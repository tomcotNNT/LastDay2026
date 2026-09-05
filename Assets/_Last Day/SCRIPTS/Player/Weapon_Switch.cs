using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAnimationSwitcher : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private PlayerInputActions inputActions;

    private bool hasWeapon;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.SwitchWeapon.performed += OnSwitchWeapon;
    }

    private void OnDisable()
    {
        inputActions.Player.SwitchWeapon.performed -= OnSwitchWeapon;

        inputActions.Player.Disable();
    }

    private void OnSwitchWeapon(InputAction.CallbackContext context)
    {
        hasWeapon = !hasWeapon;

        animator.SetBool("HasAK", hasWeapon);
    }
}
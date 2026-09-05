using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MacheteSystem : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    private PlayerInputActions inputActions;
    private AudioSource audioSource;
    public AudioClip slideClip;
    private bool isAttacking = false;

    void Reset()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        if (inputActions == null)
            inputActions = new PlayerInputActions();

        inputActions.Player.Fire.performed += OnAttack;
        inputActions.Player.Enable();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>() ?? GetComponentInParent<AudioSource>();
        if (audioSource != null)
            audioSource.playOnAwake = false;
    }

    void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Player.Fire.performed -= OnAttack;
            inputActions.Player.Disable();
        }
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (isAttacking) return;

        StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        if (animator != null)
            animator.SetTrigger("Slide");

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>() ?? GetComponentInParent<AudioSource>();

        if (audioSource != null && slideClip != null)
            audioSource.PlayOneShot(slideClip);

        yield return new WaitForSeconds(0.5f); // thời gian animation chém

        isAttacking = false;
    }
}

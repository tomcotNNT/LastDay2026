using UnityEngine;

public class FirstKitPickup : MonoBehaviour, IInteractable
{
    [Header("Data")]
    public ItemData firstKitData;

    [Header("Rotate")]
    public float rotateSpeed = 50f;

    void Update()
    {
        // xoay quanh trục y
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    public void Interact(PlayerInteract player)
    {
        if (firstKitData == null)
            return;

        Health health = player.GetComponentInChildren<Health>();

        if (health == null)
            return;

        health.Heal(firstKitData.healthRestore);

        Destroy(gameObject);
    }
}
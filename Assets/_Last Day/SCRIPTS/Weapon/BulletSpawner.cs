using System.Collections;
using UnityEngine;

public class AmmoSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Ammo Prefab")]
    public GameObject ammoPrefab;

    [Header("Respawn Time")]
    public float respawnTime = 15f;

    // lưu ammo hiện tại ở mỗi point
    private AmmoPickup[] currentAmmo;

    void Start()
    {
        currentAmmo = new AmmoPickup[spawnPoints.Length];

        // spawn ban đầu
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            SpawnAmmo(i);
        }

        StartCoroutine(CheckRespawn());
    }

    void SpawnAmmo(int index)
    {
        GameObject obj = Instantiate(
            ammoPrefab,
            spawnPoints[index].position,
            spawnPoints[index].rotation
        );

        currentAmmo[index] = obj.GetComponent<AmmoPickup>();
    }

    IEnumerator CheckRespawn()
    {
        while (true)
        {
            for (int i = 0; i < currentAmmo.Length; i++)
            {
                // đã bị nhặt hoặc bị destroy
                if (currentAmmo[i] == null)
                {
                    yield return new WaitForSeconds(respawnTime);

                    SpawnAmmo(i);
                }
            }

            yield return null;
        }
    }
}
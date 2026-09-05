using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AmbientZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;

    public List<Transform> spawnPoints;

    public float spawnInterval=5 ;

    private bool spawning;
    public float maxZombie;

    // =====================================================
    // START
    // =====================================================

    public void StartSpawn()
    {
        spawning = true;

        InvokeRepeating(
            nameof(SpawnZombie),
            0f,
            spawnInterval
        );

        Debug.Log("Ambient Spawner Started");
        
    }

    // =====================================================
    // STOP
    // =====================================================

    public void StopSpawn()
    {
        spawning = false;

        CancelInvoke();
    }

    // =====================================================
    // SPAWN
    // =====================================================

    void SpawnZombie()
    {
        if (!spawning)
            return;

        // Đã đủ số lượng zombie
        int currentZombie =
            GameObject.FindGameObjectsWithTag("Enemy").Length;

        if (currentZombie >= maxZombie)
        {
            Debug.Log("Đạt max zombie: " + currentZombie);
            return;
        }

        Transform point =
            spawnPoints[
                Random.Range(0, spawnPoints.Count)
            ];

        if (NavMesh.SamplePosition(
            point.position,
            out NavMeshHit hit,
            3f,
            NavMesh.AllAreas))
        {
            GameObject zombie = Instantiate(
                zombiePrefab,
                hit.position,
                Quaternion.identity
            );

            EnemyAI ai = zombie.GetComponent<EnemyAI>();

            if (ai != null)
            {
                ai.FindClosestTarget();
                ai.SetChaseState();
            }

            Debug.Log("Zombie Spawned");
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform firePoint;
    public GameObject bulletPrefab;
    public Animator animator;

    [Header("Detection")]
    public LayerMask enemyLayer;
    public float attackRange = 10f;
    public float detectInterval = 0.2f;

    [Header("Movement")]
    public float followDistance = 3f;
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 10f;
    public float playerAreaRadius = 12f;
    public string speedParam = "Speed";

    [Header("Dodge")]
    // public float safeDistance = 3f;//
    public float dodgeDistance = 5f;

    [Header("Combat")]
    public float shootInterval = 1f;
    public string shootTrigger = "Shoot";
    public GunData gunData;
    public float farDamageMultiplier = 0.5f;
    public float nearDamageRange = 0f;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public AudioSource gunAudio;

    private List<Transform> targets = new List<Transform>();
    private Transform currentTarget;
    private NavMeshAgent agent;
    private float lastShootTime;
    private float detectTimer;

    void Start()
    {
        FindPlayer();

        if (nearDamageRange <= 0f)
            nearDamageRange = attackRange * 0.5f;

        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
            agent = gameObject.AddComponent<NavMeshAgent>();

        agent.speed = moveSpeed;
        agent.angularSpeed = rotationSpeed * 100f;
        agent.autoBraking = true;
        agent.updateRotation = true;
    }

    void Update()
    {
        if (player == null)
            FindPlayer();

        if (player == null)
            return;

        detectTimer -= Time.deltaTime;

        if (detectTimer <= 0f)
        {
            DetectEnemies();
            detectTimer = detectInterval;
        }

        CleanupTargets();

        // if (HasDangerEnemy())
        // {
        //     SmartDodge();
        //     UpdateAnimation();
        //     return;
        // }

        if (currentTarget != null)
        {
            Combat();
        }
        else
        {
            FollowPlayer();
        }

        UpdateAnimation();
    }

    void DetectEnemies()
    {
        targets.Clear();

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            attackRange,
            enemyLayer
        );

        foreach (Collider hit in hits)
        {
            Transform enemyRoot = hit.transform.root;

            if (!targets.Contains(enemyRoot))
                targets.Add(enemyRoot);
        }

        currentTarget = GetClosestTarget();
    }

    void Combat()
    {
        if (currentTarget == null)
            return;

        float distanceToTarget =
            Vector3.Distance(transform.position, currentTarget.position);

        if (distanceToTarget <= attackRange)
        {
            agent.ResetPath();

            Vector3 targetPos = GetTargetPosition(currentTarget);

            RotateToward(targetPos);
            ShootAt(currentTarget);
        }
        else
        {
            agent.SetDestination(currentTarget.position);
        }
    }

    void FollowPlayer()
    {
        float distanceToPlayer = Vector3.Distance(
            transform.position,
            player.position
        );

        if (distanceToPlayer > followDistance)
        {
            Vector3 dir = (transform.position - player.position).normalized;
            Vector3 followPos = player.position + dir * followDistance;

            if (NavMesh.SamplePosition(
                followPos,
                out NavMeshHit hit,
                3f,
                NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            agent.ResetPath();
        }
    }

    // bool HasDangerEnemy()
    // {
    //     foreach (Transform enemy in targets)
    //     {
    //         if (enemy == null)
    //             continue;

    //         float dist = Vector3.Distance(
    //             transform.position,
    //             enemy.position
    //         );

    //         if (dist <= safeDistance)
    //             return true;
    //     }

    //     return false;
    // }

    // void SmartDodge()
    // {
    //     Vector3 fleeDir = Vector3.zero;

    //     foreach (Transform enemy in targets)
    //     {
    //         if (enemy == null)
    //             continue;

    //         Vector3 away = transform.position - enemy.position;
    //         float dist = away.magnitude;

    //         if (dist > safeDistance || dist <= 0.1f)
    //             continue;

    //         float weight = 1f / dist;
    //         fleeDir += away.normalized * weight;
    //     }

    //     if (fleeDir == Vector3.zero)
    //         return;

    //     Vector3 targetPos =
    //         transform.position + fleeDir.normalized * dodgeDistance;

    //     targetPos = ClampPositionToPlayerRadius(targetPos);

    //     if (NavMesh.SamplePosition(
    //         targetPos,
    //         out NavMeshHit hit,
    //         dodgeDistance,
    //         NavMesh.AllAreas))
    //     {
    //         agent.SetDestination(hit.position);
    //     }
    // }

    void ShootAt(Transform target)
    {
        if (target == null || bulletPrefab == null || firePoint == null)
            return;

        if (Time.time - lastShootTime < shootInterval)
            return;

        lastShootTime = Time.time;

        if (animator != null)
            animator.SetTrigger(shootTrigger);

        if (muzzleFlash != null)
            muzzleFlash.Play();

        if (gunAudio != null && gunData != null && gunData.singleShotClip != null)
            gunAudio.PlayOneShot(gunData.singleShotClip);

        Vector3 targetPos = GetTargetPosition(target);

        Vector3 dir =
            (targetPos - firePoint.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            rot
        );

        float damage = gunData != null ? gunData.damage : 10f;
        float bulletSpeed = gunData != null ? gunData.bulletSpeed : 20f;

        float distanceToTarget = Vector3.Distance(
            firePoint.position,
            target.position
        );

        if (distanceToTarget > nearDamageRange)
            damage *= farDamageMultiplier;

        ProjectileHandler projectileHandler =
            bullet.GetComponent<ProjectileHandler>();

        if (projectileHandler != null)
            projectileHandler.damage = damage;

        Rigidbody rb = bullet.GetComponent<Rigidbody>();

        if (rb != null)
            rb.linearVelocity = dir * bulletSpeed;
    }

    Vector3 GetTargetPosition(Transform target)
    {
        if (target == null)
            return Vector3.zero;

        EnemyHealth enemy = target.GetComponent<EnemyHealth>();
        if (enemy != null && enemy.aimPoint != null)
            return enemy.aimPoint.position;

        MutantHealth mutant = target.GetComponent<MutantHealth>();
        if (mutant != null && mutant.aimPoint != null)
            return mutant.aimPoint.position;

        return target.position + Vector3.up * 1.2f;
    }

    Transform GetClosestTarget()
    {
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (Transform target in targets)
        {
            if (target == null)
                continue;

            if (target.gameObject.layer == LayerMask.NameToLayer("Dead"))
                continue;

            float dist = Vector3.Distance(
                transform.position,
                target.position
            );

            if (dist < minDist)
            {
                minDist = dist;
                closest = target;
            }
        }

        return closest;
    }

    void CleanupTargets()
    {
        int deadLayer = LayerMask.NameToLayer("Dead");

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (targets[i] == null)
            {
                targets.RemoveAt(i);
                continue;
            }

            if (deadLayer != -1 &&
                targets[i].gameObject.layer == deadLayer)
            {
                targets.RemoveAt(i);
            }
        }

        currentTarget = GetClosestTarget();
    }

    Vector3 ClampPositionToPlayerRadius(Vector3 position)
    {
        Vector3 offset = position - player.position;

        if (offset.magnitude <= playerAreaRadius)
            return position;

        return player.position + offset.normalized * playerAreaRadius;
    }

    void RotateToward(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion rot = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            rot,
            rotationSpeed * Time.deltaTime * 100f
        );
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            player = p.transform;
    }

    void UpdateAnimation()
    {
        if (animator != null && agent != null)
            animator.SetFloat(speedParam, agent.velocity.magnitude);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Gizmos.color = Color.red;
        // Gizmos.DrawWireSphere(transform.position, safeDistance);

        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, playerAreaRadius);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, followDistance);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, nearDamageRange);
    }
}
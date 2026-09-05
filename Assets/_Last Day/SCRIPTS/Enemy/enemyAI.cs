using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("References")]
    public Transform target;
    public NavMeshAgent agent;
    public Animator animator;
    public GameObject BloodScreenObj;
    private GameObject InstantiatedObj;
    private Coroutine bloodCoroutine;
    public ZombieData zombieData;

    [Header("Patrol")]
    public float patrolWaitTime = 2f;

    private Transform patrolCenter;
    private PatrolCenter patrolCenterData;
    private int currentWaypointIndex = 0;
    private Vector3 patrolPoint;

    [Header("Detection")]
    public float detectionRange = 15f;
    public float loseRange = 20f;

    private float attackRangeSqr;
    private float loseRangeSqr;
    private float detectionRangeSqr;

    [Header("Attack")]
    public float attackRange = 2f;
    public float attackCooldown = 2f;

    [Header("Audio")]
    public AudioSource audioSource;

    private EnemyState currentState;

    private float attackTimer;

    private bool waiting;

    private bool isInitialized = false;

    void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }
    void Start()
    {

        attackRangeSqr = attackRange * attackRange;
        loseRangeSqr = loseRange * loseRange;
        detectionRangeSqr = detectionRange * detectionRange;

        agent.updateRotation = false;

        FindClosestTarget();

        StartCoroutine(InitWhenReady());

        currentState = EnemyState.Chase;
    }

    void Update()
    {
        if (!isInitialized || target == null)
        {
            FindClosestTarget();
            return;
        }

        attackTimer += Time.deltaTime;

        float sqrDistance =
        (target.position - transform.position)
        .sqrMagnitude;

        UpdateState(sqrDistance);

        switch (currentState)
        {
            case EnemyState.Chase:
                Chase();
                break;

            case EnemyState.Attack:
                Attack();
                break;
        }

        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    public void FindClosestTarget()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        GameObject hunterObj = GameObject.FindGameObjectWithTag("NPC");

        if (playerObj == null && hunterObj == null)
            return;

        if (playerObj != null && hunterObj != null)
        {
            float playerDistSqr =
                (playerObj.transform.position - transform.position).sqrMagnitude;

            float hunterDistSqr =
                (hunterObj.transform.position - transform.position).sqrMagnitude;

            target = playerDistSqr < hunterDistSqr
                ? playerObj.transform
                : hunterObj.transform;
        }
        else if (playerObj != null)
        {
            target = playerObj.transform;
        }
        else
        {
            target = hunterObj.transform;
        }
    }


    void SetClosestWaypointAsStart()
    {
        if (patrolCenterData == null ||
            patrolCenterData.waypoints == null ||
            patrolCenterData.waypoints.Length == 0)
            return;

        float closest = Mathf.Infinity;

        for (int i = 0; i < patrolCenterData.waypoints.Length; i++)
        {
            if (patrolCenterData.waypoints[i] == null) continue;

            float dist = Vector3.Distance(
                transform.position,
                patrolCenterData.waypoints[i].position
            );

            if (dist < closest)
            {
                closest = dist;
                currentWaypointIndex = i;
            }
        }

        patrolPoint = patrolCenterData.waypoints[currentWaypointIndex].position;
    }

    void RotateToMovement()
    {
        Vector3 dir = agent.velocity;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.01f)
            return;

        Quaternion rot = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rot,
            Time.deltaTime * 10f
        );
    }

    void UpdateState(float sqrDistance)
    {
        switch (currentState)
        {
            case EnemyState.Chase:

                if (sqrDistance <= attackRange * attackRange)
                {
                    currentState = EnemyState.Attack;
                }

                break;

            case EnemyState.Attack:

                if (sqrDistance > attackRange * attackRange)
                {
                    currentState = EnemyState.Chase;
                }

                break;
        }
    }

    public void SetChaseState()
    {
        currentState = EnemyState.Chase;
        waiting = false;

        if (agent != null)
        {
            agent.isStopped = false;
        }
    }

    private IEnumerator InitWhenReady()
    {
        // Chờ đến khi agent thực sự nằm trên NavMesh
        yield return new WaitUntil(() => agent.isOnNavMesh);

        agent.updateRotation = false;
        agent.speed = zombieData.Walkspeed;
        agent.acceleration = 15f;

        // FindClosestPatrolCenter();
        SetClosestWaypointAsStart();

        currentState = EnemyState.Chase;
        isInitialized = true;
        attackTimer = attackCooldown;
    }

    void Chase()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        PlayAudio(zombieData.ChaseSound);

        agent.isStopped = false;
        agent.speed = zombieData.Runspeed;

        agent.SetDestination(target.position);

        RotateToMovement();
    }

    void Attack()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        agent.isStopped = true;

        RotateToPlayer();

        if (attackTimer < attackCooldown)
            return;

        animator.SetTrigger("Attack");

        if (target != null && target.CompareTag("Player"))
        {
            if (bloodCoroutine != null)
                StopCoroutine(bloodCoroutine);

            bloodCoroutine = StartCoroutine(ActiveBloodScreen());
        }

        attackTimer = 0f;
    }

    void RotateToPlayer()
    {
        Vector3 dir =
            target.position - transform.position;

        dir.y = 0;

        // tránh vector zero
        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion rot =
            Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rot,
            Time.deltaTime * 10f
        );
    }

    void PlayAudio(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        if (audioSource.clip == clip && audioSource.isPlaying)
            return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    void InstantiatedObject()
    {
        if (InstantiatedObj != null) return;

        InstantiatedObj = Instantiate(BloodScreenObj);
    }

    void DeleteObject()
    {
        if (InstantiatedObj != null)
        {
            Destroy(InstantiatedObj);
            InstantiatedObj = null;
        }
    }

    private IEnumerator ActiveBloodScreen()
    {
        InstantiatedObject();
        yield return new WaitForSeconds(attackCooldown);
        DeleteObject();
    }

    void OnDrawGizmosSelected()
    {
        // Detection Range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        // Attack Range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );

        // Lose Range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(
            transform.position,
            loseRange
        );

    }
}
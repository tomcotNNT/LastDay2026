using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MutantBossAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    private MutantHealth mutantHealth;
    public MutantData mutantData;

    [Header("Audio")]
    public AudioSource audioSource;
    public float walkPitchMin = 0.8f;
    public float walkPitchMax = 1.5f;
    public float footstepIntervalWalk = 0.7f;
    public float footstepIntervalRun = 0.35f;
    private float footstepTimer;
    private AudioClip footstepClip;

    [Header("Jump")]
    public float jumpDistance = 5f; // khoảng cách nhảy phía trước

    [Header("Time")]
    public float jumpAttackTime = 1.5f;
    public float roarTime = 2.5f;
    public float poisonRange = 10f;
    public float meleeRange = 2f;
    public float meleeTime = 2f;
    public float poisonTime = 3f;
    public float skillCooldown = 3f;
    public float postJumpDelay = 1.5f; // thời gian chờ sau khi đáp
    public float landingShakeRange = 5f; // range để kích hoạt camera shake
    public float landingShakeDuration = 0.5f; // thời gian rung
    public float landingShakeMagnitude = 1f; // cường độ rung

    private Transform player;
    private bool isAttacking;
    private bool canUseSkill = true;
    private bool isRoaring = false;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        mutantHealth = GetComponent<MutantHealth>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;

        agent.speed = mutantData != null ? mutantData.walkSpeed : 2f;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        if (mutantData != null && mutantData.WalkingSound != null)
        {
            footstepClip = mutantData.WalkingSound;
        }
        footstepTimer = 0f;

        // đảm bảo Camera.main có component CameraShake để có thể rung
       Camera camMain = GameObject
            .FindGameObjectWithTag("MainCamera")
            ?.GetComponent<Camera>();


        if (camMain == null)
        {
            Debug.LogWarning("Main Camera not found. Please tag your camera as 'MainCamera'.");
        }
        else
        {
            if (camMain.GetComponent<CameraShake>() == null)
            {
                camMain.gameObject.AddComponent<CameraShake>();
                Debug.Log("MutantAI: CameraShake component added to Main Camera.");
            }
        }

        StartCoroutine(SpawnRoar());
    }

    void Update()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
                player = playerObj.transform;

            return;
        }

        if (isAttacking) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (mutantData != null && mutantHealth != null)
        {
            bool isLowHealth = mutantHealth.currentHealth <= mutantData.maxHealth * 0.5f;
            agent.speed = isLowHealth ? mutantData.runSpeed : mutantData.walkSpeed;

            if (isLowHealth)
            {
                agent.speed = mutantData.runSpeed;
                animator.SetFloat("Speed", 1f); // Run
            }
            else
            {
                agent.speed = mutantData.walkSpeed;
                animator.SetFloat("Speed", 0.5f); // Walk
            }
        }

        agent.isStopped = false;

        bool isMoving = agent.velocity.magnitude > 0.1f && !isRoaring && !isAttacking;
        if (audioSource != null && footstepClip != null)
        {
            if (isMoving)
            {
                float speedRatio = mutantData != null && mutantData.walkSpeed > 0f
                    ? agent.speed / mutantData.walkSpeed
                    : 1f;
                float pitch = Mathf.Clamp(speedRatio, walkPitchMin, walkPitchMax);
                audioSource.pitch = pitch;

                float interval = Mathf.Lerp(footstepIntervalWalk, footstepIntervalRun, (speedRatio - 1f) / (mutantData != null && mutantData.runSpeed > mutantData.walkSpeed ? mutantData.runSpeed - mutantData.walkSpeed : 1f));
                interval = Mathf.Clamp(interval, footstepIntervalRun, footstepIntervalWalk);

                footstepTimer -= Time.deltaTime;
                if (footstepTimer <= 0f)
                {
                    audioSource.PlayOneShot(footstepClip);
                    footstepTimer = interval;
                }
            }
            else
            {
                footstepTimer = 0f;
            }
        }

        // Chỉ set destination nếu không đang roar
        if (!isRoaring)
            agent.SetDestination(player.position);

        if (distance <= meleeRange)
        {
            StartCoroutine(MeleeAttack());
        }
        else if (distance <= poisonRange && canUseSkill)
        {
            int rand = Random.Range(0, 3);

            if (rand == 0)
                StartCoroutine(PoisonAttack());
            else if (rand == 1)
                StartCoroutine(JumpAttack());
            else
                StartCoroutine(NoAttack());
        }
    }

    IEnumerator SpawnRoar()
    {
        isAttacking = true;
        isRoaring = true;
        agent.isStopped = true;

        animator.SetTrigger("Roar");
        animator.SetFloat("Speed", 0f);
        if (mutantData != null && mutantData.RoarSound != null)
            audioSource.PlayOneShot(mutantData.RoarSound);

        yield return new WaitForSeconds(roarTime);

        agent.isStopped = false;
        isAttacking = false;
        isRoaring = false;
    }

    IEnumerator MeleeAttack()
    {
        isAttacking = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        animator.SetFloat("Speed", 0f);

        animator.SetTrigger("AttackMelee");
        if (mutantData != null && mutantData.MeleeAttackSound != null)
            audioSource.PlayOneShot(mutantData.MeleeAttackSound);

        yield return new WaitForSeconds(meleeTime);

        agent.isStopped = false;
        isAttacking = false;
    }

    IEnumerator PoisonAttack()
    {
        isAttacking = true;
        canUseSkill = false;
        agent.isStopped = true;

        animator.SetTrigger("AttackPoison");
        animator.SetFloat("Speed", 0f);

        if(mutantData != null && mutantData.PoisonAttackSound != null)
            audioSource.PlayOneShot(mutantData.PoisonAttackSound);
      
        yield return new WaitForSeconds(poisonTime);

        agent.isStopped = false;
        isAttacking = false;

        yield return new WaitForSeconds(skillCooldown);

        canUseSkill = true;
    }

    IEnumerator JumpAttack()
    {
        isAttacking = true;
        canUseSkill = false;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updatePosition = false;

        animator.applyRootMotion = true;
        animator.SetFloat("Speed", 0f);
        animator.SetTrigger("AttackJump");

        yield return new WaitForSeconds(jumpAttackTime);

        agent.Warp(transform.position);

        if (mutantData != null && mutantData.JumpAttackSound != null)
            audioSource.PlayOneShot(mutantData.JumpAttackSound);

        agent.updatePosition = true;
        animator.applyRootMotion = false;

        // nếu player trong range thì kích hoạt camera shake ngay khi đáp
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            bool inRange = dist <= landingShakeRange;
            Debug.Log($"MutantAI: landing distance to player = {dist:F2}, inRange={inRange}");

            if (inRange)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    CameraShake cs = cam.GetComponent<CameraShake>();
                    if (cs != null)
                    {
                        Debug.Log($"MutantAI: invoking CameraShake.Shake(duration={landingShakeDuration}, magnitude={landingShakeMagnitude})");
                        cs.Shake(landingShakeDuration, landingShakeMagnitude);
                    }
                    else
                    {
                        Debug.Log("MutantAI: CameraShake component not found, using SendMessage fallback");
                        cam.gameObject.SendMessage("Shake", landingShakeDuration, SendMessageOptions.DontRequireReceiver);
                    }
                }
                else
                {
                    Debug.LogWarning("MutantAI: Camera.main is null when trying to shake camera.");
                }

                Health playerHealth = player.GetComponentInChildren<Health>();
                if (playerHealth != null && mutantData != null)
                {
                    playerHealth.TakeDamage(mutantData.jumpAttackDamage);
                    Debug.Log($"MutantAI: landing hit player for {mutantData.jumpAttackDamage} damage");
                }
                else if (playerHealth == null)
                {
                    Debug.LogWarning("MutantAI: Player Health component not found on landing.");
                }
            }
        }

        // chờ một khoảng sau khi đáp trước khi quay về chase
        yield return new WaitForSeconds(postJumpDelay);

        agent.isStopped = false;
        agent.SetDestination(player.position);

        isAttacking = false;

        yield return new WaitForSeconds(skillCooldown);
        canUseSkill = true;
    }


    IEnumerator NoAttack()
    {
        canUseSkill = false;
        yield return new WaitForSeconds(skillCooldown);
        canUseSkill = true;
    }

    void OnAnimatorMove()
    {
        if (isAttacking && animator.applyRootMotion)
        {
            transform.position = animator.rootPosition;
            transform.rotation = animator.rootRotation;

            if (agent.enabled)
                agent.nextPosition = transform.position;
        }
    }

    //gizmos
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, poisonRange);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, landingShakeRange);
    }
}
using UnityEngine;
using UnityEngine.AI;

public class NPCHealth : MonoBehaviour
{
    [Header("Health")]
    public CharData charData;
    public float currentHealth;
    private bool isDead;
    public bool IsDead => isDead;

    [Header("References")]
    public Animator animator;
    public NavMeshAgent agent;
    public NPCAI npcAI;

    [Header("Animation")]
    // public string hitTrigger = "Hit";
    public string dieTrigger = "Die";

    [Header("Knockback")]
    public bool useKnockback = true;
    public float pushCooldown = 1f;
    public float pushForce = 8f;
    private float lastPushTime;

    public GameObject Hunter;

    void Start()
    {
        currentHealth = charData.maxHealth;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (npcAI == null)
            npcAI = GetComponent<NPCAI>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead)
            return;

        // Zombie đánh thường
        if (other.CompareTag("EnemyHand"))
        {
            EnemyAI enemy = other.GetComponentInParent<EnemyAI>();

            if (enemy != null && enemy.zombieData != null)
            {
                TakeDamage(enemy.zombieData.damage);
                ApplyKnockback(other.transform);
                Debug.Log("NPC hit by enemy hand");
            }
        }

        // Mutant đánh tay
        if (other.CompareTag("LHandMutant"))
        {
            MutantBossAI mutant = other.GetComponentInParent<MutantBossAI>();

            if (mutant != null && mutant.mutantData != null)
            {
                TakeDamage(mutant.mutantData.damage);
                ApplyKnockback(other.transform);
                Debug.Log("NPC hit by mutant melee");
            }
        }

        // Đạn độc bay
        if (other.CompareTag("Poison"))
        {
            Poison poison = other.GetComponent<Poison>();

            if (poison != null && poison.poisonData != null)
            {
                TakeDamage(poison.poisonData.damage);
                Debug.Log("NPC hit by poison");
            }
        }

        // Vùng độc dưới đất
        if (other.CompareTag("GroundPoison"))
        {
            GroundPoison poison = other.GetComponent<GroundPoison>();

            if (poison != null && poison.poisonData != null)
            {
                TakeDamage(poison.poisonData.damage - 5f);
                Debug.Log("NPC hit by ground poison");
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        Debug.Log(gameObject.name + " NPC took damage: " + damage + 
                  " | HP: " + currentHealth);

        // if (animator != null)
        //     animator.SetTrigger(hitTrigger);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void ApplyKnockback(Transform attacker)
    {
        if (!useKnockback)
            return;

        if (Time.time < lastPushTime + pushCooldown)
            return;

        if (agent == null || !agent.enabled)
            return;

        Vector3 dir = transform.position - attacker.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
            dir = -transform.forward;

        Vector3 targetPos = transform.position + dir.normalized * pushForce;

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, pushForce, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            lastPushTime = Time.time;
        }
    }

    void Die()
    {
        isDead = true;

        Debug.Log(gameObject.name + " NPC died");

        if (npcAI != null)
            npcAI.enabled = false;

        if (agent != null && agent.enabled)
        {
            agent.ResetPath();
            agent.enabled = false;
        }

        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        if (animator != null)
            animator.SetTrigger(dieTrigger);

        //xóa tag
        Hunter.tag = "Untagged";    

        Destroy(Hunter, 4f);
    }
}
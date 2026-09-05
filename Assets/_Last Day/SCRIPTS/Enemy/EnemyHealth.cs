using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    public ZombieData zombieData;
    public float currentHealth;
    bool isDead = false;
    private Animator anim;
    public GameObject icon;
    private bool isReward = false;
    public GameObject[] item;

    private EnemyAI enemyAI;
    private NavMeshAgent agent;
    private Rigidbody rb;
    private Collider col;

    //Sound
    public AudioSource audioSource;

    public Transform aimPoint;
    public Transform Rewardpoint;


    void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        anim = GetComponent<Animator>();


        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (zombieData != null)
        {
            currentHealth = zombieData.maxHealth;
        }
        else
        {
            Debug.LogError("ZombieData not assigned on " + gameObject.name);
            currentHealth = 100f; // default value
        }

        //random rewward
        int rand = Random.Range(0, 2);
        if(rand == 1)
        {
            isReward = true;
            
        }
        else
        {
            isReward = false;
        }
    }

    public void TakeDamage(float damage, bool isHeadshot)
    {
        if(isDead)
            return;

            
        currentHealth -= damage;

        Debug.Log(gameObject.name + 
            " took " + damage + 
            " damage. Remaining health: " + currentHealth);

        if (currentHealth <= 0)
        {   
            isDead = true;
            Die(isHeadshot);
        }
    }

    void Die(bool isHeadshot)
    {
        Debug.Log("Zombie Die: " + Time.time);

        if (enemyAI != null)
            enemyAI.enabled = false;

        if (agent != null)
            agent.enabled = false;

        if (rb != null)
            rb.isKinematic = true;

        if (col != null)
            col.enabled = false;

        if (isHeadshot)
        {
            if (anim != null)
            {
                anim.SetTrigger("HeadShot");
            }

            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(zombieData.DieSound);
            }

            Debug.Log("Headshot!");
        }
        else
        {
            if (anim != null)
            {
                anim.SetTrigger("Die");
            }

            if (audioSource != null)
            {   
                audioSource.Stop();
                audioSource.PlayOneShot(zombieData.DieSound);
            }

            Debug.Log(gameObject.name + " has died.");
        }

        // Nếu có item thưởng, instantiate nó tại vị trí của zombie
        if (isReward && item.Length > 0)
        {
            int randItem = Random.Range(0, item.Length);
            Instantiate(item[randItem], Rewardpoint.position, Quaternion.identity);
        }

        //tắt icon minimap
        if(icon != null)
        {
            icon.SetActive(false);
        }

        // Không còn được xem là enemy
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            t.tag = "Untagged";
        }

        // Đổi layer sang Dead
        gameObject.layer = LayerMask.NameToLayer("Dead");

        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.layer = LayerMask.NameToLayer("Dead");
        }

        Destroy(gameObject, 5f);
    }
}
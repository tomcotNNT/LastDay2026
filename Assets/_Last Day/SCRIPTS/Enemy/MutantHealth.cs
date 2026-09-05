using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MutantHealth : MonoBehaviour
{
    public MutantData mutantData;
    public float currentHealth;
    bool isDead = false;
    private Animator anim;
    public GameObject icon;
    //Sound
    public AudioSource audioSource;
    public Transform aimPoint;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (mutantData != null)
        {
            currentHealth = mutantData.maxHealth;
        }
        else
        {
            Debug.LogError("MutantData not assigned on " + gameObject.name);
            currentHealth = 100f; // default value
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
        // tắt AI
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.enabled = false;
        }

        MutantBossAI bossAI = GetComponent<MutantBossAI>();
        if (bossAI != null)
        {
            bossAI.enabled = false;
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.isKinematic = true;
        }

        // tắt collider nếu muốn
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }


        if (isHeadshot)
        {
            if (anim != null)
            {
                anim.SetTrigger("Die");
            }

            if (audioSource != null)
            {
                audioSource.Stop();
                audioSource.PlayOneShot(mutantData.DieSound);
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
                audioSource.PlayOneShot(mutantData.DieSound);
            }

            Debug.Log(gameObject.name + " has died.");
        }

        //tắt icon minimap
        if(icon != null)
        {
            icon.SetActive(false);
        }

        StartCoroutine(WinAfterBossDead());
        Destroy(gameObject, 8f);
    }

    IEnumerator WinAfterBossDead()
    {   
        Debug.Log("Mutant Boss defeated. Preparing win sequence...");
        // Ngưng spawn zombie
        AmbientZombieSpawner spawner = FindObjectOfType<AmbientZombieSpawner>();
        if (spawner != null)
            spawner.StopSpawn();

        // Cho toàn bộ zombie chết
        EnemyHealth[] zombies = FindObjectsOfType<EnemyHealth>();

        foreach (EnemyHealth zombie in zombies)
        {
            if (zombie != null)
                zombie.TakeDamage(99999f, false);
        }

        // Đợi 8 giây rồi hiện Win UI
        yield return new WaitForSeconds(7f);

        Health playerHealth = FindObjectOfType<Health>();

        if (RewardManager.Instance != null && playerHealth != null)
        {   

            Debug.Log("Giving win reward to player. Total damage taken: " + playerHealth.totalDamageTaken);
            RewardManager.Instance.GiveWinReward(playerHealth.totalDamageTaken);
        }
    }
}
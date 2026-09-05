using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;

public class Health : MonoBehaviour
{
    public CharData charData;
    [Header("Health")]
    public float currentHealth;
    private bool isDead = false;
    public bool IsDead => isDead;
    public float totalDamageTaken;

    [Header("Damage Cooldown")]
    public float damageCooldown = 1f; // thời gian miễn sát thương
    private float lastDamageTime;

    [Header("UI")]
    public Image healthBarFill; 
    public GameObject GameOverPanel;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("GunSystem")]
    public GunSystem[] gunSystem;
    public AimSystem[] aimSystem;
    public MacheteSystem[] macheteSystem;

    public GameObject BloodScreenObj;
    private GameObject InstantiatedObj;
    public float attackCooldown = 2f;

    private float pushCooldown = 0.5f;
    private float lastPushTime = -999f;
    public ParticleSystem HealEffect;

    void Awake()
    {
        if (charData != null)
            currentHealth = charData.maxHealth;
            
        lastDamageTime = -999f;
        UpdateHealthBar();
    }

    // NHẬN DAMAGE (có cooldown)
    public void TakeDamage(float damage)
    {   
        if (isDead) return;
        if (Time.time < lastDamageTime + damageCooldown)
            return; // đang trong thời gian bất tử

        lastDamageTime = Time.time;

        if (charData != null)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, charData.maxHealth);
        }

        totalDamageTaken += damage;

        // play hit sound
        if (charData != null && charData.hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(charData.hitSound);
        }

        // bật bloodscreen
        StartCoroutine(ActiveBloodScreen());

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // HỒI MÁU
    public void Heal(float amount)
    {
        currentHealth += amount;
        if (charData != null)
            currentHealth = Mathf.Clamp(currentHealth, 0, charData.maxHealth);

        // effect
        if (HealEffect != null)
            HealEffect.Play();

        UpdateHealthBar();
    }

    void Die()
    {   
        if (isDead) return;
        isDead = true;

        Debug.Log("Dead");
        // play death sound
        if (charData != null && charData.deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(charData.deathSound);
        }

        FPSController fps = GetComponentInParent<FPSController>();
        if (fps != null)
            fps.Die();

        StartCoroutine(FreezeAfterDelay(2f));
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyHand"))
        {
            EnemyAI enemyHand = other.GetComponentInParent<EnemyAI>();
            if (enemyHand != null && enemyHand.zombieData != null)
                TakeDamage(enemyHand.zombieData.damage);
        }

        if (other.CompareTag("LHandMutant"))
        {
            MutantBossAI enemyMelee = other.GetComponentInParent<MutantBossAI>();
            if (enemyMelee != null && enemyMelee.mutantData != null)
            {
                TakeDamage(enemyMelee.mutantData.damage);

                if (Time.time >= lastPushTime + pushCooldown)
                {
                    FPSController fps = GetComponentInParent<FPSController>();
                    Vector3 pushDirection = other.transform.forward;
                    pushDirection.y = 0;
                    if (pushDirection.sqrMagnitude < 0.001f)
                        pushDirection = transform.position - other.transform.position;

                    if (fps != null)
                    {
                        float pushForce = 20f;
                        fps.ApplyKnockback(pushDirection, pushForce);
                        lastPushTime = Time.time;
                    }
                }
            }
        }

        if (other.CompareTag("Poison"))
        {
            Poison poison = other.GetComponent<Poison>();
            if (poison != null && poison.poisonData != null)
                TakeDamage(poison.poisonData.damage);
        }

        if (other.CompareTag("GroundPoison"))
        {
            GroundPoison poison = other.GetComponent<GroundPoison>();
            if (poison != null && poison.poisonData != null)
                TakeDamage(poison.poisonData.damage - 5f);
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarFill != null && charData != null && charData.maxHealth > 0)
            healthBarFill.fillAmount = currentHealth / charData.maxHealth;
    }

    IEnumerator FreezeAfterDelay(float delay)
    {
        gunSystem = GetComponentsInChildren<GunSystem>(true);
        aimSystem = GetComponentsInChildren<AimSystem>(true);
        macheteSystem = GetComponentsInChildren<MacheteSystem>(true);

        foreach (var gun in gunSystem)
        {
            if (gun != null) gun.enabled = false;
        }

        foreach (var aim in aimSystem)
        {
            if (aim != null) aim.enabled = false;
        }

        foreach (var machete in macheteSystem)
        {
            if (machete != null) machete.enabled = false;
        }

        yield return new WaitForSecondsRealtime(delay);

        if (GameOverPanel != null)
            GameOverPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    void InstantiatedObject()
    {
        if (InstantiatedObj != null || BloodScreenObj == null) return;
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
}
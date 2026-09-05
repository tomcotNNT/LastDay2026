using UnityEngine;

public class ProjectileHandler : MonoBehaviour
{
    [HideInInspector] public float damage = 10f;
    [HideInInspector] public bool hasHit = false;
    
    private Rigidbody rb;
    private Collider col;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.collider);
    }

    void HandleHit(Collider hitCollider)
    {
        if (hasHit) return;

        // check if hit enemy tags
        if (hitCollider.CompareTag("EnemyHead") || hitCollider.CompareTag("MutantHead"))
        {
            hasHit = true;
            ApplyHeadshotDamage(hitCollider);

            Debug.Log("[Projectile] Hit Head");
            Destroy(gameObject);
        }
        else if (hitCollider.CompareTag("EnemyBody") || hitCollider.CompareTag("MutantBody") || hitCollider.CompareTag("Enemy"))
        {
            hasHit = true;
            ApplyBodyDamage(hitCollider);
            Debug.Log("[Projectile] Hit Body");
            Destroy(gameObject);
        }
    }

    void ApplyHeadshotDamage(Collider hitCollider)
    {
        float headDamage = damage;

        if (hitCollider.CompareTag("MutantHead"))
        {
            var mutant = hitCollider.GetComponentInParent<MutantHealth>();
            if (mutant != null)
            {
                mutant.TakeDamage(headDamage, true);
                Debug.Log($"[Projectile] Hit Mutant Head for {headDamage} damage");
            }
        }
        else
        {
            var enemy = hitCollider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(headDamage, true);
                Debug.Log($"[Projectile] Hit Enemy Head for {headDamage} damage");
            }
        }
    }

    void ApplyBodyDamage(Collider hitCollider)
    {
        if (hitCollider.CompareTag("MutantBody"))
        {
            var mutant = hitCollider.GetComponentInParent<MutantHealth>();
            if (mutant != null)
            {
                mutant.TakeDamage(damage, false);
                Debug.Log($"[Projectile] Hit Mutant Body for {damage} damage");
            }
        }
        else
        {
            var enemy = hitCollider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, false);
                Debug.Log($"[Projectile] Hit Enemy for {damage} damage");
            }
        }
    }
}

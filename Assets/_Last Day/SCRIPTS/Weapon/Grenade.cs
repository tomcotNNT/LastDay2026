using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Header("Explosion")]
    public float delay = 2f;
    public float radius = 5f;
    public float damage = 200f;
    public float explosionForce = 700f;

    [Header("Effect")]
    public GameObject explosionEffect;

    [Header("Screen Shake")]
    public float shakeRadius = 10f;
    public float maxShakeDuration = 0.4f;
    public float maxShakeMagnitude = 0.3f;

    [Header("Audio")]
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip explosionSound;



    private CameraShake cameraShake;
    private Transform player;
    private bool exploded;


    void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Start()
    {
        player =
            GameObject.FindGameObjectWithTag("Player")
            ?.transform;
        if(player == null)
        {
            Debug.Log("Ko có player");

        }

        cameraShake =
        Camera.main.GetComponent<CameraShake>();

        StartCoroutine(ExplodeAfterDelay());
    }

    IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        Explode();
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        if (explosionEffect != null)
        {
            Instantiate(
                explosionEffect,
                transform.position,
                Quaternion.identity
            );
        }


        if (explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);

        }
        
        if (player != null && cameraShake != null)
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    player.position
                );

            if (distance <= shakeRadius)
            {
                float percent =
                    1f - (distance / shakeRadius);

                cameraShake.Shake(
                    maxShakeDuration * percent,
                    maxShakeMagnitude * percent
                );
            }
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hits)
        {
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();

            Health player = hit.GetComponentInParent<Health>();

            MutantHealth mutant = hit.GetComponentInParent<MutantHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage, false);
            }

            if (player != null)
            {
                player.TakeDamage(damage);
            }

            if (mutant != null)
            {
                mutant.TakeDamage(damage, false);

                Debug.Log("mutant hit boom");
            }

            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddExplosionForce(
                    explosionForce,
                    transform.position,
                    radius
                );
            }
        }

       
        Destroy(gameObject,1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            radius
        );

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            transform.position,
            shakeRadius
        );
    }

}
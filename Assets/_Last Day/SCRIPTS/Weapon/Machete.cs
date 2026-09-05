using UnityEngine;

public class Machete : MonoBehaviour
{
    [SerializeField]
    private Collider hitCollider;
    public MacheteData macheteData;
    public ParticleSystem bloodPrefab;
    public GameObject bulletUI;
    public GameObject crosshairUI;
    private AudioSource audioSource;

    public AudioClip SwitchClip;

    void Awake()
    {
        if (hitCollider == null)
            hitCollider = GetComponent<Collider>() ?? GetComponentInChildren<Collider>();

        if (hitCollider != null)
            hitCollider.enabled = false;

        audioSource = GetComponent<AudioSource>();
        
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.clip = SwitchClip;
        }
    }

    public void EnableCollider()
    {
        if (hitCollider != null)
            hitCollider.enabled = true;
    }

    public void DisableCollider()
    {
        if (hitCollider != null)
            hitCollider.enabled = false;
    }

    void Start()
    {
        EnsureUIReferences();
    }

    void OnEnable()
    {
        EnsureUIReferences();
        SetUIActive(false);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>() ?? GetComponentInParent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            if (SwitchClip != null)
                audioSource.PlayOneShot(SwitchClip);
        }
        else
        {
            Debug.LogWarning("Machete: AudioSource not found for SwitchClip playback.");
        }
    }

    void EnsureUIReferences()
    {
        if (bulletUI == null)
        {
            GameObject obj = GameObject.Find("BulletUI");
            if (obj != null) bulletUI = obj;
        }

        if (crosshairUI == null)
        {
            GameObject obj = GameObject.Find("Crosshair");
            if (obj == null)
                obj = GameObject.Find("CrossHair");
            if (obj != null) crosshairUI = obj;
        }
    }

    void SetUIActive(bool active)
    {
        if (bulletUI != null)
            bulletUI.SetActive(active);
        if (crosshairUI != null)
            crosshairUI.SetActive(active);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBody"))
        {
            EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
            if (enemy != null && macheteData != null)
            {
                enemy.TakeDamage(macheteData.damage, false);
                Debug.Log($"Hit {other.name} for {macheteData.damage} damage");
                if (bloodPrefab != null)
                {
                    Vector3 spawnPos = other.ClosestPoint(transform.position);
                    Vector3 normal = (spawnPos - other.transform.position).normalized;
                    if (normal.sqrMagnitude < 0.0001f) normal = transform.forward;

                    ParticleSystem blood = Instantiate(
                        bloodPrefab,
                        spawnPos + normal * 0.01f,
                        Quaternion.LookRotation(normal)
                    );
                    blood.transform.SetParent(other.transform);
                    Destroy(blood.gameObject, 2f);
                }
            }
        }
    }
}

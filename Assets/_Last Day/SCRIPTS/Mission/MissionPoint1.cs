using UnityEngine;

public class MissionPoint : MonoBehaviour
{
    public Mission missionSystem;
    private bool activated;
    public AudioSource audioSource;
    public AudioClip Clip;

    void Start()
    {
        audioSource = GetComponentInParent<AudioSource>();
        audioSource.clip = Clip;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(activated) return;

        if(other.CompareTag("Player"))
        {
            activated = true;
            audioSource.PlayOneShot(audioSource.clip);
            Mission.Instance.StartExplore();

            Debug.Log("Explore Triggered");

            // tắt marker
            Mission.Instance.missionSystem.DisableMaker();
            Debug.Log("Marker Disabled");
        }
    }
}
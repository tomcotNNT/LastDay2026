using UnityEngine;

[CreateAssetMenu(fileName = "NewZombie", menuName = "Zombie/Zombie Data")]
public class ZombieData : ScriptableObject
{
    [Header("Info")]
    public string zombieName;

    [Header("Damage")]
    public float damage = 20f;

    [Header("Health")]
    public float maxHealth = 100f;
    [Header("Audio")]
    public AudioClip ChaseSound;
    public AudioClip PatrolSound;
    public AudioClip DieSound;
    [Header("Movement")]
    public float Walkspeed = 2f;
    public float Runspeed = 4f;
}
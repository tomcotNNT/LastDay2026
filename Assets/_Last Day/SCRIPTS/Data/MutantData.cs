using UnityEngine;

[CreateAssetMenu(fileName = "MutantData", menuName = "Game/Mutant Data")]
public class MutantData : ScriptableObject
{
    [Header("Info")]
    public string mutantName;

    [Header("Stats")]
    public float maxHealth = 1000f;
    public float damage = 20f;
    public float jumpAttackDamage = 15f;
    public float poisonAttackDamage = 10f;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;

    [Header("Audio")]
    public AudioClip DieSound;
    public AudioClip WalkingSound;
    public AudioClip RoarSound;
    public AudioClip JumpAttackSound;
    public AudioClip MeleeAttackSound;
    public AudioClip PoisonAttackSound;
}
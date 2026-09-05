using UnityEngine;

[CreateAssetMenu(fileName = "CharData", menuName = "Character Data")]
public class CharData : ScriptableObject
{
    [Header("Info")]
    public string characterName;

    [Header("Health")]
    public float maxHealth = 100f;
    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip deathSound;
}
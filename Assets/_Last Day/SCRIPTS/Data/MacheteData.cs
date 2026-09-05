using UnityEngine;

[CreateAssetMenu(fileName = "NewMachete", menuName = "Melee/Machete Data")]
public class MacheteData : ScriptableObject
{
    [Header("Info")]
    public string macheteName;

    [Header("Damage")]
    public float damage = 50f;
}

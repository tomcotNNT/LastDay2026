using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Info")]
    public string itemName;

    [Header("Health")]
    public float healthRestore = 50f;
}

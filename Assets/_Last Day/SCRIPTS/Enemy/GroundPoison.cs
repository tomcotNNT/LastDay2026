using UnityEngine;

public class GroundPoison : MonoBehaviour
{
    public float lifetime = 45f;
    public PoisonData poisonData;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

}
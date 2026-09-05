using UnityEngine;

public class Poison : MonoBehaviour
{
    public float lifetime = 5f;

    public GameObject groundPoisonPrefab;
    public LayerMask groundLayer;
    public PoisonData poisonData;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
{
    if (((1 << other.gameObject.layer) & groundLayer) == 0)
        return;

    RaycastHit hit;
    if (Physics.Raycast(transform.position + Vector3.up,
                        Vector3.down,
                        out hit,
                        5f,
                        groundLayer))
    {
        Instantiate(
            groundPoisonPrefab,
            hit.point + Vector3.up * 0.02f,
            groundPoisonPrefab.transform.rotation
        );
    }

    Destroy(gameObject);
}
}
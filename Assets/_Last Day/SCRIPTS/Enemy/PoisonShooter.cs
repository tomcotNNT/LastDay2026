using UnityEngine;

public class PoisonShooter : MonoBehaviour
{
    public Transform FirePoint;
    public GameObject PoisonPrefab;
    public float projectileSpeed = 15f;

    

    // Can be called from code as well
    public void ShootAt()
    {
        if (FirePoint == null || PoisonPrefab == null) return;

        Vector3 spawnPos = FirePoint.position;
        Vector3 dir = FirePoint.forward;
        Quaternion rot = Quaternion.LookRotation(dir);

        GameObject proj = Instantiate(PoisonPrefab, spawnPos, rot);

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = dir * projectileSpeed;
        }
        else
        {
            proj.transform.forward = dir;
        }
    }
}

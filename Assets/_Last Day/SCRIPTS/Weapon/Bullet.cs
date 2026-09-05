using UnityEngine;

public class Bullet : MonoBehaviour
{
    // thời gian tự hủy
    public float lifeTime = 3f;
    // sát thương của viên đạn
    public float damage ;
    

    void Start()
    {
        // tự hủy
        Destroy(gameObject, lifeTime);
    }


}
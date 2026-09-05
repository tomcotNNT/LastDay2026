// EnemyDamage.cs - gắn vào Enemy
using UnityEngine;

public class EnemyHand : MonoBehaviour
{

    public GameObject Lhand;
    public GameObject Rhand;
    void EnableLHand()
    {
        Lhand.SetActive(true);
       
    }

    void EnableRHand()
    {
        Rhand.SetActive(true);
    }


    void DisableLHand()
    {
        Lhand.SetActive(false);
    }
    void DisableRHand()
    {
        Rhand.SetActive(false);
    }
}
    

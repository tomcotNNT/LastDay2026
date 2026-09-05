using UnityEngine;

public class MarkerSystem : MonoBehaviour
{
    public static MarkerSystem Instance;

    public Transform[] targets;
    public GameObject markerPrefab;

    private GameObject currentMarker;
    private int currentTargetIndex;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Tắt hết point lúc đầu
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
                targets[i].gameObject.SetActive(false);
        }

        SpawnMarker();
    }

    public void SpawnMarker()
    {
        if (currentMarker != null)
            Destroy(currentMarker);

        if (currentTargetIndex < 0 || currentTargetIndex >= targets.Length)
            return;

        Transform target = targets[currentTargetIndex];

        if (target == null)
            return;

        // Bật point hiện tại
        target.gameObject.SetActive(true);

        Collider col = target.GetComponent<Collider>();

        Vector3 spawnPos = col != null
            ? col.bounds.center
            : target.position;

        currentMarker = Instantiate(
            markerPrefab,
            spawnPos,
            Quaternion.identity
        );

        Debug.Log("Spawn marker cho: " + target.name);
    }

    public void CompleteCurrentMission()
    {
        // Tắt point hiện tại
        if (currentTargetIndex >= 0 && currentTargetIndex < targets.Length)
        {
            if (targets[currentTargetIndex] != null)
                targets[currentTargetIndex].gameObject.SetActive(false);
        }

        currentTargetIndex++;

        if (currentTargetIndex >= targets.Length)
        {
            Debug.Log("All missions complete");

            if (currentMarker != null)
            {
                Destroy(currentMarker);
                currentMarker = null;
            }

            return;
        }

        SpawnMarker();
    }

    public void DisableMaker()
    {
        if (currentMarker != null)
        {
            Debug.Log("Xóa Marker: " + currentMarker.name);
            Destroy(currentMarker);
            currentMarker = null;
        }
        else
        {
            Debug.LogWarning("Không có currentMarker để xóa");
        }
    }
}
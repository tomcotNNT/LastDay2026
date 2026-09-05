using UnityEngine;

public class WeaponPreviewUI : MonoBehaviour
{
    [Header("Root sẽ xoay")]
    public Transform previewRoot;

    [Header("Danh sách súng")]
    public GameObject[] weaponObjects;

    private int currentIndex;

    void Start()
    {
        ShowWeapon(0);
    }

    public void ShowWeapon(int index)
    {
        if (weaponObjects == null || weaponObjects.Length == 0)
            return;

        currentIndex = Mathf.Clamp(index, 0, weaponObjects.Length - 1);

        for (int i = 0; i < weaponObjects.Length; i++)
        {
            if (weaponObjects[i] != null)
                weaponObjects[i].SetActive(i == currentIndex);
        }
    }
}
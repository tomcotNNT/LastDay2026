using UnityEngine;
using UnityEngine.UI;

public class GunButton : MonoBehaviour
{
    public WeaponData weaponData;
    public GunShopUI shopUI;

    private Button button;

    void Awake()
    {
        // Tự tìm Button trên chính nó hoặc trong các object con
        button = GetComponent<Button>();
        if (button == null)
            button = GetComponentInChildren<Button>();

        if (button != null)
        {
            button.onClick.AddListener(OnClickGun);
        }
        else
        {
            Debug.LogWarning($"[GunButton] Không tìm thấy component Button nào trên {gameObject.name} hoặc các con của nó!");
        }
    }

    void Start()
    {
        if (shopUI == null)
            shopUI = Object.FindFirstObjectByType<GunShopUI>();
    }

    public void OnClickGun()
    {
        Debug.Log($"<color=yellow>[Shop Click]</color> Đã bấm nút súng: {(weaponData != null ? weaponData.weaponName : "CHƯA GẮN WEAPON DATA")}");

        if (shopUI != null && weaponData != null)
        {
            shopUI.SelectWeapon(weaponData);
        }
    }
}
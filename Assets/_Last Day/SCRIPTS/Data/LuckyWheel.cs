using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LuckyWheel : MonoBehaviour
{
    public enum RewardType
    {
        None,           // Ô trượt (Good luck again)
        Money,          // Tiền tệ
        Ticket,         // Vé quay
        UpgradeShard,   // Mảnh nâng cấp
        Grenade,        // Lựu đạn
        ProtectCard,    // Thẻ bảo vệ
        MutantCrystal,  // Tinh thể
        Equipment       // Súng, Vũ khí, Skin (Dạng vật phẩm sở hữu)
    }

    [System.Serializable]
    public class RewardData
    {
        public string rewardName;
        public RewardType rewardType;

        [Tooltip("Mã định danh vật phẩm để đồng bộ qua các Scene khác (Ví dụ: gun_rifle, gun_ak47)")]
        public string itemId = ""; // Tái sử dụng cho hệ thống Kho đồ sau này

        public Sprite icon;
        public int amount = 1;
        public int weight;
        public string popupMessage;

        public Vector2 iconSize = new Vector2(70, 70);
        public Vector2 iconOffset;
        public GameObject rewardPopup;
    }

    // Sự kiện C# bắn ra toàn game khi người chơi trúng trang bị/vật phẩm sở hữu
    public static event Action<string, int> OnEquipmentUnlocked;

    [Header("Wheel UI")]
    public RectTransform wheel;
    public Button spinButton;
    public Button spin5Button;
    public Button spin10Button;
    public Button backButton;
    public TMP_Text rewardText;

    [Header("Reward")]
    public RewardData[] rewards;

    [Header("Popup")]
    public float popupAutoCloseTime = 1.5f;

    [Header("Spin Setting")]
    public float spinDuration = 4f;
    public float x5SpinDuration = 1.2f;
    public float x10SpinDuration = 0.7f;
    public float delayBetweenSpin = 0.1f;

    public int spinRound = 5;
    public float pointerAngle = 0f;
    public float offsetAngle = 0f;

    private bool isSpinning;

    [Header("Popup Close")]
    public GameObject currentPopup;

    [Header("Currency UI")]
    public TMP_Text moneyText;
    public TMP_Text ticketText;
    public TMP_Text shardText;
    public TMP_Text costText;
    public TMP_Text cost5Text;
    public TMP_Text cost10Text;

    [Header("Spin Cost")]
    public int ticketCost = 1;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip rewardSound;

    void Start()
    {
#if UNITY_EDITOR
        if (PlayerPrefs.GetInt("Ticket", 0) < 10)
        {
            PlayerPrefs.SetInt("Ticket", 99);
            PlayerPrefs.Save();
        }
#endif
        UpdateUI();
    }

    public void UpdateUI()
    {
        moneyText.text = PlayerPrefs.GetInt("Money", 0).ToString();
        ticketText.text = PlayerPrefs.GetInt("Ticket", 0).ToString();
        shardText.text = PlayerPrefs.GetInt("UpgradeShard", 0).ToString();

        costText.text = ticketCost.ToString();

        if (cost5Text != null)
            cost5Text.text = "X" + (ticketCost * 5).ToString();

        if (cost10Text != null)
            cost10Text.text = "X" + (ticketCost * 10).ToString();
    }

    public void Spin()
    {
        StartCoroutine(MultiSpin(1, spinDuration));
    }

    public void SpinX5()
    {
        StartCoroutine(MultiSpin(5, x5SpinDuration));
    }

    public void SpinX10()
    {
        StartCoroutine(MultiSpin(10, x10SpinDuration));
    }

    IEnumerator MultiSpin(int spinCount, float duration)
    {
        if (isSpinning) yield break;

        int totalCost = ticketCost * spinCount;
        int ticket = PlayerPrefs.GetInt("Ticket", 0);

        if (ticket < totalCost)
        {
            if (rewardText != null) rewardText.text = "Not enough tickets!";
            yield break;
        }

        ticket -= totalCost;
        PlayerPrefs.SetInt("Ticket", ticket);
        PlayerPrefs.Save();
        UpdateUI();

        isSpinning = true;
        SetButtons(false);

        for (int i = 0; i < spinCount; i++)
        {
            int rewardIndex = GetRandomRewardIndex();
            bool autoClose = spinCount > 1;

            yield return StartCoroutine(SpinToReward(rewardIndex, duration, autoClose));

            if (i < spinCount - 1)
            {
                yield return new WaitForSeconds(delayBetweenSpin);
            }
        }

        PlayerPrefs.Save();
        UpdateUI();

        SetButtons(true);
        isSpinning = false;
    }

    void SetButtons(bool value)
    {
        if (spinButton != null) spinButton.interactable = value;
        if (spin5Button != null) spin5Button.interactable = value;
        if (spin10Button != null) spin10Button.interactable = value;
        if (backButton != null) backButton.interactable = value;
    }

    int GetRandomRewardIndex()
    {
        int totalWeight = 0;
        for (int i = 0; i < rewards.Length; i++)
        {
            totalWeight += rewards[i].weight;
        }

        int random = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;

        for (int i = 0; i < rewards.Length; i++)
        {
            currentWeight += rewards[i].weight;
            if (random < currentWeight)
                return i;
        }

        return 0;
    }

    IEnumerator SpinToReward(int rewardIndex, float duration, bool autoClosePopup)
    {
        if (rewardText != null) rewardText.text = "";

        float anglePerSlot = 360f / rewards.Length;
        float slotCenterAngle = rewardIndex * anglePerSlot;
        float targetRotation = pointerAngle - slotCenterAngle + offsetAngle;

        float startZ = wheel.eulerAngles.z;
        float endZ = startZ + 360f * spinRound + Mathf.DeltaAngle(startZ, targetRotation);

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / duration);
            float z = Mathf.Lerp(startZ, endZ, t);

            wheel.eulerAngles = new Vector3(0, 0, z);
            yield return null;
        }

        wheel.eulerAngles = new Vector3(0, 0, endZ);
        GiveReward(rewardIndex, autoClosePopup);
    }

    void GiveReward(int index, bool autoClosePopup = false)
    {
        RewardData reward = rewards[index];

        if (rewardSound != null && audioSource != null)
            audioSource.PlayOneShot(rewardSound);

        ShowRewardPopup(reward, autoClosePopup);
        AddReward(reward);
    }

    void AddReward(RewardData reward)
    {
        if (reward.rewardType == RewardType.None) return;

        // XỬ LÝ TRANG BỊ / SÚNG (Tái sử dụng cho hệ thống Kho đồ tương lai)
        if (reward.rewardType == RewardType.Equipment)
        {
            string id = string.IsNullOrEmpty(reward.itemId) ? "gun_default" : reward.itemId;

            // Quy chuẩn lưu trữ: 1 = Đã sở hữu, cộng dồn số lượng nếu quay trúng nhiều lần
            PlayerPrefs.SetInt("EQUIP_UNLOCKED_" + id, 1);
            PlayerPrefs.SetInt("EQUIP_COUNT_" + id, PlayerPrefs.GetInt("EQUIP_COUNT_" + id, 0) + reward.amount);
            PlayerPrefs.SetString("LAST_ACQUIRED_EQUIP", id);

            // Kích hoạt sự kiện cho toàn bộ game
            OnEquipmentUnlocked?.Invoke(id, reward.amount);
            return;
        }

        // TÀI NGUYÊN THÔNG THƯỜNG
        string prefKey = reward.rewardType switch
        {
            RewardType.Money => "Money",
            RewardType.Ticket => "Ticket",
            RewardType.UpgradeShard => "UpgradeShard",
            RewardType.Grenade => "Grenade",
            RewardType.ProtectCard => "ProtectCard",
            RewardType.MutantCrystal => "MutantCrystal",
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(prefKey))
        {
            int currentVal = PlayerPrefs.GetInt(prefKey, 0);
            PlayerPrefs.SetInt(prefKey, currentVal + reward.amount);
        }
    }

    void ShowRewardPopup(RewardData reward, bool autoClose = false)
    {
        if (currentPopup != null)
            currentPopup.SetActive(false);

        if (reward.rewardPopup != null)
        {
            currentPopup = reward.rewardPopup;
            currentPopup.SetActive(true);

            TMP_Text text = currentPopup.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = reward.popupMessage;

            if (autoClose)
                StartCoroutine(AutoClosePopup());
        }
    }

    IEnumerator AutoClosePopup()
    {
        yield return new WaitForSeconds(popupAutoCloseTime);

        if (currentPopup != null)
        {
            currentPopup.SetActive(false);
            currentPopup = null;
        }
    }

    public void CloseRewardPopup()
    {
        if (currentPopup != null)
        {
            currentPopup.SetActive(false);
            currentPopup = null;
        }
    }

    public void BackToLobby()
    {
        if (isSpinning) return;
        SceneManager.LoadScene("Lobby");
    }
}
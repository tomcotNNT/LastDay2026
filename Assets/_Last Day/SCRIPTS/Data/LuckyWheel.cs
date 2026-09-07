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
        Equipment       // Súng, Trang bị sở hữu
    }

    [System.Serializable]
    public class RewardData
    {
        public string rewardName;
        public RewardType rewardType;

        [Tooltip("Mã định danh vật phẩm để đồng bộ qua các Scene khác (Ví dụ: gun_kar98)")]
        public string itemId = "";

        public Sprite icon;
        public int amount = 1;
        public int weight;
        public string popupMessage;

        public Vector2 iconSize = new Vector2(70, 70);
        public Vector2 iconOffset;
        public GameObject rewardPopup;
    }

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
    [Tooltip("Thời gian popup nhận thưởng tự động đóng")]
    public float popupAutoCloseTime = 3f;

    [Header("Spin Setting")]
    public float spinDuration = 4f;
    public float x5SpinDuration = 1.2f;
    public float x10SpinDuration = 0.7f;
    public float delayBetweenSpin = 0.1f;

    public int spinRound = 5;
    public float pointerAngle = 0f;
    public float offsetAngle = 0f;

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

    // Biến trạng thái nội bộ
    private bool isSpinning;
    private Coroutine noticeCoroutine; // Đã bổ sung biến này để hết lỗi CS0103
    private const string PENDING_REFUND_KEY = "Spin_Pending_Refund";

    void Start()
    {
        // Kiểm tra và hoàn trả vé nếu phiên chơi trước bị tắt ngang khi đang quay
        CheckAndRefundInterruptedSpin();

        UpdateUI();
    }

    void CheckAndRefundInterruptedSpin()
    {
        int pendingRefund = PlayerPrefs.GetInt(PENDING_REFUND_KEY, 0);
        if (pendingRefund > 0)
        {
            int currentTicket = PlayerPrefs.GetInt("Ticket", 0);
            PlayerPrefs.SetInt("Ticket", currentTicket + pendingRefund);
            PlayerPrefs.DeleteKey(PENDING_REFUND_KEY);
            PlayerPrefs.Save();

            ShowNotice($"Đã hoàn lại {pendingRefund} vé do vòng quay trước bị gián đoạn!", Color.yellow);
        }
    }

    public void UpdateUI()
    {
        if (moneyText != null) moneyText.text = PlayerPrefs.GetInt("Money", 0).ToString();
        if (ticketText != null) ticketText.text = PlayerPrefs.GetInt("Ticket", 0).ToString();
        if (shardText != null) shardText.text = PlayerPrefs.GetInt("UpgradeShard", 0).ToString();

        if (costText != null) costText.text = ticketCost.ToString();
        if (cost5Text != null) cost5Text.text = "X" + (ticketCost * 5).ToString();
        if (cost10Text != null) cost10Text.text = "X" + (ticketCost * 10).ToString();
    }

    public void Spin() => StartCoroutine(MultiSpin(1, spinDuration));
    public void SpinX5() => StartCoroutine(MultiSpin(5, x5SpinDuration));
    public void SpinX10() => StartCoroutine(MultiSpin(10, x10SpinDuration));

    IEnumerator MultiSpin(int spinCount, float duration)
    {
        if (isSpinning) yield break;

        int totalCost = ticketCost * spinCount;
        int currentTickets = PlayerPrefs.GetInt("Ticket", 0);

        // 1. Kiểm tra thiếu vé
        if (currentTickets < totalCost)
        {
            ShowNotice("Không đủ vé quay!", Color.red);
            yield break;
        }

        // 2. KHÓA GIAO DỊCH BAN ĐẦU: Giữ chỗ đúng tổng số vé quay
        PlayerPrefs.SetInt(PENDING_REFUND_KEY, totalCost);
        PlayerPrefs.SetInt("Ticket", currentTickets - totalCost);
        PlayerPrefs.Save();
        UpdateUI();

        isSpinning = true;
        SetButtons(false);

        for (int i = 0; i < spinCount; i++)
        {
            int rewardIndex = GetRandomRewardIndex();
            bool autoClose = true;

            // Chờ kim quay đến ô thưởng và phát quà
            yield return StartCoroutine(SpinToReward(rewardIndex, duration, autoClose));

            // =========================================================================
            // ĐIỂM CỐT LÕI (HƯỚNG 2): Đã nhận quà lượt này thành công 
            // -> Trừ bớt 1 lượt vé ra khỏi danh sách hoàn tiền đề phòng crash ở lượt sau
            // =========================================================================
            int remainingRefund = PlayerPrefs.GetInt(PENDING_REFUND_KEY, 0) - ticketCost;
            if (remainingRefund > 0)
            {
                PlayerPrefs.SetInt(PENDING_REFUND_KEY, remainingRefund);
            }
            else
            {
                PlayerPrefs.DeleteKey(PENDING_REFUND_KEY); // Đã quay hết các lượt an toàn
            }
            PlayerPrefs.Save(); // Lưu mốc an toàn sau từng lượt

            if (i < spinCount - 1)
            {
                yield return new WaitForSeconds(delayBetweenSpin);
            }
        }

        // Đảm bảo dọn dẹp sạch sẽ key hoàn vé khi kết thúc chuỗi
        PlayerPrefs.DeleteKey(PENDING_REFUND_KEY);
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
            wheel.eulerAngles = new Vector3(0, 0, Mathf.Lerp(startZ, endZ, t));
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

        if (reward.rewardType == RewardType.Equipment)
        {
            string id = string.IsNullOrEmpty(reward.itemId) ? "gun_default" : reward.itemId;
            PlayerPrefs.SetInt("EQUIP_UNLOCKED_" + id, 1);
            PlayerPrefs.SetInt("EQUIP_COUNT_" + id, PlayerPrefs.GetInt("EQUIP_COUNT_" + id, 0) + reward.amount);
            PlayerPrefs.SetString("LAST_ACQUIRED_EQUIP", id);
            OnEquipmentUnlocked?.Invoke(id, reward.amount);
            return;
        }

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

    void ShowNotice(string message, Color textColor)
    {
        if (rewardText != null)
        {
            rewardText.gameObject.SetActive(true);
            rewardText.color = textColor;
            rewardText.text = message;

            if (noticeCoroutine != null) StopCoroutine(noticeCoroutine);
            noticeCoroutine = StartCoroutine(HideNoticeRoutine(3f));
        }
    }

    IEnumerator HideNoticeRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rewardText != null)
        {
            rewardText.text = "";
            rewardText.gameObject.SetActive(false);
        }
    }

    public void BackToLobby()
    {
        if (isSpinning) return;
        SceneManager.LoadScene("Lobby");
    }

    // Tiện ích Context Menu để test nhanh mà không làm hỏng dữ liệu
    [ContextMenu("DEBUG: Đặt vé về 0")]
    public void DebugSetZeroTickets()
    {
        PlayerPrefs.SetInt("Ticket", 0);
        PlayerPrefs.Save();
        UpdateUI();
    }

    [ContextMenu("DEBUG: Cấp 99 vé")]
    public void DebugAddTickets()
    {
        PlayerPrefs.SetInt("Ticket", 99);
        PlayerPrefs.Save();
        UpdateUI();
    }
}
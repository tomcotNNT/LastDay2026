using UnityEngine;
using TMPro;
using System.Collections;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    [Header("Reward UI")]
    public GameObject winPanel;
    public TMP_Text starText;
    public TMP_Text moneyText;
    public TMP_Text ticketText;
    public TMP_Text upgradeShardText;

    [Header("Chapter")]
    public int chapterIndex = 1;

    [Header("Reward Result")]
    public int star;
    public int money;
    public int ticket;
    public int upgradeShard;

    [Header("Star UI")]
    public GameObject[] stars;

    private bool hasRewarded;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip starSound;

    void Awake()
    {
        Instance = this;

        if (winPanel != null)
            winPanel.SetActive(false);
    }

    public void GiveWinReward(float totalPlayerDamage)
    {
        if (hasRewarded)
            return;

        hasRewarded = true;

        //hiện curssor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        bool npcAlive = IsNPCAlive();

        if (npcAlive && totalPlayerDamage < 200f)
        {
            star = 5;
            money = Random.Range(250, 351);
            ticket = Random.Range(2, 4);
            upgradeShard = 5;
        }
        else if (totalPlayerDamage < 400f)
        {
            star = 3;
            money = Random.Range(100, 201);
            ticket = 1;
            upgradeShard = 2;
        }
        else
        {
            star = 1;
            money = Random.Range(50, 91);
            ticket = 0;
            upgradeShard = 1;
        }

        SaveReward();
        ShowRewardUI();
    }

    bool IsNPCAlive()
    {
        NPCHealth npc = FindObjectOfType<NPCHealth>();

        if (npc == null)
            return false;

        return !npc.IsDead;
    }

    void SaveReward()
    {
        PlayerPrefs.SetInt("Money", PlayerPrefs.GetInt("Money", 0) + money);
        PlayerPrefs.SetInt("Ticket", PlayerPrefs.GetInt("Ticket", 0) + ticket);
        PlayerPrefs.SetInt("UpgradeShard", PlayerPrefs.GetInt("UpgradeShard", 0) + upgradeShard);

        string key = "Chapter_" + chapterIndex + "_Stars";
        int oldStar = PlayerPrefs.GetInt(key, 0);

        if (star > oldStar)
            PlayerPrefs.SetInt(key, star);

        PlayerPrefs.Save();
    }

    void ShowRewardUI()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        if (starText != null)
            starText.text = star.ToString();

        if (moneyText != null)
            moneyText.text = money.ToString();

        if (ticketText != null)
            ticketText.text = ticket.ToString();

        if (upgradeShardText != null)
            upgradeShardText.text = upgradeShard.ToString();

        StartCoroutine(ShowStarsCoroutine());
    }

    IEnumerator ShowStarsCoroutine()
    {
        if (stars == null || stars.Length == 0)
            yield break;

        foreach (GameObject s in stars)
        {
            if (s != null)
                s.SetActive(false);
        }

        for (int i = 0; i < star && i < stars.Length; i++)
        {
            stars[i].SetActive(true);

            // phát âm thanh
            audioSource.PlayOneShot(starSound);

            yield return new WaitForSeconds(1f);
        }
    }
}
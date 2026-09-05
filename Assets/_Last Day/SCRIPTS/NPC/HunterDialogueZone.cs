using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HunterDialogueZone : MonoBehaviour, IInteractable
{
    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public Button nextButton;

    [Header("Dialogue Content 1")]
    [TextArea]
    public string[] dialogueLines ;
    public bool useSpeakerLabels = true;
    public string[] dialogueSpeakers ;

    [Header("Dialogue Content 2")]
    [TextArea]
    public string[] dialogueLines2 ;
    public bool useSpeakerLabels2 = true;
    public string[] dialogueSpeakers2 ;

    private bool firstDialogueFinished = false;
    private bool isPlayingSecondDialogue = false;
    private bool canStartSecondDialogue = false;

    public GameObject Hunter;
    public GameObject HunterIdle;

    [Header("Settings")]
    public bool hideOnExit = true;
    public bool autoAdvance = true;
    public float autoAdvanceDelay = 3f;
    public bool useNextButton = false;
    public bool useTypewriter = true;
    public float typewriterSpeed = 0.03f;
    public bool disableGunDuringDialogue = true;
    public bool disableAimDuringDialogue = true;
    public bool disableMacheteDuringDialogue = true;
    public float autoHideAfterSeconds = 0f;
    public float zombieSpawnDuration = 90f;

    [Header("Mutant Spawn")]
    public GameObject mutantPrefab;
    public Transform mutantSpawnPoint;
    public float mutantSpawnDelay = 3f;
    public float zombieSpawnInterval = 2f;
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.4f;
    public float secondDialogueCameraTime = 4f;

    private PlayerInteract currentPlayer;
    private GunSystem[] playerGunSystems;
    private AimSystem[] playerAimSystems;
    private MacheteSystem[] playerMacheteSystems;
    private FPSController playerController;
    private int currentLineIndex;
    private Coroutine dialogueCoroutine;
    private Coroutine hideCoroutine;
    private Coroutine typewriterCoroutine;
    private Coroutine zombieSpawnCoroutine;
    private bool isTyping;
    private bool dialogueEndedNaturally;
    public MutantIntro camIntro;
    public bool donedialoge2= false;

    void Awake()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // if(Hunter != null)
        //     Hunter.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerInteract player = other.GetComponentInParent<PlayerInteract>();
        if (player != null)
        {
            currentPlayer = player;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!hideOnExit)
            return;

        if (currentPlayer != null && other.GetComponentInParent<PlayerInteract>() == currentPlayer)
        {
            StopDialogue();
            currentPlayer = null;
        }
    }

    public void Interact(PlayerInteract player)
    {
        if (firstDialogueFinished && !canStartSecondDialogue)
        {
            Debug.Log("Chưa chạm Point3 nên chưa cho thoại đợt 2");
            return;
        }

        currentPlayer = player;
        StartDialogue();
        Debug.Log("Interacted with Hunter Dialogue Zone");
    }

    public void UnlockSecondDialogue()
    {
        canStartSecondDialogue = true;
        Debug.Log("Đã mở khóa hội thoại đợt 2");
    }

    public void StartDialogue()
    {
        if (dialoguePanel == null || dialogueText == null)
            return;
        isPlayingSecondDialogue = firstDialogueFinished;    

        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (zombieSpawnCoroutine != null)
        {
            StopCoroutine(zombieSpawnCoroutine);
            zombieSpawnCoroutine = null;
        }

        dialogueEndedNaturally = false;

        CollectPlayerSystems();

        if (disableGunDuringDialogue)
            SetGunControl(false);

        if (disableAimDuringDialogue)
            SetAimControl(false);

        if (disableMacheteDuringDialogue)
            SetMacheteControl(false);

        SetCameraControl(false);

        currentLineIndex = 0;
        dialoguePanel.SetActive(true);
        EnableCursor();

        if (useNextButton && nextButton != null)
        {
            nextButton.onClick.RemoveListener(AdvanceDialogue);
            nextButton.onClick.AddListener(AdvanceDialogue);
            ShowCurrentLine();
        }
        else if (autoAdvance)
        {
            dialogueCoroutine = StartCoroutine(PlayDialogueSequence());
        }
        else
        {
            ShowCurrentLine();
        }
    }

    public void StopDialogue()
    {
        if (dialoguePanel == null)
            return;

        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
            isTyping = false;
        }

        if (nextButton != null)
            nextButton.onClick.RemoveListener(AdvanceDialogue);

        dialoguePanel.SetActive(false);

        if (disableGunDuringDialogue)
            SetGunControl(true);

        if (disableAimDuringDialogue)
            SetAimControl(true);

        if (disableMacheteDuringDialogue)
            SetMacheteControl(true);

        SetCameraControl(true);
        RestoreCursor();
    }

    void ShowCurrentLine()
    {
        string[] lines = firstDialogueFinished ? dialogueLines2 : dialogueLines;
        string[] speakers = firstDialogueFinished ? dialogueSpeakers2 : dialogueSpeakers;
        bool speakerLabels = firstDialogueFinished ? useSpeakerLabels2 : useSpeakerLabels;

        if (lines == null || lines.Length == 0)
            return;

        currentLineIndex = Mathf.Clamp(currentLineIndex, 0, lines.Length - 1);

        string line = lines[currentLineIndex];
        string speaker = string.Empty;

        if (speakerLabels && speakers != null && currentLineIndex < speakers.Length)
            speaker = speakers[currentLineIndex];

        string formattedLine = string.IsNullOrEmpty(speaker)
            ? line
            : $"<b>{speaker}:</b> {line}";

        if (useTypewriter)
            StartTypewriter(formattedLine);
        else
            dialogueText.text = formattedLine;
    }

    void StartTypewriter(string line)
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        typewriterCoroutine = StartCoroutine(TypeText(line));
    }

    IEnumerator TypeText(string line)
    {
        isTyping = true;
        dialogueText.text = string.Empty;

        foreach (char letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        typewriterCoroutine = null;
    }

    void FinishTyping()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        string[] lines = isPlayingSecondDialogue ? dialogueLines2 : dialogueLines;
        string[] speakers = isPlayingSecondDialogue ? dialogueSpeakers2 : dialogueSpeakers;
        bool speakerLabels = isPlayingSecondDialogue ? useSpeakerLabels2 : useSpeakerLabels;

        if (lines != null && lines.Length > 0)
        {
            string line = lines[currentLineIndex];
            string speaker = string.Empty;

            if (speakerLabels && speakers != null && currentLineIndex < speakers.Length)
                speaker = speakers[currentLineIndex];

            dialogueText.text = string.IsNullOrEmpty(speaker)
                ? line
                : $"<b>{speaker}:</b> {line}";
        }

        isTyping = false;
    }

    public void AdvanceDialogue()
    {
        string[] lines = isPlayingSecondDialogue ? dialogueLines2 : dialogueLines;

        if (lines == null || lines.Length == 0)
            return;

        if (isTyping)
        {
            FinishTyping();
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= lines.Length)
        {
            dialogueEndedNaturally = true;

            if (!isPlayingSecondDialogue)
            {
                firstDialogueFinished = true;
                ScheduleZombieSpawn();
            }
            else
            {
                Debug.Log("Đã xong hội thoại đợt 2");
                HandleSecondDialogueComplete();
            }

            StopDialogue();
            return;
        }

        ShowCurrentLine();
    }
    void CollectPlayerSystems()
    {
        if (currentPlayer == null)
            return;

        playerGunSystems = currentPlayer.GetComponentsInChildren<GunSystem>(true);
        playerAimSystems = currentPlayer.GetComponentsInChildren<AimSystem>(true);
        playerMacheteSystems = currentPlayer.GetComponentsInChildren<MacheteSystem>(true);
        playerController = currentPlayer.GetComponentInParent<FPSController>();
    }

    void SetGunControl(bool enabled)
    {
        if (playerGunSystems == null || playerGunSystems.Length == 0)
            return;

        foreach (GunSystem gun in playerGunSystems)
        {
            if (gun != null)
                gun.enabled = enabled;
        }
    }

    void SetAimControl(bool enabled)
    {
        if (playerAimSystems == null || playerAimSystems.Length == 0)
            return;

        foreach (AimSystem aim in playerAimSystems)
        {
            if (aim != null)
                aim.enabled = enabled;
        }
    }

    void SetMacheteControl(bool enabled)
    {
        if (playerMacheteSystems == null || playerMacheteSystems.Length == 0)
            return;

        foreach (MacheteSystem machete in playerMacheteSystems)
        {
            if (machete != null)
                machete.enabled = enabled;
        }
    }

    void SetCameraControl(bool enabled)
    {
        if (playerController == null)
            return;

        playerController.SetLookEnabled(enabled);
    }

    IEnumerator PlayDialogueSequence()
    {
        string[] lines = firstDialogueFinished ? dialogueLines2 : dialogueLines;

        while (currentLineIndex < lines.Length)
        {
            ShowCurrentLine();

            if (useTypewriter)
                yield return new WaitUntil(() => !isTyping);

            if (currentLineIndex >= lines.Length - 1)
                break;

            yield return new WaitForSeconds(autoAdvanceDelay);
            currentLineIndex++;
        }

        if (autoHideAfterSeconds > 0f)
        {
            hideCoroutine = StartCoroutine(AutoHideAfterDelay(autoHideAfterSeconds));
        }

        if (!dialogueEndedNaturally)
        {
            dialogueEndedNaturally = true;

            // Logic thoại 1
            if (!firstDialogueFinished)
            {
                firstDialogueFinished = true;

                ScheduleZombieSpawn();

                Debug.Log("Talk to jake");

                if (Mission.Instance != null)
                    Mission.Instance.UIMission(3);
            }
            // Logic thoại 2
            else
            {
                Debug.Log("Xong hội thoại đợt 2");
                HandleSecondDialogueComplete();

                
            }
        }
    }

    System.Collections.IEnumerator AutoHideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopDialogue();
    }

    void ScheduleZombieSpawn()
    {           

        if (zombieSpawnCoroutine != null)
            return;

        if (Mission.Instance == null)
        {
            Debug.LogWarning("HunterDialogueZone: Mission instance not found for zombie spawn.");
            return;
        }

        if (Mission.Instance.ambientSpawner == null)
        {
            Debug.LogWarning("HunterDialogueZone: ambientSpawner reference missing in Mission.");
            return;
        }

        Mission.Instance.ambientSpawner.spawnInterval = zombieSpawnInterval;
        Debug.Log("HunterDialogueZone: Starting zombie spawn with " + Mission.Instance.ambientSpawner.spawnInterval + " second interval.");
        Mission.Instance.ambientSpawner.StartSpawn();

        Debug.Log("HunterDialogueZone: Zombie spawning started after dialogue completion.");
        zombieSpawnCoroutine = StartCoroutine(StopZombieSpawnAfterDelay(zombieSpawnDuration));
    }

    void HandleSecondDialogueComplete()
    {
        StartCoroutine(SecondDialogueCompleteSequence());
    }

    IEnumerator SecondDialogueCompleteSequence()
    {
        if (fadeCanvasGroup != null)
            yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));

        if (mutantSpawnDelay > 0f)
            yield return new WaitForSeconds(mutantSpawnDelay);

        Transform mutantTarget = mutantSpawnPoint;
        GameObject spawnedMutant = null;

        if (fadeCanvasGroup != null)
            yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));

        if (mutantPrefab != null && mutantSpawnPoint != null)
        {
            spawnedMutant = Instantiate(mutantPrefab, mutantSpawnPoint.position, mutantSpawnPoint.rotation);
            mutantTarget = spawnedMutant.transform;
            Debug.Log("HunterDialogueZone: Mutant spawned at spawn point after dialogue 2.");
        }
        else
        {
            Debug.LogWarning("HunterDialogueZone: mutantPrefab or mutantSpawnPoint not assigned.");
        }

        donedialoge2 = true;

        if (camIntro != null && mutantTarget != null)
            yield return StartCoroutine(camIntro.ShowMutant(mutantTarget, secondDialogueCameraTime));
        else
            yield return new WaitForSeconds(secondDialogueCameraTime);

        if (fadeCanvasGroup != null)
            yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));   

        if (fadeCanvasGroup != null)
            yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));                

        if (Mission.Instance != null)
            Mission.Instance.UIMission(4);

        Mission.Instance.ambientSpawner.StartSpawn();

        //hiện npc 
        if (Hunter != null)
            Hunter.SetActive(true);
        //tắt object cha
        if (HunterIdle != null)
            HunterIdle.SetActive(false);
        
    }

    IEnumerator FadeCanvas(float startAlpha, float endAlpha, float duration)
    {
        if (fadeCanvasGroup == null)
            yield break;

        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.alpha = startAlpha;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;
        if (Mathf.Approximately(endAlpha, 0f))
            fadeCanvasGroup.gameObject.SetActive(false);
    }

    IEnumerator StopZombieSpawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (Mission.Instance != null && Mission.Instance.ambientSpawner != null)
        {
            Mission.Instance.ambientSpawner.StopSpawn();
            Debug.Log("Đã dừng spawn zombie sau 90s");
        }

        // Chờ tới khi tất cả zombie bị giết
        while (GameObject.FindGameObjectsWithTag("Enemy").Length > 0)
        {
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("Đã hết zombie, hoàn thành nhiệm vụ");

        if (Mission.Instance != null)
            Mission.Instance.missionSystem.CompleteCurrentMission();



        if (Mission.Instance != null)
            Mission.Instance.UIMission(1);

        zombieSpawnCoroutine = null;
    }

    void EnableCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void RestoreCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsDialogueOpen()
    {
        return dialoguePanel != null && dialoguePanel.activeInHierarchy;
    }
}


//khi mutant chết thì dừng spawn zombie 
//khi hết zombie thì hoàn thành nhiệm vụ
//thêm enemy có thể random muc tiêu
//thêm Jake bắn cùng

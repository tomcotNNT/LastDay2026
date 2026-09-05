using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameSetting : MonoBehaviour
{
    [Header("UI")]
    public GameObject optionPanel;
    public GameObject loadingObj;
    [Header("Pause")]
    public bool pauseWhenOpen = true;
    private bool isOptionOpen;

    void Start()
    {
        if (optionPanel != null)
            optionPanel.SetActive(false);

         // tự tìm Loading
        loadingObj = GameObject.FindGameObjectWithTag("Loading");
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            bool panelOpening = optionPanel != null && optionPanel.activeSelf;

            if (panelOpening)
                CloseOption();
            else
                OpenOption();
        }
    }

    public void OpenOption()
    {
        if (loadingObj != null && loadingObj.activeInHierarchy)
            return;

        isOptionOpen = true;

        if (optionPanel != null)
            optionPanel.SetActive(true);

        Time.timeScale = 0f;
        AudioListener.pause = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        CancelGunFire();

        Debug.Log("Open Option | TimeScale = " + Time.timeScale);
    }
    
    public void CloseOption()
    {
        isOptionOpen = false;

        if (optionPanel != null)
            optionPanel.SetActive(false);

        Time.timeScale = 1f;
        AudioListener.pause = false;

        StartCoroutine(LockCursorNextFrame());

        CancelGunFire();

        Debug.Log("Close Option | TimeScale = " + Time.timeScale);
    }

    IEnumerator LockCursorNextFrame()
    {
        yield return null;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void CancelGunFire()
    {
        foreach (var gun in FindObjectsOfType<GunSystem>())
            gun.CancelFire();
    }

    public void BackToMenu()
    {   
        Time.timeScale = 1f;
        AudioListener.pause = false;

        //Load scene MapMenu
        SceneManager.LoadScene("MapMenu");
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        //Load lại scene hiện tại
        SceneManager.LoadScene("Map1");
    }


    void ApplyPauseState()
    {
        if (isOptionOpen==true)
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
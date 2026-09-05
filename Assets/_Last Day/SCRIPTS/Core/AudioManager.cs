using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioMixer mixer;

    public float environment = 0.5f;
    public float master = 0.5f;
    public float sfx = 0.5f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
        ApplyAll();
    }

    public void ChangeEnvironment(float amount)
    {
        environment = Mathf.Clamp01(environment + amount);
        SaveAndApply("EnvironmentVolume", environment);
    }

    public void ChangeMaster(float amount)
    {
        master = Mathf.Clamp01(master + amount);
        SaveAndApply("MasterVolume", master);
    }

    public void ChangeSFX(float amount)
    {
        sfx = Mathf.Clamp01(sfx + amount);
        SaveAndApply("SFXVolume", sfx);
    }

    void Load()
    {
        environment = PlayerPrefs.GetFloat("EnvironmentVolume", 0.5f);
        master = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        sfx = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
    }

    void ApplyAll()
    {
        SetMixer("EnvironmentVolume", environment);
        SetMixer("MasterVolume", master);
        SetMixer("SFXVolume", sfx);
    }

    void SaveAndApply(string key, float value)
    {
        SetMixer(key, value);
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    void SetMixer(string key, float value)
    {
        // Kiểm tra null an toàn khi chạy Test Runner
        if (mixer != null)
        {
            mixer.SetFloat(key, value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f);
        }
    }

    void OnApplicationQuit()
    {
        SaveAll();
    }

    void SaveAll()
    {
        PlayerPrefs.SetFloat("EnvironmentVolume", environment);
        PlayerPrefs.SetFloat("MasterVolume", master);
        PlayerPrefs.SetFloat("SFXVolume", sfx);
        PlayerPrefs.Save();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveAll();
    }
}
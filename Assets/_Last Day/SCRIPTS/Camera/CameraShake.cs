using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraShake : MonoBehaviour
{
    public float defaultDuration = 0.5f;
    public float defaultMagnitude = 0.5f;

    private Vector3 originalLocalPosition;
    private float shakeDuration;
    private float shakeMagnitude;
    private float shakeTimer;
    private bool isShaking;

    void Awake()
    {
        originalLocalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        if (!isShaking)
            return;

        shakeTimer += Time.deltaTime;
        if (shakeTimer >= shakeDuration)
        {
            isShaking = false;
            transform.localPosition = originalLocalPosition;
            return;
        }

        float x = Random.Range(-1f, 1f) * shakeMagnitude;
        float y = Random.Range(-1f, 1f) * shakeMagnitude;
        transform.localPosition = originalLocalPosition + new Vector3(x, y, 0f);
    }

    public void Shake()
    {
        Debug.Log("CameraShake: Shake() called (no args)");
        Shake(defaultDuration, defaultMagnitude);
    }

    public void Shake(float duration)
    {
        Debug.Log($"CameraShake: Shake(duration={duration}) called");
        Shake(duration, defaultMagnitude);
    }

    public void Shake(float duration, float magnitude)
    {
        Debug.Log($"CameraShake: Shake(duration={duration}, magnitude={magnitude}) called");
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        shakeTimer = 0f;
        isShaking = true;
    }

    public void StopShake()
    {
        isShaking = false;
        shakeTimer = 0f;
        transform.localPosition = originalLocalPosition;
    }
}

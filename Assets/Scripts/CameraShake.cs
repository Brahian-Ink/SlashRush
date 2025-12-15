using UnityEngine;

public class CameraShake : MonoBehaviour
{
    Vector3 originalPos;

    float shakeTimer;
    float shakeIntensity;

    bool continuousShake = false;
    float continuousIntensity = 0f;

    bool shakeEnabled = true;

    void Awake()
    {
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (!shakeEnabled)
        {
            transform.localPosition = originalPos;
            return;
        }

        if (continuousShake)
        {
            transform.localPosition = originalPos +
                (Vector3)Random.insideUnitCircle * continuousIntensity;
            return;
        }

        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.unscaledDeltaTime;

            transform.localPosition = originalPos +
                (Vector3)Random.insideUnitCircle * shakeIntensity;
        }
        else
        {
            transform.localPosition = originalPos;
        }
    }

    public void Shake(float duration, float intensity)
    {
        if (!shakeEnabled) return;

        shakeTimer = duration;
        shakeIntensity = intensity;
    }

    public void StartContinuousShake(float intensity)
    {
        if (!shakeEnabled) return;

        continuousShake = true;
        continuousIntensity = intensity;
    }

    public void StopContinuousShake()
    {
        continuousShake = false;
        transform.localPosition = originalPos;
    }

    public void StopAllShake()
    {
        shakeTimer = 0f;
        shakeIntensity = 0f;
        continuousShake = false;
        continuousIntensity = 0f;
        transform.localPosition = originalPos;
    }

    public void SetEnabled(bool enabled)
    {
        shakeEnabled = enabled;

        if (!shakeEnabled)
            StopAllShake();
    }
}

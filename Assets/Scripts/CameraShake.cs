using UnityEngine;

public class CameraShake : MonoBehaviour
{
    Vector3 originalPos;

    // Shake por impacto
    float shakeTimer;
    float shakeIntensity;

    // Shake continuo (PowerUp)
    bool continuousShake = false;
    float continuousIntensity = 0f;

    void Awake()
    {
        originalPos = transform.localPosition;
    }

    void Update()
    {
        // -----------------------------------------
        // SHAKE CONTINUO (POWER UP)
        // -----------------------------------------
        if (continuousShake)
        {
            transform.localPosition = originalPos +
                (Vector3)(Random.insideUnitCircle * continuousIntensity);
            return; 
        }

        // -----------------------------------------
        // SHAKE CORTO POR IMPACTO
        // -----------------------------------------
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;

            transform.localPosition = originalPos +
                (Vector3)(Random.insideUnitCircle * shakeIntensity);
        }
        else
        {
            transform.localPosition = originalPos;
        }
    }

    public void Shake(float duration, float intensity)
    {
        shakeTimer = duration;
        shakeIntensity = intensity;
    }

    public void StartContinuousShake(float intensity)
    {
        continuousShake = true;
        continuousIntensity = intensity;
    }

    public void StopContinuousShake()
    {
        continuousShake = false;
        transform.localPosition = originalPos;
    }
}

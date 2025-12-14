using UnityEngine;
using TMPro;

public class SlasherPulseText : MonoBehaviour
{
    public PlayerController player;
    public TMP_Text text;

    [Header("Pulse")]
    public float pulseSpeed = 1.6f;
    public float minScale = 0.95f;
    public float maxScale = 1.08f;
    public float hiddenAlpha = 0f;
    public float minAlpha = 0.5f;
    public float maxAlpha = 1f;

    Vector3 baseScale;
    Color baseColor;

    void Awake()
    {
        if (!text) text = GetComponent<TMP_Text>();
        if (!player) player = Object.FindFirstObjectByType<PlayerController>();

        baseScale = transform.localScale;
        baseColor = text.color;
    }

    void Update()
    {
        if (!player || !text) return;

        float t = Mathf.Clamp01(player.buffRemaining01);
        bool active = t > 0.001f;

        // pulso 0..1 en tiempo real (no depende del timeScale)
        float p = Mathf.Sin(Time.unscaledTime * pulseSpeed) * 0.5f + 0.5f;

        // escala
        float scale = active ? Mathf.Lerp(minScale, maxScale, p) : 1f;
        transform.localScale = baseScale * scale;

        // alpha
        float a = active ? Mathf.Lerp(minAlpha, maxAlpha, p) : hiddenAlpha;
        Color c = baseColor;
        c.a = a;
        text.color = c;
    }
}

using UnityEngine;
using TMPro;

public class ComboPopup : MonoBehaviour
{
    public TMP_Text comboText;
    public Transform player;             
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    [Header("Juice")]
    public float shakeIntensity = 0.05f;
    public float shakeDuration = 0.15f;
    public float popScale = 1.4f;
    public float popSpeed = 8f;

    Vector3 defaultScale;
    float shakeTimer;

    void Awake()
    {
        defaultScale = transform.localScale;
        comboText.text = "";
    }

    void LateUpdate()
    {
        if (player != null)
            transform.position = player.position + offset;

        // vibración
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            transform.position += (Vector3)Random.insideUnitCircle * shakeIntensity;
        }

        transform.localScale = Vector3.Lerp(transform.localScale, defaultScale, Time.deltaTime * popSpeed);

        transform.rotation = Quaternion.identity;
    }

    public void ShowCombo(int combo)
    {
        if (combo <= 1)
        {
            comboText.text = "";
            return;
        }

        comboText.text = "COMBO X" + combo;

        // POP
        transform.localScale = defaultScale * popScale;

        // Shake
        shakeTimer = shakeDuration;
    }

}

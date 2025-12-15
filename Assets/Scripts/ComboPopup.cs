using System.Collections;
using TMPro;
using UnityEngine;

public class ComboPopup : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text comboText;

    [Header("FX")]
    public float showTime = 0.35f;
    public float popScale = 1.35f;

    Coroutine routine;

    void Awake()
    {
        if (comboText != null)
            comboText.gameObject.SetActive(false);
    }

    public void ShowCombo(int combo)
    {
        if (comboText == null) return;

        // no mostrar x0 / x1
        if (combo <= 1)
        {
            comboText.gameObject.SetActive(false);
            return;
        }

        comboText.text = "COMBO X" + combo;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(PlayFx());
    }

    IEnumerator PlayFx()
    {
        comboText.gameObject.SetActive(true);

        // reset
        float baseAlpha = 1f;
        Vector3 baseScale = Vector3.one;
        comboText.alpha = baseAlpha;
        comboText.transform.localScale = baseScale * popScale;

        // pequeño pop back
        float t = 0f;
        while (t < 0.12f)
        {
            t += Time.unscaledDeltaTime;
            comboText.transform.localScale = Vector3.Lerp(comboText.transform.localScale, baseScale, t / 0.12f);
            yield return null;
        }

        // esperar visible
        float timer = 0f;
        while (timer < showTime)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        // fade out
        t = 0f;
        while (t < 0.18f)
        {
            t += Time.unscaledDeltaTime;
            comboText.alpha = Mathf.Lerp(1f, 0f, t / 0.18f);
            yield return null;
        }

        comboText.gameObject.SetActive(false);
    }
}

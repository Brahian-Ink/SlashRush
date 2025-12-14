using UnityEngine;
using TMPro;

public class TimePopup : MonoBehaviour
{
    public float floatSpeed = 1.2f;
    public float lifeTime = 1f;

    TMP_Text text;
    Color color;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
        color = text.color;
    }

    void Update()
    {
        // subir (independiente del slow motion)
        transform.position += Vector3.up * floatSpeed * Time.unscaledDeltaTime;

        // fade out
        color.a -= Time.unscaledDeltaTime / lifeTime;
        text.color = color;

        if (color.a <= 0f)
            Destroy(gameObject);
    }
}

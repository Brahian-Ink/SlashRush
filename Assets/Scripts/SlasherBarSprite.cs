using UnityEngine;

public class SlasherBarSprite : MonoBehaviour
{
    public PlayerController player;
    public RectTransform fill;

    [Header("Visuals")]
    public GameObject visuals; // arrastrá acá el BG o un contenedor

    void Awake()
    {
        // Por si te olvidás de arrastrar el player
        if (!player)
            player = Object.FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        if (!player || !fill) return;

        float t = Mathf.Clamp01(player.buffRemaining01);

        // NO apagues este GameObject (BarRoot). Apagá solo los visuals.
        if (visuals) visuals.SetActive(t > 0.001f);

        Vector3 s = fill.localScale;
        s.y = t;                // vertical
        fill.localScale = s;
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class StudioIntroController : MonoBehaviour
{
    public float duration = 5f;        // tiempo antes de pasar al menu
    public string menuSceneName = "Menu";

    public Image logo;                 // opcional si querés fade

    void Start()
    {
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        // FUNDIDO opcional (fade-in)
        if (logo != null)
        {
            Color c = logo.color;
            c.a = 0f;
            logo.color = c;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime;
                logo.color = new Color(c.r, c.g, c.b, t);
                yield return null;
            }
        }

        // esperar 5 segundos
        yield return new WaitForSeconds(duration);

        // cargar menu
        SceneManager.LoadScene(menuSceneName);
    }
}

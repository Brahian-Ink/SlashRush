using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("Buttons")]
    public Button playBtn;
    public Button controlsBtn;
    public Button exitBtn;
    public Button backBtn;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSFX;
    public AudioClip clickSFX;

    [Header("UI Texts")]
    public TMP_Text highScoreText;
    public GameObject creditsGroup;   // ← contenedor con tu nombre + studio

    [Header("Panels")]
    public GameObject controlsPanel;

    void Start()
    {
        // listeners
        playBtn.onClick.AddListener(PlayGame);
        controlsBtn.onClick.AddListener(OpenControls);
        exitBtn.onClick.AddListener(ExitGame);
        backBtn.onClick.AddListener(BackToMenu);

        // estado inicial
        controlsPanel.SetActive(false);
        backBtn.gameObject.SetActive(false);

        if (creditsGroup != null)
            creditsGroup.SetActive(true);

        // mostrar high score
        int high = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = "HIGH SCORE: " + high.ToString("000000");
    }

    // ---------------------------------------------------------
    // AUDIO
    // ---------------------------------------------------------
    public void PlayHoverSound()
    {
        if (audioSource && hoverSFX)
            audioSource.PlayOneShot(hoverSFX);
    }

    void PlayClickSound()
    {
        if (audioSource && clickSFX)
            audioSource.PlayOneShot(clickSFX);
    }

    // ---------------------------------------------------------
    // MENU ACTIONS
    // ---------------------------------------------------------
    void PlayGame()
    {
        PlayClickSound();
        SceneManager.LoadScene("Game");
    }

    void OpenControls()
    {
        PlayClickSound();

        controlsPanel.SetActive(true);
        backBtn.gameObject.SetActive(true);

        // ocultar UI principal
        if (highScoreText != null)
            highScoreText.gameObject.SetActive(false);

        if (creditsGroup != null)
            creditsGroup.SetActive(false);

        playBtn.gameObject.SetActive(false);
        controlsBtn.gameObject.SetActive(false);
        exitBtn.gameObject.SetActive(false);
    }

    void BackToMenu()
    {
        PlayClickSound();

        controlsPanel.SetActive(false);
        backBtn.gameObject.SetActive(false);

        // restaurar UI principal
        if (highScoreText != null)
            highScoreText.gameObject.SetActive(true);

        if (creditsGroup != null)
            creditsGroup.SetActive(true);

        playBtn.gameObject.SetActive(true);
        controlsBtn.gameObject.SetActive(true);
        exitBtn.gameObject.SetActive(true);
    }

    void ExitGame()
    {
        PlayClickSound();
        Debug.Log("EXIT PRESSED — CERRANDO JUEGO...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

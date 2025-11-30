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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hoverSFX;
    public AudioClip clickSFX;
    [Header("UI Texts")]
    public TMP_Text highScoreText;   // ← acá va el texto de HIGH SCORE



    [Header("Panels")]
    public GameObject controlsPanel;

    void Start()
    {
        // listeners de botones
        playBtn.onClick.AddListener(PlayGame);
        controlsBtn.onClick.AddListener(OpenControls);
        exitBtn.onClick.AddListener(ExitGame);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        // leer y mostrar high score
        int high = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = "HIGH SCORE: " + high.ToString("000000");
    }

    void Update()
    {
        if (controlsPanel != null && controlsPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            controlsPanel.SetActive(false);
    }


    public void PlayHoverSound()
    {
        if (audioSource && hoverSFX)
            audioSource.PlayOneShot(hoverSFX);
    }

    public void PlayClickSound()
    {
        if (audioSource && clickSFX)
            audioSource.PlayOneShot(clickSFX);
    }

    void PlayGame()
    {
        PlayClickSound();
        SceneManager.LoadScene("Game"); 
    }

    void OpenControls()
    {
        PlayClickSound();
        if (controlsPanel != null)
            controlsPanel.SetActive(true);
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

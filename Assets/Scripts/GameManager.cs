using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // ---------------------------------------------------------
    // SCORE + COMBO
    // ---------------------------------------------------------
    [Header("Score")]
    public int score;
    public int baseKillPoints = 100;
    public int comboBonusPerKill = 50;
    private int lastScoreMilestone = 0;

    [Header("Combo")]
    public float comboWindow = 1f;
    public int comboCount = 0;
    float comboTimer = 0f;

    // PowerUp thresholds
    int nextBuffAt = 5000;
    public PlayerController player;   

    // ---------------------------------------------------------
    // WAVE + TIMER
    // ---------------------------------------------------------
    [Header("Wave System")]
    public int currentWave = 1;
    public int enemiesToSpawn = 3;
    public int enemiesAlive = 0;

    [Header("Timer")]
    public float waveTime = 60f;
    float remainingTime;
    public float timeDecreaseMultiplier = 40f;
    private int lastAnnouncedSecond = -1;
    public AudioClip countdownBeep;

    bool waveRunning = false;

    // ---------------------------------------------------------
    // SPAWNING
    // ---------------------------------------------------------
    [Header("Spawning")]
    public GameObject teenPrefab;
    public Transform[] spawnPoints;

    // ---------------------------------------------------------
    // UI
    // ---------------------------------------------------------
    [Header("UI")]
    public TMP_Text waveText;
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public TMP_Text gameOverText;
    public TMP_Text highScoreText;
    public TMP_Text pressEnterText;

    // ---------------------------------------------------------
    // FX CAMERA + OVERLAY
    // ---------------------------------------------------------
    [Header("FX")]
    public ComboPopup comboPopup;
    public CameraShake camShake;
    public Image redOverlay;

    // ---------------------------------------------------------
    // AUDIO
    // ---------------------------------------------------------
    public AudioSource audioSource;
    public AudioClip comboClip;
    public AudioClip waveStartClip;
    public AudioClip gameOverClip;

    // ---------------------------------------------------------
    // INTERNAL
    // ---------------------------------------------------------
    bool isGameOver = false;

    // ---------------------------------------------------------
    // INITIALIZATION
    // ---------------------------------------------------------
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Ocultar overlay rojo
        if (redOverlay != null)
        {
            Color c = redOverlay.color;
            c.a = 0f;
            redOverlay.color = c;
        }

        gameOverText.gameObject.SetActive(false);
        highScoreText.gameObject.SetActive(false);
        pressEnterText.gameObject.SetActive(false);

        remainingTime = waveTime;

        StartCoroutine(StartWave());
    }

    // ---------------------------------------------------------
    // MAIN UPDATE LOOP
    // ---------------------------------------------------------
    void Update()
    {
        HandleComboTimer();
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 1f; // por si estabas en game over
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
            return;
        }
        // Restart con ENTER
        if (isGameOver && Input.GetKeyDown(KeyCode.Return))
        {
            RestartGame();
            return;
        }

        if (!waveRunning) return;

        //-------------------------------------
        // TIMER
        //-------------------------------------
        float speedFactor = 1f + (currentWave - 1) * timeDecreaseMultiplier;
        remainingTime -= Time.deltaTime * speedFactor;

        UpdateTimerUI();

        int sec = Mathf.CeilToInt(remainingTime);

        if (sec != lastAnnouncedSecond)
        {
            lastAnnouncedSecond = sec;

            if (sec <= 10 && sec > 0)
            {
                if (audioSource && countdownBeep)
                    audioSource.PlayOneShot(countdownBeep);
            }

            if (sec <= 0)
            {
                GameOver();
            }
        }
    }

    // ---------------------------------------------------------
    // WAVE START
    // ---------------------------------------------------------
    IEnumerator StartWave()
    {
        waveRunning = false;

        waveText.gameObject.SetActive(true);
        waveText.text = "WAVE " + currentWave;

        if (audioSource && waveStartClip)
            audioSource.PlayOneShot(waveStartClip);

        yield return new WaitForSeconds(5f);

        waveText.gameObject.SetActive(false);

        enemiesToSpawn = currentWave * 10;
        enemiesAlive = enemiesToSpawn;

        SpawnWave();

        waveRunning = true;
    }

    void SpawnWave()
    {
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Transform p = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(teenPrefab, p.position, Quaternion.identity);
        }
    }

    // ---------------------------------------------------------
    // REGISTER KILL
    // ---------------------------------------------------------
    public void RegisterKill()
    {
        //-----------------------------------------------------
        // COMBO
        //-----------------------------------------------------
        if (comboTimer > 0f) comboCount++;
        else comboCount = 1;

        comboTimer = comboWindow;

        int gained = baseKillPoints + comboBonusPerKill * (comboCount - 1);
        score += gained;

        UpdateScoreUI();
        comboPopup?.ShowCombo(comboCount);

        if (comboCount > 1 && audioSource && comboClip)
            audioSource.PlayOneShot(comboClip);

        camShake?.Shake(0.08f, 0.06f);


        //-----------------------------------------------------
        // POWER UP CHECK
        //-----------------------------------------------------
        CheckPowerUp();

        //-----------------------------------------------------
        // WAVE CONTROL
        //-----------------------------------------------------
        enemiesAlive--;

        if (enemiesAlive <= 0)
        {
            waveRunning = false;
            currentWave++;
            StartCoroutine(StartWave());
        }
    }

    // ---------------------------------------------------------
    // POWER-UP
    // ---------------------------------------------------------
    void CheckPowerUp()
    {
        if (score >= nextBuffAt)
        {
            Debug.Log(">>> ACTIVANDO POWER-UP EN SCORE: " + score);

            if (player != null)
                player.ActivatePowerUp();
            else
                Debug.LogWarning("Player no asignado en GameManager.");

            nextBuffAt += 5000;
        }
    }

    // ---------------------------------------------------------
    // COMBO TIMER
    // ---------------------------------------------------------
    void HandleComboTimer()
    {
        if (comboCount > 0)
        {
            comboTimer -= Time.deltaTime;

            if (comboTimer <= 0f)
            {
                comboCount = 0;
                comboPopup?.ShowCombo(0);
            }
        }
    }

    // ---------------------------------------------------------
    // GAME OVER
    // ---------------------------------------------------------
    void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        waveRunning = false;

        if (audioSource && gameOverClip)
            audioSource.PlayOneShot(gameOverClip);

        gameOverText.gameObject.SetActive(true);

        int high = PlayerPrefs.GetInt("HighScore", 0);

        if (score > high)
        {
            PlayerPrefs.SetInt("HighScore", score);
            high = score;
        }

        highScoreText.text = "RECORD: " + high;
        highScoreText.gameObject.SetActive(true);

        pressEnterText.gameObject.SetActive(true);
        StartCoroutine(BlinkPressEnter());
        StartCoroutine(FadeRedOverlay());

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ---------------------------------------------------------
    // UI FUNCTIONS
    // ---------------------------------------------------------
    void UpdateScoreUI()
    {
        scoreText.text = "SCORE " + score.ToString("000000");

        int milestone = (score / 1000) * 1000;
        if (milestone > lastScoreMilestone)
        {
            lastScoreMilestone = milestone;
            StartCoroutine(ScorePopupEffect());
        }
    }

    void UpdateTimerUI()
    {
        timerText.text = "TIMER: " + Mathf.CeilToInt(remainingTime);
    }

    IEnumerator ScorePopupEffect()
    {
        Vector3 originalScale = scoreText.transform.localScale;
        scoreText.transform.localScale = originalScale * 1.4f;

        Color baseColor = scoreText.color;

        for (int i = 0; i < 6; i++)
        {
            scoreText.color = (i % 2 == 0) ? Color.white : new Color(1, 1, 1, 0.3f);
            yield return new WaitForSeconds(0.1f);
        }

        scoreText.color = baseColor;

        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            scoreText.transform.localScale =
                Vector3.Lerp(scoreText.transform.localScale, originalScale, t / 0.15f);
            yield return null;
        }

        scoreText.transform.localScale = originalScale;
    }

    IEnumerator FadeRedOverlay()
    {
        Color c = redOverlay.color;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(0f, 1f, t);
            redOverlay.color = c;
            yield return null;
        }
    }

    IEnumerator BlinkPressEnter()
    {
        while (true)
        {
            pressEnterText.alpha = 1f;
            yield return new WaitForSecondsRealtime(0.5f);

            pressEnterText.alpha = 0f;
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
}

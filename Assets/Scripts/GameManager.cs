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
    public int comboBonusFixed = 50; // bonus fijo por combo
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
    public int enemiesToSpawn = 2;
    public int enemiesAlive = 0;

    [Header("Timer")]
    public float waveTime = 60f;
    public float maxTime = 60f;
    float remainingTime;
    public float timeDecreaseMultiplier = 0.25f;
    private int lastAnnouncedSecond = -1;
    public AudioClip countdownBeep;
    bool waveRunning = false;

    // ---------------------------------------------------------
    // TIME BONUS POPUP
    // ---------------------------------------------------------
    [Header("Time Bonus Popup")]
    public GameObject timePopupPrefab;
    public Vector3 popupOffset = new Vector3(0f, 1.4f, 0f);

    // ---------------------------------------------------------
    // SPAWNING
    // ---------------------------------------------------------
    [Header("Spawning")]
    public GameObject teenPrefab;
    public Transform[] spawnPoints;

    [Header("Spawn Flow")]
    public float spawnDelayMin = 0.15f;
    public float spawnDelayMax = 0.6f;

    [Header("Anti-Spawn Camp")]
    public float spawnBlockRadius = 2.5f;
    public int spawnPickTries = 20;

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
    public Button restartButton;
    public Button menuButton;

    // ---------------------------------------------------------
    // FX
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
    Coroutine spawnRoutineHandle;

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
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
            restartButton.onClick.AddListener(RestartGame);
        }

        if (menuButton != null)
        {
            menuButton.gameObject.SetActive(false);
            menuButton.onClick.AddListener(BackToMenu);
        }
        // Ocultar overlay rojo
        if (redOverlay != null)
        {
            Color c = redOverlay.color;
            c.a = 0f;
            redOverlay.color = c;
        }

        if (gameOverText) gameOverText.gameObject.SetActive(false);
        if (highScoreText) highScoreText.gameObject.SetActive(false);
        if (pressEnterText) pressEnterText.gameObject.SetActive(false);

        remainingTime = Mathf.Min(waveTime, maxTime);
        UpdateTimerUI();
        UpdateScoreUI();

        StartCoroutine(StartWave());
    }

    // ---------------------------------------------------------
    // MAIN UPDATE LOOP
    // ---------------------------------------------------------
    void Update()
    {
        HandleComboTimer();

        // ESC -> menú
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;

            if (player != null)
                player.ForceStopSlasherMode();

            SceneManager.LoadScene("Menu");
            return;
        }

        // ENTER -> restart en game over
        if (isGameOver && Input.GetKeyDown(KeyCode.Return))
        {
            RestartGame();
            return;
        }

        if (!waveRunning) return;

        // TIMER
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
                GameOver();
        }
    }

    // ---------------------------------------------------------
    // WAVE START
    // ---------------------------------------------------------
    IEnumerator StartWave()
    {
        waveRunning = false;

        if (waveText)
        {
            waveText.gameObject.SetActive(true);
            waveText.text = "WAVE " + currentWave;
        }

        if (audioSource && waveStartClip)
            audioSource.PlayOneShot(waveStartClip);

        yield return new WaitForSeconds(2.5f);

        if (waveText) waveText.gameObject.SetActive(false);

        enemiesToSpawn = currentWave * 10;
        enemiesAlive = enemiesToSpawn;

        // cancelar rutina anterior por seguridad
        if (spawnRoutineHandle != null)
            StopCoroutine(spawnRoutineHandle);

        spawnRoutineHandle = StartCoroutine(SpawnWaveRoutine(enemiesToSpawn));

        waveRunning = true;
    }

    IEnumerator SpawnWaveRoutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Transform sp = GetSpawnPointAvoidingPlayer();
            if (sp != null && teenPrefab != null)
                Instantiate(teenPrefab, sp.position, Quaternion.identity);

            float d = Random.Range(spawnDelayMin, spawnDelayMax);
            yield return new WaitForSeconds(d);
        }
    }

    Transform GetSpawnPointAvoidingPlayer()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return null;

        if (player == null)
        {
            Debug.LogWarning("GameManager: 'player' NO asignado. Anti-camp no funciona.");
            return spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        Vector2 ppos = player.transform.position;

        // 1) Buscar spawns válidos (fuera del radio)
        int validCount = 0;
        Transform best = null;
        float bestDist = -1f;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform sp = spawnPoints[i];
            if (sp == null) continue;

            float dist = Vector2.Distance(sp.position, ppos);

            // guardo el más lejano siempre (por si no hay válidos)
            if (dist > bestDist)
            {
                bestDist = dist;
                best = sp;
            }

            if (dist >= spawnBlockRadius)
                validCount++;
        }

        // 2) Si hay válidos, elegimos uno válido random
        if (validCount > 0)
        {
            int pick = Random.Range(0, validCount);
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Transform sp = spawnPoints[i];
                if (sp == null) continue;

                float dist = Vector2.Distance(sp.position, ppos);
                if (dist >= spawnBlockRadius)
                {
                    if (pick == 0) return sp;
                    pick--;
                }
            }
        }

        // 3) Si NO hay válidos, spawnea en el MÁS LEJANO (anti-campeo total)
        return best;
    }


    // ---------------------------------------------------------
    // REGISTER KILL
    // ---------------------------------------------------------
    public void RegisterKill()
    {
        if (isGameOver) return;

        #if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
        #endif

        // COMBO
        if (comboTimer > 0f) comboCount++;
        else comboCount = 1;

        comboTimer = comboWindow;

        int gained = baseKillPoints;
        if (comboCount > 1)
            gained += comboBonusFixed;

        score += gained;

        // +5s solo en slasher mode (clamp a 60)
        if (player != null && player.isBuffed)
        {
            AddTime(5f);
            SpawnTimePopup("+5");
        }

        UpdateScoreUI();
        comboPopup?.ShowCombo(comboCount);

        if (comboCount > 1 && audioSource && comboClip)
            audioSource.PlayOneShot(comboClip);
        
        if (!isGameOver)
            camShake?.Shake(0.08f, 0.06f);

        // POWER UP
        CheckPowerUp();

        // WAVE
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
        if (score < nextBuffAt) return;

        if (player != null)
            player.ActivatePowerUp();
        else
            Debug.LogWarning("Player no asignado en GameManager.");

        nextBuffAt += 5000;
    }

    // ---------------------------------------------------------
    // COMBO TIMER
    // ---------------------------------------------------------
    void HandleComboTimer()
    {
        if (comboCount <= 0) return;

        comboTimer -= Time.deltaTime;
        if (comboTimer <= 0f)
        {
            comboCount = 0;
            comboPopup?.ShowCombo(0);
        }
    }

    // ---------------------------------------------------------
    // TIME HELPERS
    // ---------------------------------------------------------
    public void AddTime(float seconds)
    {
        remainingTime = Mathf.Min(remainingTime + seconds, maxTime);
        UpdateTimerUI();
    }

    void SpawnTimePopup(string txt)
    {
        if (!timePopupPrefab || player == null) return;

        Vector3 pos = player.transform.position + popupOffset;
        GameObject g = Instantiate(timePopupPrefab, pos, Quaternion.identity);

        TMP_Text t = g.GetComponent<TMP_Text>();
        if (t) t.text = txt;
    }

    // ---------------------------------------------------------
    // GAME OVER
    // ---------------------------------------------------------
    void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        waveRunning = false;

        if (restartButton != null)
            restartButton.gameObject.SetActive(true);

        if (menuButton != null)
            menuButton.gameObject.SetActive(true);
        // cortar spawns pendientes
        if (spawnRoutineHandle != null)
            StopCoroutine(spawnRoutineHandle);

        // cortar slasher y restaurar tiempos
        if (player != null)
            player.ForceStopSlasherMode();

        // cortar shake si tenés continuo (por seguridad)
        if (camShake != null)
            camShake.StopContinuousShake();

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (audioSource && gameOverClip)
            audioSource.PlayOneShot(gameOverClip);

        if (gameOverText) gameOverText.gameObject.SetActive(true);

        int high = PlayerPrefs.GetInt("HighScore", 0);
        if (score > high)
        {
            PlayerPrefs.SetInt("HighScore", score);
            high = score;
        }

        if (highScoreText)
        {
            highScoreText.text = "RECORD: " + high;
            highScoreText.gameObject.SetActive(true);
        }

        if (pressEnterText)
        {
            pressEnterText.gameObject.SetActive(true);
            StartCoroutine(BlinkPressEnter());
        }

        if (redOverlay != null)
            StartCoroutine(FadeRedOverlay());

        // freeze final
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void BackToMenu()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (camShake != null)
        {
            camShake.StopAllShake();
            camShake.SetEnabled(true);
        }

        SceneManager.LoadScene("Menu");
    }

    // ---------------------------------------------------------
    // UI
    // ---------------------------------------------------------
    void UpdateScoreUI()
    {
        if (!scoreText) return;

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
        if (!timerText) return;
        timerText.text = "TIMER: " + Mathf.CeilToInt(remainingTime);
    }

    IEnumerator ScorePopupEffect()
    {
        if (!scoreText) yield break;

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
        if (redOverlay == null) yield break;

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

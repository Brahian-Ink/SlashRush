using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public Camera cam;
    public Animator animator;

    Vector2 movement;
    Vector2 mousePos;
    bool isAttacking = false;

    float baseMoveSpeed;
    float baseAttackDuration;

    [Header("Sprite del Player")]
    public SpriteRenderer bodySR;

    [Header("Cuchillo")]
    public Transform weapon;
    public SpriteRenderer weaponSR;
    public float offsetRight = 0.08f;
    public float offsetLeft = 0.07f;
    public int frontOrder = 5;

    public Sprite knifeIdleSprite;
    public Sprite knifeAttackSprite;
    public GameObject slashHitbox;

    [Header("Ataque")]
    public float attackDuration = 0.12f;
    public float slashAngle = 80f;

    // ---------------------------------------------------------
    // POWER-UP (SLASHER MODE)
    // ---------------------------------------------------------
    [Header("Power Up")]
    public bool isBuffed = false;
    public float buffDuration = 5f;
    public float buffMoveMultiplier = 1.4f;
    public float buffAttackMultiplier = 0.7f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip powerUpSFX;

    [Header("Ghost FX")]
    public GameObject ghostPrefab;
    public Transform ghostSpawnPoint;

    [Header("Slasher Slow Motion")]
    public float slasherTimeScale = 0.6f;   // slow global
    public float teenFearSlow = 0.5f;       // extra slow teens
    public float buffRemaining;             // UI
    public float buffRemaining01;           // UI 0..1

    float originalTimeScale = 1f;
    float originalFixedDeltaTime = 0.02f;

    // handles coroutines
    Coroutine buffRoutineHandle;
    Coroutine blinkRoutineHandle;
    Coroutine ghostRoutineHandle;

    void Start()
    {
        baseMoveSpeed = moveSpeed;
        baseAttackDuration = attackDuration;

        // seguridad
        if (slashHitbox != null) slashHitbox.SetActive(false);
        if (weaponSR != null && knifeIdleSprite != null) weaponSR.sprite = knifeIdleSprite;
        if (weapon != null) weapon.localRotation = Quaternion.identity;
    }

    // ---------------------------------------------------------
    void Update()
    {
        HandleMovementInput();
        HandleMouseLook();

        if (!isAttacking)
            HandleKnifeRotation();

        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            StartCoroutine(AttackRoutine());
            animator.SetTrigger("Attack");
        }
    }

    void FixedUpdate()
    {
        if (!isAttacking)
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    // ---------------------------------------------------------
    // MOVIMIENTO
    // ---------------------------------------------------------
    void HandleMovementInput()
    {
        if (!isAttacking)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
            movement = movement.normalized;
        }
        else
        {
            movement = Vector2.zero;
        }
    }

    void HandleMouseLook()
    {
        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePos - rb.position;

        transform.localScale = (lookDir.x < 0f)
            ? new Vector3(-1, 1, 1)
            : new Vector3(1, 1, 1);

        animator.SetBool("LookUp", lookDir.y > 0.3f);
    }

    // ---------------------------------------------------------
    // CUCHILLO
    // ---------------------------------------------------------
    void HandleKnifeRotation()
    {
        Vector2 lookDir = mousePos - rb.position;
        bool lookingUp = lookDir.y > 0.3f;

        Vector3 pos = weapon.localPosition;
        weapon.localScale = new Vector3(transform.localScale.x, 1, 1);

        if (lookDir.x >= 0f)
        {
            pos.x = offsetRight;
            weaponSR.flipX = false;
        }
        else
        {
            pos.x = offsetLeft;
            weaponSR.flipX = true;
        }

        pos.y = 0f;
        weapon.localPosition = pos;

        weapon.localRotation = Quaternion.identity;

        // detrás pero NO 0 (como pediste): cuando mira arriba va a order 2
        weaponSR.sortingOrder = lookingUp ? 2 : frontOrder;

        if (!isAttacking && knifeIdleSprite != null)
            weaponSR.sprite = knifeIdleSprite;
    }

    // ---------------------------------------------------------
    // ATAQUE
    // ---------------------------------------------------------
    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        float usedDuration = isBuffed ? baseAttackDuration * buffAttackMultiplier : baseAttackDuration;
        if (usedDuration < 0.06f) usedDuration = 0.06f;

        float half = usedDuration * 0.5f;

        if (knifeAttackSprite != null) weaponSR.sprite = knifeAttackSprite;
        if (slashHitbox != null) slashHitbox.SetActive(true);

        float elapsed = 0f;
        float startAngle = 0f;
        float endAngle = -slashAngle;

        while (elapsed < half)
        {
            float t = elapsed / half;
            weapon.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(startAngle, endAngle, t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            float t = elapsed / half;
            weapon.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(endAngle, startAngle, t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (slashHitbox != null) slashHitbox.SetActive(false);
        weapon.localRotation = Quaternion.identity;

        if (knifeIdleSprite != null) weaponSR.sprite = knifeIdleSprite;

        isAttacking = false;
    }

    // ---------------------------------------------------------
    // POWER-UP
    // ---------------------------------------------------------
    public void ActivatePowerUp()
    {
        // si estaba atacando, reseteo limpio para evitar estados raros
        if (slashHitbox != null) slashHitbox.SetActive(false);
        if (weapon != null) weapon.localRotation = Quaternion.identity;
        isAttacking = false;

        if (buffRoutineHandle != null)
            StopCoroutine(buffRoutineHandle);

        buffRoutineHandle = StartCoroutine(BuffRoutine());
    }

    IEnumerator BuffRoutine()
    {
        isBuffed = true;

        if (audioSource && powerUpSFX)
            audioSource.PlayOneShot(powerUpSFX);

        // activar slow motion global
        originalTimeScale = Time.timeScale;
        originalFixedDeltaTime = Time.fixedDeltaTime;

        Time.timeScale = slasherTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // slow extra para teens
        TeenMovement.GlobalSpeedMultiplier = teenFearSlow;

        // buff player compensando timeScale
        moveSpeed = baseMoveSpeed * buffMoveMultiplier / Time.timeScale;

        float poweredAttack = baseAttackDuration * buffAttackMultiplier;
        if (poweredAttack < 0.06f) poweredAttack = 0.06f;
        attackDuration = poweredAttack;

        // FX realtime (no afectados por timeScale)
        if (blinkRoutineHandle != null) StopCoroutine(blinkRoutineHandle);
        blinkRoutineHandle = StartCoroutine(BlinkRedRealtime());

        if (ghostRoutineHandle != null) StopCoroutine(ghostRoutineHandle);
        ghostRoutineHandle = StartCoroutine(GhostTrailRealtime());

        // timer REAL
        buffRemaining = buffDuration;

        while (buffRemaining > 0f)
        {
            buffRemaining -= Time.unscaledDeltaTime;
            buffRemaining01 = Mathf.Clamp01(buffRemaining / buffDuration);
            yield return null;
        }

        // restaurar todo
        TeenMovement.GlobalSpeedMultiplier = 1f;

        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDeltaTime;

        moveSpeed = baseMoveSpeed;
        attackDuration = baseAttackDuration;

        bodySR.color = Color.white;
        isBuffed = false;

        buffRemaining = 0f;
        buffRemaining01 = 0f;
    }

    // ---------------------------------------------------------
    // EFECTOS VISUALES (Realtime)
    // ---------------------------------------------------------
    IEnumerator BlinkRedRealtime()
    {
        while (isBuffed)
        {
            bodySR.color = Color.red;
            yield return new WaitForSecondsRealtime(0.05f);

            bodySR.color = Color.white;
            yield return new WaitForSecondsRealtime(0.05f);
        }

        bodySR.color = Color.white;
    }

    IEnumerator GhostTrailRealtime()
    {
        while (isBuffed)
        {
            if (ghostPrefab != null && ghostSpawnPoint != null)
            {
                GameObject g = Instantiate(ghostPrefab, ghostSpawnPoint.position, Quaternion.identity);
                SpriteRenderer gsr = g.GetComponent<SpriteRenderer>();

                if (gsr != null)
                {
                    gsr.sprite = bodySR.sprite;
                    gsr.flipX = bodySR.flipX;
                    gsr.color = new Color(1f, 0f, 0f, 0.4f);
                }

                g.transform.localScale = transform.localScale;

                if (gsr != null) StartCoroutine(FadeGhost(gsr));
                Destroy(g, 0.25f);
            }

            yield return new WaitForSecondsRealtime(0.04f);
        }
    }

    IEnumerator FadeGhost(SpriteRenderer sr)
    {
        float a = sr.color.a;
        while (a > 0)
        {
            a -= Time.unscaledDeltaTime * 8f;
            sr.color = new Color(1f, 0f, 0f, a);
            yield return null;
        }
    }
}

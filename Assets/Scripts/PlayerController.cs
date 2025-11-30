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
    public int backOrder = 0;

    public Sprite knifeIdleSprite;
    public Sprite knifeAttackSprite;
    public GameObject slashHitbox;

    [Header("Ataque")]
    public float attackDuration = 0.12f;
    public float slashAngle = 80f;

    // ---------------------------------------------------------
    // POWER-UP
    // ---------------------------------------------------------
    [Header("Power Up")]
    public bool isBuffed = false;
    public float buffDuration = 5f;
    public float buffMoveMultiplier = 1.4f;
    public float buffAttackMultiplier = 0.7f;
    public AudioSource audioSource;
    public AudioClip powerUpSFX;
    public GameObject ghostPrefab;
    public Transform ghostSpawnPoint;

    [Header("Camera FX")]
    public CameraShake camShake;

    Coroutine buffRoutineHandle;
    Coroutine blinkRoutineHandle;
    Coroutine ghostRoutineHandle;

    void Start()
    {
        baseMoveSpeed = moveSpeed;
        baseAttackDuration = attackDuration;
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
            movement = Vector2.zero;
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
        weaponSR.sortingOrder = lookingUp ? 2 : frontOrder;


        if (!isAttacking)
            weaponSR.sprite = knifeIdleSprite;
    }

    // ---------------------------------------------------------
    // ATAQUE (seguro, no desaparece el arma)
    // ---------------------------------------------------------
    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        float usedDuration = isBuffed
            ? baseAttackDuration * buffAttackMultiplier
            : baseAttackDuration;

        if (usedDuration < 0.06f) usedDuration = 0.06f;

        float half = usedDuration * 0.5f;

        weaponSR.sprite = knifeAttackSprite;
        slashHitbox.SetActive(true);

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

        slashHitbox.SetActive(false);
        weapon.localRotation = Quaternion.identity;
        weaponSR.sprite = knifeIdleSprite;
        isAttacking = false;
    }

    // ---------------------------------------------------------
    // POWER-UP
    // ---------------------------------------------------------
    public void ActivatePowerUp()
    {
        if (buffRoutineHandle != null)
            StopCoroutine(buffRoutineHandle);

        buffRoutineHandle = StartCoroutine(BuffRoutine());
    }

    IEnumerator BuffRoutine()
    {
        isBuffed = true;

        if (audioSource && powerUpSFX)
            audioSource.PlayOneShot(powerUpSFX);

        moveSpeed = baseMoveSpeed * buffMoveMultiplier;

        float poweredAttack = baseAttackDuration * buffAttackMultiplier;
        if (poweredAttack < 0.06f) poweredAttack = 0.06f;
        attackDuration = poweredAttack;

        if (blinkRoutineHandle != null)
            StopCoroutine(blinkRoutineHandle);
        blinkRoutineHandle = StartCoroutine(BlinkRed());

        if (ghostRoutineHandle != null)
            StopCoroutine(ghostRoutineHandle);
        ghostRoutineHandle = StartCoroutine(GhostTrail());

        if (camShake != null)
            camShake.StartContinuousShake(0.05f);

        float t = 0f;
        while (t < buffDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        moveSpeed = baseMoveSpeed;
        attackDuration = baseAttackDuration;
        bodySR.color = Color.white;
        isBuffed = false;

        if (camShake != null)
            camShake.StopContinuousShake();
    }

    // ---------------------------------------------------------
    // EFECTOS VISUALES
    // ---------------------------------------------------------
    IEnumerator BlinkRed()
    {
        while (isBuffed)
        {
            bodySR.color = Color.red;
            yield return new WaitForSeconds(0.05f);
            bodySR.color = Color.white;
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator GhostTrail()
    {
        while (isBuffed)
        {
            if (ghostPrefab != null)
            {
                GameObject g = Instantiate(ghostPrefab, ghostSpawnPoint.position, Quaternion.identity);
                SpriteRenderer gsr = g.GetComponent<SpriteRenderer>();
                gsr.sprite = bodySR.sprite;
                gsr.flipX = bodySR.flipX;
                gsr.color = new Color(1f, 0f, 0f, 0.4f);
                g.transform.localScale = transform.localScale;

                StartCoroutine(FadeGhost(gsr));
                Destroy(g, 0.25f);
            }

            yield return new WaitForSeconds(0.04f);
        }
    }

    IEnumerator FadeGhost(SpriteRenderer sr)
    {
        float a = sr.color.a;
        while (a > 0)
        {
            a -= Time.deltaTime * 8f;
            sr.color = new Color(1f, 0f, 0f, a);
            yield return null;
        }
    }
}

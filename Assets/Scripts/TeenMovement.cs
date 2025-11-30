using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TeenMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float minChangeDirTime = 1f;
    public float maxChangeDirTime = 3f;

    [Header("Knockback")]
    public float knockbackDrag = 5f; 

    Rigidbody2D rb;
    Vector2 moveDir;
    float changeDirTimer;
    SpriteRenderer sr;

    bool isKnockback = false;
    Vector2 knockbackVelocity;
    bool canMove = true;   

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

    }

    void OnEnable()
    {
        PickRandomDirection();
        ResetChangeDirTimer();
    }

    void Update()
    {
        if (!isKnockback && canMove)
        {
            changeDirTimer -= Time.deltaTime;
            if (changeDirTimer <= 0f)
            {
                PickRandomDirection();
                ResetChangeDirTimer();
            }
        }
        else if (isKnockback)
        {
           
            knockbackVelocity = Vector2.MoveTowards(
                knockbackVelocity,
                Vector2.zero,
                knockbackDrag * Time.deltaTime
            );

            if (knockbackVelocity.magnitude < 0.05f)
            {
                isKnockback = false;
            }
        }
        UpdateLookDirection();

    }

    void FixedUpdate()
    {
        if (isKnockback)
        {
            rb.linearVelocity = knockbackVelocity;
        }
        else if (canMove)
        {
            rb.linearVelocity = moveDir * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void PickRandomDirection()
    {
        moveDir = Random.insideUnitCircle.normalized;
        if (moveDir == Vector2.zero)
            moveDir = Vector2.right;
    }

    void ResetChangeDirTimer()
    {
        changeDirTimer = Random.Range(minChangeDirTime, maxChangeDirTime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Wall")) return;

        Vector2 normal = collision.contacts[0].normal;

        if (isKnockback)
        {
            knockbackVelocity = Vector2.Reflect(knockbackVelocity, normal);
        }
        else if (canMove)
        {
            moveDir = Vector2.Reflect(moveDir, normal).normalized;
            ResetChangeDirTimer();
        }
    }

    public void OnDie()
    {
        canMove = false;  
    }

    public void AddKnockback(Vector2 dir, float force)
    {
        isKnockback = true;
        knockbackVelocity = dir.normalized * force;
    }
    void UpdateLookDirection()
    {
        Vector2 dir = isKnockback ? knockbackVelocity : rb.linearVelocity;

        if (dir.x < -0.05f)
            sr.flipX = true;
        else if (dir.x > 0.05f)
            sr.flipX = false;
    }

}

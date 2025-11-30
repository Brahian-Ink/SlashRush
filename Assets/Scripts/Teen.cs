using UnityEngine;
using System.Collections;

public class Teen : MonoBehaviour
{
    public int health = 1;

    public Sprite aliveSprite;
    public Sprite deadSprite;

    public GameObject bloodParticles;
    public GameObject[] bloodStains;

    [Header("Audio")]
    public AudioSource audioSource;   
    public AudioClip hitClip;         

    private SpriteRenderer sr;
    private bool isDead = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (sr != null && aliveSprite != null)
            sr.sprite = aliveSprite;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        // sonido de golpe
        if (audioSource != null && hitClip != null)
            audioSource.PlayOneShot(hitClip);

        health -= dmg;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // cambiar sprite
        if (sr != null && deadSprite != null)
            sr.sprite = deadSprite;

        Animator anim = GetComponent<Animator>();
        if (anim != null)
            anim.enabled = false;

        // sangre
        if (bloodParticles)
            Instantiate(bloodParticles, transform.position, Quaternion.identity);

        // mancha
        if (bloodStains != null && bloodStains.Length > 0)
        {
            int r = Random.Range(0, bloodStains.Length);
            Instantiate(bloodStains[r], transform.position, Quaternion.identity);
           

            // SHAKE DE CÁMARA AL MATAR UNO
            if (GameManager.Instance != null && GameManager.Instance.camShake != null)
            {
                GameManager.Instance.camShake.Shake(0.08f, 0.06f);
            }

            if (GameManager.Instance != null)
                GameManager.Instance.RegisterKill();

        }

        TeenMovement mv = GetComponent<TeenMovement>();
        if (mv != null)
            mv.OnDie();

        StartCoroutine(FadeAndDisappear());
    }

    IEnumerator FadeAndDisappear()
    {
        yield return new WaitForSeconds(5f);

        float fadeTime = 1f;
        float elapsed = 0f;
        Color c = sr.color;
        bool visible = true;
        float blinkInterval = 0.1f;
        float blinkTimer = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            blinkTimer += Time.deltaTime;

            // parpadeo
            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;
                visible = !visible;
                sr.enabled = visible;
            }

            // bajar opacidad
            float t = elapsed / fadeTime;
            float alpha = Mathf.Lerp(1f, 0f, t);
            c.a = alpha;
            sr.color = c;

            yield return null;
        }

        Destroy(gameObject);
    }


}

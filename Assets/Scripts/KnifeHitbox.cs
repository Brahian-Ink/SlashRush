using UnityEngine;

public class KnifeHitbox : MonoBehaviour
{
    public int damage = 1;
    public float knockbackForce = 6f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // knockback SOLO desde el arma
        TeenMovement mv = other.GetComponent<TeenMovement>();
        if (mv != null)
        {
            Vector2 dir = (other.transform.position - transform.position);
            mv.AddKnockback(dir, knockbackForce);
        }

        Teen t = other.GetComponent<Teen>();
        if (t != null)
        {
            t.TakeDamage(damage);
        }
    }
}

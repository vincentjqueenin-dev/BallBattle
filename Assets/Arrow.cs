using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [HideInInspector] public Ball owner;
    public float damage = 1f;
    public bool isCrit = false; // Declared for BowWeapon critical strike hits!

    public float slowAmount = 0.5f;   // 50% velocity reduction
    public float slowDuration = 0.6f; // Holds slow for 0.6s
    public float lifeTime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Arrow vs Arrow clash
        if (collision.CompareTag("Arrow"))
        {
            Arrow otherArrow = collision.GetComponent<Arrow>();
            if (otherArrow != null && otherArrow.owner != owner)
            {
                Destroy(gameObject);
            }
            return;
        }

        // 2. Clash with enemy Sword or Spear
        if (collision.CompareTag("Sword") || collision.CompareTag("Spear"))
        {
            Destroy(gameObject);
            return;
        }

        // 3. Hit Enemy Ball
        Ball target = collision.GetComponent<Ball>();

        if (target != null && target != owner)
        {
            // Deal damage and pass critical status for visual FX & text pop-ups
            target.TakeDamage(damage, isCrit);

            // Only apply slow if target survived
            if (target.gameObject.activeInHierarchy && !target.isDead)
            {
                Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
                if (targetRb != null)
                {
                    target.StartCoroutine(ApplySlowRoutine(targetRb));
                }
            }

            // Register hit back to BowBall
            BowBall bowOwner = owner as BowBall;
            if (bowOwner != null)
            {
                bowOwner.RegisterArrowHit();
            }

            Destroy(gameObject);
            return;
        }

        // 4. Destroy on arena boundaries
        if (owner != null && collision.gameObject != owner.gameObject)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator ApplySlowRoutine(Rigidbody2D targetRb)
    {
        if (targetRb == null) yield break;

        targetRb.linearVelocity *= (1f - slowAmount);
        yield return new WaitForSeconds(slowDuration);
    }
}
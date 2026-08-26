using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float damage = 4f;
    public float lifeTime = 5f; // Despawn after 5 seconds if no hit
    [HideInInspector] public Combat owner;

    private void Start()
    {
        // Auto-destroy after timer expires
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Combat target = collision.GetComponent<Combat>();

        // 1. Hit Enemy Fighter
        if (target != null && target != owner)
        {
            target.TakeDamage(damage);

            if (owner != null)
            {
                owner.RegisterArrowHit(); // +1 arrow for next volley
            }

            Destroy(gameObject);
            return;
        }

        // 2. Hit Wall or Environment (Despawn on impact)
        // Ensure you don't destroy when touching the owner or other arrows
        if (collision.gameObject != owner.gameObject && !collision.CompareTag("Arrow"))
        {
            Destroy(gameObject);
        }
    }
}
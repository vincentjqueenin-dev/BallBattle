using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float damage = 1f;
    public float lifeTime = 5f;
    [HideInInspector] public Combat owner;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Safe tag check for Sword & Arrow clashing
        if (collision.gameObject.CompareTag("Sword") || collision.gameObject.CompareTag("Arrow"))
        {
            Destroy(gameObject);
            return;
        }

        Combat target = collision.GetComponent<Combat>();

        if (target != null && target != owner)
        {
            target.TakeDamage(damage);

            if (owner != null)
            {
                owner.RegisterArrowHit(); // Adds +1 arrow for next volley
            }

            Destroy(gameObject);
            return;
        }

        if (owner != null && collision.gameObject != owner.gameObject)
        {
            Destroy(gameObject);
        }
    }
}
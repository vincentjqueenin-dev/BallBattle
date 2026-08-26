using UnityEngine;

public class SwordWeapon : MonoBehaviour
{
    public Combat parentCombat;

    private float nextAttackTime = 0f;
    private const float ATTACK_COOLDOWN = 0.5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Clash with another Sword
        if (collision.CompareTag("Sword"))
        {
            if (parentCombat != null) parentCombat.ReverseRotation();
            return;
        }

        // 2. Clash with an Arrow
        if (collision.CompareTag("Arrow"))
        {
            if (parentCombat != null) parentCombat.ReverseRotation();
            return;
        }

        // 3. Hit Enemy Ball
        Combat target = collision.GetComponent<Combat>();

        if (target != null && target != parentCombat)
        {
            // Block damage if sword is currently on cooldown
            if (Time.time < nextAttackTime) return;

            if (parentCombat != null)
            {
                // Trigger rotation reversal on ball hit
                parentCombat.ReverseRotation();

                // Apply damage & lock attacks for 0.5s
                float damage = parentCombat.CalculateSwordDamage();
                target.TakeDamage(damage);
                nextAttackTime = Time.time + ATTACK_COOLDOWN;
            }
        }
    }
}
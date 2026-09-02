using UnityEngine;

public class UnarmedBall : Ball
{
    [Header("Unarmed Dynamic Cap Settings")]
    public float currentCap = 5f;
    public float capGrowthPerHit = 1f;

    protected override void DecideNextState()
    {
        float roll = Random.value;
        if (roll < 0.5f) currentState = CombatState.Aggressor;
        else if (roll < 0.8f) currentState = CombatState.Defensive;
        else currentState = CombatState.Evasive;
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);

        Ball target = collision.gameObject.GetComponent<Ball>();
        if (target != null && data != null && rb != null)
        {
            float currentSpeed = rb.linearVelocity.magnitude;

            // Scaled strictly from speed (0.8 damage per speed unit, no random base spike)
            float rawDamage = currentSpeed * 0.8f;

            // Clamped by current scaling cap
            float finalDamage = Mathf.Min(rawDamage, currentCap);
            target.TakeDamage(finalDamage);

            // Grow cap for future hits
            currentCap += capGrowthPerHit;
        }
    }

    protected override string GetStatusText()
    {
        float currentSpeed = (rb != null) ? rb.linearVelocity.magnitude : 0f;

        string status = $"<b>{data.ballName}</b>\n";
        status += $"HP: {Mathf.Max(0, currentHealth):F0}/{data.maxHealth:F0}\n";
        status += $"State: [{currentState}]\n";
        status += $"Spd: {currentSpeed:F1} | Cap: {currentCap:F0}";
        return status;
    }
}
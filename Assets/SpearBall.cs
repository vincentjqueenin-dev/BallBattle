using UnityEngine;

public class SpearBall : Ball
{
    [Header("Spear Scaling Settings")]
    public float currentDamage = 3f;           // Starting base damage
    public float currentInterval = 8.0f;       // Starting thrust interval
    public float baseThrustDamage = 8.0f;      // Fixed damage for the big thrust attack

    private const float MIN_INTERVAL = 0.5f;

    protected override void DecideNextState()
    {
        // Spear is inherently defensive: 70% Defensive (strafing), 30% Evasive
        currentState = (Random.value < 0.7f) ? CombatState.Defensive : CombatState.Evasive;
    }

    // Called on standard non-thrust hits
    public float RegisterStandardHit()
    {
        float hitDamage = currentDamage;

        // Scale current damage by +1.1x
        currentDamage *= 1.1f;

        // Reduce thrust interval by 0.1s
        currentInterval = Mathf.Max(MIN_INTERVAL, currentInterval - 0.1f);

        return hitDamage;
    }

    // Called when a heavy thrust lands on the enemy
    public float RegisterThrustHit()
    {
        // Thrust damage doubles the base thrust damage value
        float hitDamage = baseThrustDamage * 2.0f;

        // Halve the thrust interval
        currentInterval = Mathf.Max(MIN_INTERVAL, currentInterval / 2.0f);

        return hitDamage;
    }

    protected override string GetStatusText()
    {
        string status = $"<b>{data.ballName}</b>\n";
        status += $"HP: {Mathf.Max(0, currentHealth):F0}/{data.maxHealth:F0}\n";
        status += $"State: [{currentState}]\n";
        status += $"Standard Dmg: {currentDamage:F1}\n";
        status += $"Thrust Cooldown: {currentInterval:F1}s";
        return status;
    }
}
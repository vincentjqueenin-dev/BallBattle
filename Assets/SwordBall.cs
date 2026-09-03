using UnityEngine;

public class SwordBall : Ball
{
    [Header("Sword Compound Scaling Settings")]
    public float currentDamage = 5.0f;     // Retained base damage at 5.0
    public float currentMultiplier = 1.5f; // Lowered initial multiplier (1.5x)
    private const float MULTIPLIER_DECAY = 0.2f; // Increased decay rate (drops by 0.2x per hit)
    private const float MIN_MULTIPLIER = 1.1f;
    private const float MAX_DAMAGE_CAP = 60f;    // Prevents extreme exponential runaways

    protected override void DecideNextState()
    {
        currentState = CombatState.Aggressor;
    }

    public float CalculateSwordDamage()
    {
        // 1. Calculate hit damage using current multiplier
        float hitDamage = currentDamage * currentMultiplier;

        // 2. Set new damage baseline for the next hit
        currentDamage = Mathf.Min(hitDamage, MAX_DAMAGE_CAP);

        // 3. Multiplier decays by 0.2x per hit down to 1.1x floor
        currentMultiplier = Mathf.Max(MIN_MULTIPLIER, currentMultiplier - MULTIPLIER_DECAY);

        return currentDamage;
    }

    protected override string GetStatusText()
    {
        float nextDamage = Mathf.Min(currentDamage * currentMultiplier, MAX_DAMAGE_CAP);

        string status = $"<b>{data.ballName}</b>\n";
        status += $"HP: {Mathf.Max(0, currentHealth):F0}/{data.maxHealth:F0}\n";
        status += $"State: [{currentState}]\n";
        status += $"Next Dmg: {nextDamage:F1} ({currentMultiplier:F1}x)";
        return status;
    }
}
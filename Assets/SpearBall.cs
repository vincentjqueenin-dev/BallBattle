using UnityEngine;

public class SpearBall : Ball
{
    [Header("Spear Scaling Settings")]
    public float currentDamage = 3.0f;
    public float currentInterval = 8.0f;

    // Base thrust damage lowered from 8.0 to 6.0 to compensate for 25% crit chance
    public float baseThrustDamage = 6.0f;

    private const float MIN_INTERVAL = 0.5f;

    protected override void DecideNextState()
    {
        // Spear focuses heavily on defensive positioning between dash-thrusts
        currentState = CombatState.Defensive;
    }

    public float RegisterStandardHit()
    {
        currentDamage += 1.1f;
        currentInterval = Mathf.Max(MIN_INTERVAL, currentInterval - 0.1f);
        UpdateUI();
        return currentDamage;
    }

    public float RegisterThrustHit()
    {
        // Standard Thrust lands 2x base thrust damage
        float thrustDmg = baseThrustDamage * 2.0f;
        currentInterval = Mathf.Max(MIN_INTERVAL, currentInterval - 0.5f);
        UpdateUI();
        return thrustDmg;
    }

    protected override string GetStatusText()
    {
        string status = $"<b>{data.ballName}</b>\n";
        status += $"HP: {Mathf.Max(0, currentHealth):F0}/{data.maxHealth:F0}\n";
        status += $"State: [{currentState}]\n";
        status += $"Std Dmg: {currentDamage:F1}\n";
        status += $"Thrust Cooldown: {currentInterval:F1}s";
        return status;
    }
}
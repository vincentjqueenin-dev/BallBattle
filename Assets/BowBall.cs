using UnityEngine;

public class BowBall : Ball
{
    [Header("Bow Passive Settings")]
    public int currentArrowCount = 3;
    private int hitsInCurrentVolley = 0;

    protected override void DecideNextState()
    {
        currentState = (Random.value < 0.7f) ? CombatState.Evasive : CombatState.Defensive;
    }

    // Dynamic Milestone Damage per Arrow
    public float GetCalculatedArrowDamage()
    {
        if (currentArrowCount >= 10) return 3.0f; // Tier 2 Milestone (10+ arrows)
        if (currentArrowCount >= 5) return 2.0f; // Tier 1 Milestone (5+ arrows)
        return 1.0f;                              // Base Damage (1-4 arrows)
    }

    public void RegisterArrowHit()
    {
        hitsInCurrentVolley++;
    }

    public void ApplyVolleyHitBonus()
    {
        if (hitsInCurrentVolley > 0)
        {
            currentArrowCount += hitsInCurrentVolley;
            hitsInCurrentVolley = 0;
        }

        UpdateUI();
    }

    protected override string GetStatusText()
    {
        float currentDmg = GetCalculatedArrowDamage();

        string status = $"<b>{data.ballName}</b>\n";
        status += $"HP: {Mathf.Max(0, currentHealth):F0}/{data.maxHealth:F0}\n";
        status += $"State: [{currentState}]\n";
        status += $"Arrows/Burst: {currentArrowCount}\n";
        status += $"Arrow Dmg: {currentDmg:F0}";
        return status;
    }
}
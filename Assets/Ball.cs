using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Ball : MonoBehaviour
{
    public enum CombatState { Aggressor, Evasive, Defensive }

    [Header("Combat Tactics Settings")]
    public CombatState currentState = CombatState.Aggressor;
    public float actionInterval = 2.0f; // Seconds between tactical moves
    public float actionForceMultiplier = 5.0f;

    [Header("Data & Health")]
    public BallData data;
    public float currentHealth;
    [HideInInspector] public bool isDead = false;

    [Header("UI & HUD References")]
    public TMP_Text healthText;
    public TMP_Text debugHudText;

    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    private static bool gameEnding = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // FIX: Look in children so parent script can color child sprite graphics!
        sr = GetComponentInChildren<SpriteRenderer>();
        gameEnding = false;
    }

    protected virtual void Start()
    {
        if (data != null && currentHealth <= 0) currentHealth = data.maxHealth;
        UpdateUI();

        // Start tactical impulse routine
        StartCoroutine(TacticalIntervalRoutine());
    }

    protected virtual void Update()
    {
        UpdateUI();
    }

    protected virtual void FixedUpdate()
    {
        if (rb != null && data != null && data.maxSpeed > 0)
        {
            if (rb.linearVelocity.magnitude > data.maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * data.maxSpeed;
            }
        }
    }

    private IEnumerator TacticalIntervalRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(actionInterval);

            // Subclasses can update currentState here before execution
            DecideNextState();
            ExecuteCombatState();
        }
    }

    // Subclasses override this to change states dynamically
    protected virtual void DecideNextState()
    {
        // Default base behavior: keeps current state
    }

    private void ExecuteCombatState()
    {
        Ball enemy = FindNearestEnemy();
        if (enemy == null || rb == null) return;

        Vector2 dirToEnemy = (enemy.transform.position - transform.position).normalized;

        switch (currentState)
        {
            case CombatState.Aggressor:
                // Launch 1.5f towards enemy
                rb.AddForce(dirToEnemy * (1.5f * actionForceMultiplier), ForceMode2D.Impulse);
                break;

            case CombatState.Evasive:
                // Back off 2.0f away from enemy
                rb.AddForce(-dirToEnemy * (2.0f * actionForceMultiplier), ForceMode2D.Impulse);
                break;

            case CombatState.Defensive:
                // Strafe 1.0f to a random side (perpendicular to target)
                Vector2 perpendicularDir = new Vector2(-dirToEnemy.y, dirToEnemy.x);
                if (Random.value > 0.5f) perpendicularDir *= -1f; // Pick left or right
                rb.AddForce(perpendicularDir * (1.0f * actionForceMultiplier), ForceMode2D.Impulse);
                break;
        }
    }

    private Ball FindNearestEnemy()
    {
        Ball[] allBalls = Object.FindObjectsByType<Ball>(FindObjectsSortMode.None);
        Ball nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var ball in allBalls)
        {
            if (ball != this && !ball.isDead)
            {
                float dist = Vector2.Distance(transform.position, ball.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = ball;
                }
            }
        }
        return nearest;
    }

    public virtual void ApplyData(BallData newData)
    {
        if (newData == null) return;

        data = newData;
        currentHealth = data.maxHealth;

        // Apply color to ALL sprite renderers attached to the ball body,
        // or target the main body sprite specifically
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sprite in renderers)
        {
            // Ignore weapon child objects if they shouldn't be recolored
            if (!sprite.CompareTag("Sword") && !sprite.CompareTag("Spear") && !sprite.CompareTag("Arrow"))
            {
                sprite.color = data.ballColor;
            }
        }

        if (rb != null)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            rb.linearVelocity = randomDir * data.launchSpeed;
        }

        UpdateUI();
    }


    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        Ball target = collision.gameObject.GetComponent<Ball>();

        if (target != null && data != null)
        {
            Rigidbody2D targetRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                Vector2 pushDir = (collision.transform.position - transform.position).normalized;
                targetRb.AddForce(pushDir * data.knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        UpdateUI();

        if (sr != null) StartCoroutine(FlashRedRoutine());
        if (currentHealth <= 0) DieAndCheckWinner();
    }

    private IEnumerator FlashRedRoutine()
    {
        Color originalColor = data != null ? data.ballColor : Color.white;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.08f);
        if (sr != null) sr.color = originalColor;
    }

    public void UpdateUI()
    {
        if (healthText != null)
        {
            healthText.text = Mathf.Max(0, currentHealth).ToString("F0");
        }

        if (debugHudText != null && data != null)
        {
            debugHudText.text = GetStatusText();
        }
    }

    protected virtual string GetStatusText()
    {
        string status = $"<b>{data.ballName}</b>\n";
        status += $"HP: {Mathf.Max(0, currentHealth):F0}/{data.maxHealth:F0}\n";
        status += $"State: {currentState}";
        return status;
    }

    private void DieAndCheckWinner()
    {
        isDead = true;

        if (!gameEnding)
        {
            gameEnding = true;

            Ball[] fighters = Object.FindObjectsByType<Ball>(FindObjectsSortMode.None);
            Ball winner = null;

            foreach (var fighter in fighters)
            {
                if (fighter != this && !fighter.isDead)
                {
                    winner = fighter;
                    break;
                }
            }

            if (winner != null && CameraZoom.Instance != null)
            {
                CameraZoom.Instance.ZoomToTarget(winner.transform);
                CameraZoom.Instance.StartCoroutine(ReturnToMenuAfterDelay(3.0f));
            }
            else
            {
                SceneManager.LoadScene("MenuScene");
            }
        }

        gameObject.SetActive(false);
    }

    private static IEnumerator ReturnToMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("MenuScene");
    }
}
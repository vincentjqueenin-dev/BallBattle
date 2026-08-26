using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Combat : MonoBehaviour
{
    [Header("Data & Health")]
    public BallData data;
    public float currentHealth;
    [HideInInspector] public bool isDead = false;

    [Header("UI & HUD References")]
    public TMP_Text healthText;   // Text on the ball
    public TMP_Text debugHudText; // On-screen HUD text detailing stats/passive

    [Header("Weapon Type Flags")]
    public bool isUnarmed = false;
    public bool isSword = false;
    public bool isBow = false;

    [Header("Visual Weapon References")]
    public Transform weaponPivot;
    public GameObject swordVisual;
    public GameObject bowVisual;
    public Transform firePoint;
    public float rotationSpeed = 200f;
    private float rotationDirection = 1f; // Controls clockwise / counter-clockwise

    [Header("Sword Passive Settings")]
    private float swordBonusPercent = 0.50f; // Starts at +50%
    private const float MIN_BONUS = 0.05f;   // Lower bound +5%
    private const float DECAY = 0.05f;       // Drops by 5% per hit

    [Header("Bow Passive Settings")]
    public GameObject arrowPrefab;
    public int currentArrowCount = 3;
    private int hitsThisVolley = 0;
    private const float BURST_SHOT_DELAY = 0.1f;
    private const float VOLLEY_COOLDOWN = 0.8f;
    private bool isFiringBow = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private static bool gameEnding = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        gameEnding = false;
    }

    private void Start()
    {
        if (data != null && currentHealth <= 0) currentHealth = data.maxHealth;

        if (isSword)
        {
            rotationSpeed = Random.Range(150f, 280f);
        }

        UpdateUI();
    }

    private void Update()
    {
        // Weapon Rotation incorporating current direction multiplier
        if ((isSword || isBow) && weaponPivot != null)
        {
            weaponPivot.Rotate(0, 0, rotationSpeed * rotationDirection * Time.deltaTime);
        }

        // Bow Rapid Burst Loop
        if (isBow && !isFiringBow && !isDead)
        {
            StartCoroutine(FireBowBurstRoutine());
        }

        UpdateUI();
    }

    private void FixedUpdate()
    {
        // Apply Speed Limit / Velocity Cap
        if (rb != null && data != null && data.maxSpeed > 0)
        {
            if (rb.linearVelocity.magnitude > data.maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * data.maxSpeed;
            }
        }
    }

    // Called by SwordWeapon upon hitting another weapon/arrow
    public void ReverseRotation()
    {
        rotationDirection *= -1f;
    }

    public void ApplyData(BallData newData)
    {
        if (newData == null) return;

        data = newData;
        currentHealth = data.maxHealth;

        string bName = data.ballName.Trim().ToLower();
        isSword = bName.Contains("sword");
        isBow = bName.Contains("bow");
        isUnarmed = !isSword && !isBow;

        if (swordVisual != null) swordVisual.SetActive(isSword);
        if (bowVisual != null) bowVisual.SetActive(isBow);

        if (sr != null) sr.color = data.ballColor;

        if (rb != null)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            rb.linearVelocity = randomDir * data.launchSpeed;
        }

        if (isSword)
        {
            rotationSpeed = Random.Range(150f, 280f);
        }

        UpdateUI();
    }

    public float CalculateSwordDamage()
    {
        // Falls back to 1f if minDamage isn't set on BallData
        float minDmg = (data != null) ? data.minDamage : 1f;
        float maxDmg = (data != null) ? data.maxDamage : 25f;

        float baseDamage = Random.Range(minDmg, maxDmg);
        float finalDamage = baseDamage * (1f + swordBonusPercent);

        swordBonusPercent = Mathf.Max(MIN_BONUS, swordBonusPercent - DECAY);
        return finalDamage;
    }

    private IEnumerator FireBowBurstRoutine()
    {
        isFiringBow = true;
        int arrowsToFire = currentArrowCount;

        for (int i = 0; i < arrowsToFire; i++)
        {
            if (isDead) yield break;

            if (arrowPrefab != null)
            {
                Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position;
                Vector2 fireDirection = (firePoint != null) ? (firePoint.position - transform.position).normalized : transform.up;
                float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;

                GameObject arrowObj = Instantiate(arrowPrefab, spawnPos, Quaternion.Euler(0, 0, angle));
                Arrow arrow = arrowObj.GetComponent<Arrow>();
                if (arrow != null) arrow.owner = this;

                Rigidbody2D arrowRb = arrowObj.GetComponent<Rigidbody2D>();
                if (arrowRb != null)
                {
                    arrowRb.bodyType = RigidbodyType2D.Dynamic;
                    arrowRb.gravityScale = 0f;
                    arrowRb.linearVelocity = fireDirection * 16f;
                }
            }

            yield return new WaitForSeconds(BURST_SHOT_DELAY);
        }

        currentArrowCount += hitsThisVolley;
        hitsThisVolley = 0;

        yield return new WaitForSeconds(VOLLEY_COOLDOWN);
        isFiringBow = false;
    }

    public void RegisterArrowHit()
    {
        hitsThisVolley++;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Combat target = collision.gameObject.GetComponent<Combat>();

        if (target != null && data != null)
        {
            if (isUnarmed && rb != null)
            {
                float baseDamage = Random.Range(data.minDamage, data.maxDamage);
                float currentSpeed = rb.linearVelocity.magnitude;
                float finalDamage = baseDamage + (currentSpeed * 0.8f);
                target.TakeDamage(finalDamage);
            }

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
            string status = $"<b>{data.ballName}</b>\n";
            status += $"HP: {Mathf.Max(0, currentHealth):F0}/{data.maxHealth:F0}\n";

            if (isSword)
            {
                status += $"Base Dmg: {data.minDamage}-{data.maxDamage}\n";
                status += $"Passive: Sword (+{swordBonusPercent * 100f:F0}% bonus dmg)\n";
                status += $"Speed: {rotationSpeed:F0}°/s";
            }
            else if (isBow)
            {
                status += $"Arrow Dmg: 1\n";
                status += $"Passive: Bow ({currentArrowCount} arrows/burst)";
            }
            else if (isUnarmed)
            {
                float spd = rb != null ? rb.linearVelocity.magnitude : 0f;
                status += $"Base Dmg: {data.minDamage}-{data.maxDamage}\n";
                status += $"Passive: Unarmed (Spd: {spd:F1} -> Dmg: {data.minDamage + (spd * 0.8f):F1})";
            }

            debugHudText.text = status;
        }
    }

    private void DieAndCheckWinner()
    {
        isDead = true;

        if (!gameEnding)
        {
            gameEnding = true;

            // Find surviving fighter
            Combat[] fighters = Object.FindObjectsByType<Combat>(FindObjectsSortMode.None);
            Combat winner = null;

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
            }

            // Create a persistent runner object to execute the scene transition timer
            GameObject transitionRunner = new GameObject("TransitionRunner");
            transitionRunner.AddComponent<SceneTransitionRunner>();
        }

        gameObject.SetActive(false);
    }
}

// Lightweight helper attached dynamically on death to handle menu scene load
public class SceneTransitionRunner : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene("MenuScene");
    }
}
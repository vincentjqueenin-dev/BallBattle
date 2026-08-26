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

    [Header("UI Reference")]
    public TMP_Text healthText;

    [Header("Weapon Type Flags")]
    public bool isUnarmed = false;
    public bool isSword = false;
    public bool isBow = false;

    [Header("Visual Weapon References")]
    public Transform weaponPivot;
    public GameObject swordVisual;
    public GameObject bowVisual;
    public Transform firePoint;
    public float swordRotationSpeed = 360f;

    [Header("Sword Passive Settings")]
    private float swordMultiplier = 2.0f;
    private const float MIN_SWORD_MULT = 1.1f;
    private const float MULT_DECAY = 0.05f;

    [Header("Bow Passive Settings")]
    public GameObject arrowPrefab;
    public int currentArrowCount = 3;
    private int hitsThisVolley = 0;
    private const float VOLLEY_DURATION = 3.0f;
    private bool isFiringBow = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Combat enemyTarget;
    private static bool gameEnding = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        gameEnding = false;
    }

    private void Start()
    {
        if (data != null && currentHealth <= 0)
        {
            currentHealth = data.maxHealth;
        }
        UpdateUI();
        FindEnemyTarget();
    }

    private void Update()
    {
        // 1. Rotate sword continuously
        if (isSword && weaponPivot != null)
        {
            weaponPivot.Rotate(0, 0, swordRotationSpeed * Time.deltaTime);
        }

        // 2. Aim bow towards opponent
        if (isBow && weaponPivot != null && enemyTarget != null && !enemyTarget.isDead)
        {
            Vector2 dir = (enemyTarget.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            weaponPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void ApplyData(BallData newData)
    {
        if (newData == null) return;

        data = newData;
        currentHealth = data.maxHealth;

        // Inside ApplyData() in Combat.cs
        string bName = data.ballName.Trim().ToLower();
        isSword = bName.Contains("sword");
        isBow = bName.Contains("bow");
        isUnarmed = !isSword && !isBow;

        // Automatically toggle visible graphics
        if (swordVisual != null) swordVisual.SetActive(isSword);
        if (bowVisual != null) bowVisual.SetActive(isBow);

        if (sr != null) sr.color = data.ballColor;

        if (rb != null)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            rb.linearVelocity = randomDir * data.launchSpeed;
        }

        UpdateUI();
    }

    private void FindEnemyTarget()
    {
        Combat[] fighters = Object.FindObjectsByType<Combat>(FindObjectsInactive.Exclude);
        foreach (var fighter in fighters)
        {
            if (fighter != this)
            {
                enemyTarget = fighter;
                break;
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        UpdateUI();

        if (sr != null) StartCoroutine(FlashRedRoutine());

        if (currentHealth <= 0)
        {
            DieAndCheckWinner();
        }
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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Combat target = collision.gameObject.GetComponent<Combat>();

        if (target != null && data != null)
        {
            float baseDamage = Random.Range(data.minDamage, data.maxDamage);
            float finalDamage = baseDamage;

            if (isUnarmed && rb != null)
            {
                float currentSpeed = rb.linearVelocity.magnitude;
                finalDamage = baseDamage + (currentSpeed * 0.8f);
                finalDamage = Mathf.Min(finalDamage, 35f);
            }
            else if (isSword)
            {
                finalDamage = baseDamage * swordMultiplier;
                swordMultiplier = Mathf.Max(MIN_SWORD_MULT, swordMultiplier - MULT_DECAY);
            }

            target.TakeDamage(finalDamage);

            if (isBow && !isFiringBow)
            {
                StartCoroutine(FireBowVolleyRoutine(target.transform));
            }

            Rigidbody2D targetRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                Vector2 pushDir = (collision.transform.position - transform.position).normalized;
                targetRb.AddForce(pushDir * data.knockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    private IEnumerator FireBowVolleyRoutine(Transform targetTransform)
    {
        isFiringBow = true;
        float fireDelay = VOLLEY_DURATION / currentArrowCount;

        for (int i = 0; i < currentArrowCount; i++)
        {
            if (targetTransform != null && arrowPrefab != null)
            {
                Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position;
                Vector2 dir = (targetTransform.position - spawnPos).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                // Instantiate arrow with proper rotation towards target
                GameObject arrowObj = Instantiate(arrowPrefab, spawnPos, Quaternion.Euler(0, 0, angle));

                Arrow arrow = arrowObj.GetComponent<Arrow>();
                if (arrow != null) arrow.owner = this;

                // Apply force directly via Rigidbody2D
                Rigidbody2D arrowRb = arrowObj.GetComponent<Rigidbody2D>();
                if (arrowRb != null)
                {
                    arrowRb.bodyType = RigidbodyType2D.Dynamic;
                    arrowRb.gravityScale = 0f; // Keep straight flight trajectory
                    arrowRb.linearVelocity = dir * 18f; // Fast projectile speed
                }
            }

            yield return new WaitForSeconds(fireDelay);
        }

        currentArrowCount += hitsThisVolley;
        hitsThisVolley = 0;
        isFiringBow = false;
    }

    public void RegisterArrowHit()
    {
        hitsThisVolley++;
    }

    private void DieAndCheckWinner()
    {
        isDead = true;

        if (gameEnding) return;

        Combat[] fighters = Object.FindObjectsByType<Combat>(FindObjectsInactive.Exclude);
        Combat winner = null;

        foreach (var fighter in fighters)
        {
            if (fighter != this && !fighter.isDead)
            {
                winner = fighter;
                break;
            }
        }

        if (winner != null)
        {
            gameEnding = true;

            if (CameraZoom.Instance != null)
            {
                CameraZoom.Instance.ZoomToTarget(winner.transform);
            }

            GameObject transitionRunner = new GameObject("SceneTransitionRunner");
            transitionRunner.AddComponent<SceneTransitionHelper>().StartReturnSequence();
        }

        gameObject.SetActive(false);
    }
}

// Top-level class outside of Combat
public class SceneTransitionHelper : MonoBehaviour
{
    public void StartReturnSequence()
    {
        StartCoroutine(ReturnToMenu());
    }

    private IEnumerator ReturnToMenu()
    {
        Time.timeScale = 1f;
        yield return new WaitForSecondsRealtime(2.5f);
        SceneManager.LoadScene("MenuScene");
    }
}
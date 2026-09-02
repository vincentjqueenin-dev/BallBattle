using System.Collections;
using UnityEngine;

public class SpearWeapon : MonoBehaviour
{
    public SpearBall parentBall;
    public Transform weaponPivot;

    [Header("Spin & Dash Settings")]
    public float baseRotationSpeed = 100f;
    public float dashSpeed = 16f;        // Uniform linear dash speed
    public float trackSpeed = 8f;        // How fast spear rotates to face target during telegraph
    public float dashDuration = 0.35f;   // How long the dash travels in a line

    private float rotationDirection = 1f;
    private float nextAttackTime = 0f;
    private const float ATTACK_COOLDOWN = 0.4f;

    [HideInInspector] public bool isDashing = false;

    private void Awake()
    {
        if (parentBall == null)
        {
            parentBall = GetComponentInParent<SpearBall>();
        }
    }

    private void Start()
    {
        StartCoroutine(DashThrustIntervalRoutine());
    }

    private void Update()
    {
        if (parentBall == null || parentBall.isDead) return;

        // Spin normally when NOT performing a dash-thrust
        if (!isDashing && weaponPivot != null)
        {
            float hitBonus = (parentBall.currentDamage - 3f) * 10f;
            float currentSpeed = baseRotationSpeed + hitBonus;

            weaponPivot.Rotate(0, 0, currentSpeed * rotationDirection * Time.deltaTime);
        }
    }

    private IEnumerator DashThrustIntervalRoutine()
    {
        while (parentBall != null && !parentBall.isDead)
        {
            float waitTime = parentBall.currentInterval;
            yield return new WaitForSeconds(waitTime);

            Ball target = FindNearestEnemy();
            if (target != null && parentBall != null)
            {
                yield return StartCoroutine(ExecuteControlledDash(target));
            }
        }
    }

    private IEnumerator ExecuteControlledDash(Ball target)
    {
        isDashing = true;
        Rigidbody2D parentRb = parentBall.GetComponent<Rigidbody2D>();

        if (parentRb != null)
        {
            // 1. Telegraph & Track Target
            float trackTimer = 0.4f;
            while (trackTimer > 0f)
            {
                if (target == null || target.isDead) break;

                parentRb.linearVelocity = Vector2.zero;

                Vector3 dirToTarget = (target.transform.position - parentBall.transform.position).normalized;

                // REMOVED "- 90f" OFFSET: Maps 0 degrees directly to your sprite's tip vector
                float targetAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;

                if (weaponPivot != null)
                {
                    Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
                    weaponPivot.rotation = Quaternion.Slerp(weaponPivot.rotation, targetRot, Time.deltaTime * trackSpeed);
                }

                trackTimer -= Time.deltaTime;
                yield return null;
            }

            // 2. Lock in dash direction directly towards target
            Vector2 dashDirection = (target != null)
                ? (Vector2)(target.transform.position - parentBall.transform.position).normalized
                : (Vector2)weaponPivot.up;

            // 3. Uniform Speed Dash in a Straight Line
            float dashTimer = dashDuration;
            while (dashTimer > 0f)
            {
                parentRb.linearVelocity = dashDirection * dashSpeed;
                dashTimer -= Time.deltaTime;
                yield return null;
            }

            // 4. Stop movement briefly post-dash
            parentRb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(0.1f);

            // 5. Launch in a random direction to resume normal movement
            Vector2 randomLaunch = Random.insideUnitCircle.normalized * (parentBall.data != null ? parentBall.data.launchSpeed : 8f);
            parentRb.linearVelocity = randomLaunch;
        }

        isDashing = false;
    }

    public void ReverseRotation()
    {
        rotationDirection *= -1f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Sword") || collision.CompareTag("Arrow") || collision.CompareTag("Spear"))
        {
            if (!isDashing) ReverseRotation();
            return;
        }

        Ball target = collision.GetComponent<Ball>();

        if (target != null && target != parentBall)
        {
            if (Time.time < nextAttackTime) return;

            if (parentBall != null)
            {
                nextAttackTime = Time.time + ATTACK_COOLDOWN;

                if (isDashing)
                {
                    float damage = parentBall.RegisterThrustHit();
                    target.TakeDamage(damage);
                }
                else
                {
                    ReverseRotation();
                    float damage = parentBall.RegisterStandardHit();
                    target.TakeDamage(damage);
                }
            }
        }
    }

    private Ball FindNearestEnemy()
    {
        Ball[] allBalls = Object.FindObjectsByType<Ball>(FindObjectsSortMode.None);
        Ball nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var ball in allBalls)
        {
            if (ball != parentBall && !ball.isDead)
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
}
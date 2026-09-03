using System.Collections;
using UnityEngine;

public class SpearWeapon : MonoBehaviour
{
    public SpearBall parentBall;
    public Transform weaponPivot;

    [Header("Spin & Dash Settings")]
    public float baseRotationSpeed = 100f;
    public float dashSpeed = 16f;        // Uniform linear velocity
    public float trackSpeed = 8f;        // Target tracking rotation speed
    public float dashDuration = 0.35f;   // Straight-line dash length

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
            // 1. Telegraph & Track Target (Points spear directly at enemy while stationary)
            float trackTimer = 0.4f;
            while (trackTimer > 0f)
            {
                if (target == null || target.isDead) break;

                parentRb.linearVelocity = Vector2.zero;

                Vector3 dirToTarget = (target.transform.position - parentBall.transform.position).normalized;
                float targetAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;

                if (weaponPivot != null)
                {
                    Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
                    weaponPivot.rotation = Quaternion.Slerp(weaponPivot.rotation, targetRot, Time.deltaTime * trackSpeed);
                }

                trackTimer -= Time.deltaTime;
                yield return null;
            }

            // Lock dash direction vector towards target
            Vector2 dashDirection = (target != null && !target.isDead)
                ? (Vector2)(target.transform.position - parentBall.transform.position).normalized
                : (Vector2)weaponPivot.up;

            // 2. Uniform Speed Dash in a Straight Line
            float dashTimer = dashDuration;
            while (dashTimer > 0f)
            {
                parentRb.linearVelocity = dashDirection * dashSpeed;
                dashTimer -= Time.deltaTime;
                yield return null;
            }

            // 3. Pause briefly post-dash
            parentRb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(0.1f);

            // 4. Launch in a random direction to resume physics bounce
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
        // 1. Weapon clash handling
        if (collision.CompareTag("Sword") || collision.CompareTag("Arrow") || collision.CompareTag("Spear"))
        {
            if (!isDashing) ReverseRotation();
            return;
        }

        // 2. Hit Enemy Ball
        Ball target = collision.GetComponent<Ball>();

        if (target != null && target != parentBall)
        {
            if (Time.time < nextAttackTime) return;

            if (parentBall != null)
            {
                nextAttackTime = Time.time + ATTACK_COOLDOWN;

                if (isDashing)
                {
                    // Dash-Thrust Hit: 25% Crit chance roll
                    float damage = parentBall.RegisterThrustHit();
                    bool isCrit = parentBall.RollCrit(25f);
                    if (isCrit) damage *= 2f;

                    target.TakeDamage(damage, isCrit);
                }
                else
                {
                    // Standard Hit: 5% Crit chance roll
                    ReverseRotation();
                    float damage = parentBall.RegisterStandardHit();
                    bool isCrit = parentBall.RollCrit(5f);
                    if (isCrit) damage *= 2f;

                    target.TakeDamage(damage, isCrit);
                }
            }
        }
    }

    private Ball FindNearestEnemy()
    {
        // Updated Unity 6 API call to resolve CS0618 warning
        Ball[] allBalls = Object.FindObjectsByType<Ball>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
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
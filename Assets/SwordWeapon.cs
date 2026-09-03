using UnityEngine;

public class SwordWeapon : MonoBehaviour
{
    public SwordBall parentBall;
    public Transform weaponPivot;

    [Header("Spin Settings")]
    public float baseRotationSpeed = 360f;
    public float speedPerDamagePoint = 15f; // Extra degrees/sec gained per damage point accrued
    public float maxRotationSpeed = 900f;   // Visual cap so it doesn't become a blurry drill
    private float rotationDirection = 1f;

    private float nextAttackTime = 0f;
    private const float ATTACK_COOLDOWN = 0.5f;

    private void Awake()
    {
        if (parentBall == null)
        {
            parentBall = GetComponentInParent<SwordBall>();
        }
    }

    private void Start()
    {
        baseRotationSpeed = Random.Range(320f, 380f);
    }

    private void Update()
    {
        if (parentBall == null || parentBall.isDead) return;

        // Calculate extra speed gained from accrued compound damage (above base 5.0)
        float accruedDamage = Mathf.Max(0f, parentBall.currentDamage - 5.0f);
        float currentSpeed = baseRotationSpeed + (accruedDamage * speedPerDamagePoint);

        // Clamp to prevent uncontrollable visual artifacts
        currentSpeed = Mathf.Min(currentSpeed, maxRotationSpeed);

        if (weaponPivot != null)
        {
            weaponPivot.Rotate(0, 0, currentSpeed * rotationDirection * Time.deltaTime);
        }
    }

    public void ReverseRotation()
    {
        rotationDirection *= -1f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Sword") || collision.CompareTag("Arrow") || collision.CompareTag("Spear"))
        {
            ReverseRotation();
            return;
        }

        Ball target = collision.GetComponent<Ball>();

        if (target != null && target != parentBall)
        {
            if (Time.time < nextAttackTime) return;

            if (parentBall != null)
            {
                nextAttackTime = Time.time + ATTACK_COOLDOWN;
                ReverseRotation();

                float damageToDeal = parentBall.CalculateSwordDamage();
                bool isCrit = parentBall.RollCrit(5f);
                if (isCrit) damageToDeal *= 2f;

                target.TakeDamage(damageToDeal, isCrit);
            }
        }
    }
}
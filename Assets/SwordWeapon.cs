using UnityEngine;

public class SwordWeapon : MonoBehaviour
{
    public SwordBall parentBall;
    public Transform weaponPivot;

    [Header("Spin Settings")]
    public float baseRotationSpeed = 360f; // Faster starting spin speed
    public float speedScaleFactor = 0.5f;
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
        baseRotationSpeed = Random.Range(320f, 420f); // Fast initial spin
    }

    private void Update()
    {
        if (parentBall == null || parentBall.isDead) return;

        // Dynamically accelerate spin relative to current damage multiplier
        float currentSpeed = baseRotationSpeed * (1f + (parentBall.currentMultiplier - 1f) * speedScaleFactor);

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
        if (collision.CompareTag("Sword") || collision.CompareTag("Arrow"))
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

                float damage = parentBall.CalculateSwordDamage();
                target.TakeDamage(damage);
            }
        }
    }
}
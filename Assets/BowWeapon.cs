using System.Collections;
using UnityEngine;

public class BowWeapon : MonoBehaviour
{
    public BowBall parentBall;
    public Transform weaponPivot;
    public Transform firePoint;
    public GameObject arrowPrefab;

    [Header("Spin Settings")]
    public float baseRotationSpeed = 150f;
    public float speedPerArrow = 35f;

    private const float BURST_SHOT_DELAY = 0.04f;
    private const float VOLLEY_COOLDOWN = 0.45f;
    private bool isFiringBow = false;

    private void Awake()
    {
        if (parentBall == null)
        {
            parentBall = GetComponentInParent<BowBall>();
        }
    }

    private void Update()
    {
        if (parentBall == null || parentBall.isDead) return;

        int arrowCount = (parentBall != null) ? parentBall.currentArrowCount : 3;
        float currentRotationSpeed = baseRotationSpeed + (arrowCount * speedPerArrow);

        if (weaponPivot != null)
        {
            weaponPivot.Rotate(0, 0, currentRotationSpeed * Time.deltaTime);
        }

        if (!isFiringBow)
        {
            StartCoroutine(FireBowBurstRoutine());
        }
    }

    private IEnumerator FireBowBurstRoutine()
    {
        isFiringBow = true;
        int arrowsToFire = parentBall.currentArrowCount;

        for (int i = 0; i < arrowsToFire; i++)
        {
            if (parentBall.isDead) yield break;

            if (arrowPrefab != null)
            {
                Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position;
                Vector2 fireDirection = (firePoint != null) ? (firePoint.position - transform.position).normalized : transform.up;
                float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;

                GameObject arrowObj = Instantiate(arrowPrefab, spawnPos, Quaternion.Euler(0, 0, angle));
                Arrow arrow = arrowObj.GetComponent<Arrow>();

                if (arrow != null && parentBall != null)
                {
                    arrow.owner = parentBall;

                    // OVERRIDE DEFULT DAMAGE: Fetch scaled damage directly from BowBall!
                    arrow.damage = parentBall.GetCalculatedArrowDamage();
                }

                Rigidbody2D arrowRb = arrowObj.GetComponent<Rigidbody2D>();
                if (arrowRb != null)
                {
                    arrowRb.bodyType = RigidbodyType2D.Dynamic;
                    arrowRb.gravityScale = 0f;
                    arrowRb.linearVelocity = fireDirection * 18f;
                }
            }

            yield return new WaitForSeconds(BURST_SHOT_DELAY);
        }

        // Apply hit bonus count after full volley finishes
        parentBall.ApplyVolleyHitBonus();

        yield return new WaitForSeconds(VOLLEY_COOLDOWN);
        isFiringBow = false;
    }
}
using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Keep physics updates clean so it doesn't conflict with Combat.cs
    void FixedUpdate()
    {
        if (rb != null && rb.linearVelocity.magnitude < 2f && Time.timeScale > 0f)
        {
            // Re-energize the ball if it stops moving
            rb.linearVelocity = rb.linearVelocity.normalized * 5f;
        }
    }
}
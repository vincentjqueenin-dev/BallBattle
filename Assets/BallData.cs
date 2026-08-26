using UnityEngine;

[CreateAssetMenu(fileName = "NewBallData", menuName = "BattleSim/Ball Data")]
public class BallData : ScriptableObject
{
    public string ballName = "Default Fighter";
    public float maxHealth = 100f;
    public float minDamage = 1f;
    public float maxDamage = 25f;
    public float knockbackForce = 4f; // Decreased knockback
    public float launchSpeed = 8f;    // Decreased launch speed
    public float maxSpeed = 12f;       // Universal/configurable speed cap
    public Color ballColor = Color.white;
}
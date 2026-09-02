using UnityEngine;

[CreateAssetMenu(fileName = "NewBallData", menuName = "Ball System/Ball Data")]
public class BallData : ScriptableObject
{
    public string ballName;
    public Color ballColor;
    public float maxHealth = 100f;
    public float minDamage = 1f;
    public float maxDamage = 25f;
    public float launchSpeed = 10f;
    public float maxSpeed = 15f;
    public float knockbackForce = 5f;

    [Header("Class Prefab Link")]
    public GameObject classPrefab; // Drag BowBall_Prefab, SwordBall_Prefab, or UnarmedBall_Prefab here!
}
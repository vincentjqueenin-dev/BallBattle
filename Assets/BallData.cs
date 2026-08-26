using UnityEngine;

// Use lowercase 'fileName' and 'menuName'
[CreateAssetMenu(fileName = "NewBallData", menuName = "BattleSim/Ball Data")]
public class BallData : ScriptableObject
{
    public string ballName = "Default Fighter";
    public float maxHealth = 100f;
    public float minDamage = 10f;
    public float maxDamage = 25f;
    public float knockbackForce = 8f;
    public float launchSpeed = 10f;
    public Color ballColor = Color.white;
}
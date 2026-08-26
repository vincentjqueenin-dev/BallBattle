using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;

    [Header("Selected Fighters")]
    public BallData player1Data;
    public BallData player2Data;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
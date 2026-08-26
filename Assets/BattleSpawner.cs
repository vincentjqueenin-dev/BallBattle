using UnityEngine;

public class BattleSpawner : MonoBehaviour
{
    public GameObject ballPrefab;
    public Transform spawnPointP1;
    public Transform spawnPointP2;

    [Header("Fallback Default")]
    public BallData defaultBallData;

    void Start()
    {
        BallData p1Data = defaultBallData;
        BallData p2Data = defaultBallData;

        if (MatchManager.Instance != null)
        {
            if (MatchManager.Instance.player1Data != null)
                p1Data = MatchManager.Instance.player1Data;

            if (MatchManager.Instance.player2Data != null)
                p2Data = MatchManager.Instance.player2Data;
        }

        // Spawn Player 1
        if (ballPrefab != null && spawnPointP1 != null)
        {
            GameObject b1 = Instantiate(ballPrefab, spawnPointP1.position, Quaternion.identity);
            b1.name = "P1_" + (p1Data != null ? p1Data.ballName : "Ball");

            Combat c1 = b1.GetComponent<Combat>();
            if (c1 != null && p1Data != null) c1.ApplyData(p1Data);
        }

        // Spawn Player 2
        if (ballPrefab != null && spawnPointP2 != null)
        {
            GameObject b2 = Instantiate(ballPrefab, spawnPointP2.position, Quaternion.identity);
            b2.name = "P2_" + (p2Data != null ? p2Data.ballName : "Ball");

            Combat c2 = b2.GetComponent<Combat>();
            if (c2 != null && p2Data != null) c2.ApplyData(p2Data);
        }
    }
}
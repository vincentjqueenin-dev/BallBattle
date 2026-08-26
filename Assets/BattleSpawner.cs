using UnityEngine;
using TMPro;

public class BattleSpawner : MonoBehaviour
{
    public GameObject ballPrefab;
    public Transform spawnPointP1;
    public Transform spawnPointP2;

    [Header("UI HUD References")]
    public TMP_Text p1PassiveHud;
    public TMP_Text p2PassiveHud;

    [Header("Fallback Default")]
    public BallData defaultBallData;

    void Start()
    {
        BallData p1Data = defaultBallData;
        BallData p2Data = defaultBallData;

        if (MatchManager.Instance != null)
        {
            if (MatchManager.Instance.player1Data != null) p1Data = MatchManager.Instance.player1Data;
            if (MatchManager.Instance.player2Data != null) p2Data = MatchManager.Instance.player2Data;
        }

        // Spawn P1 Ball & Assign HUD
        if (spawnPointP1 != null && ballPrefab != null)
        {
            GameObject ball1 = Instantiate(ballPrefab, spawnPointP1.position, Quaternion.identity);
            Combat combat1 = ball1.GetComponent<Combat>();
            if (combat1 != null)
            {
                combat1.debugHudText = p1PassiveHud; // <--- Linked here
                combat1.ApplyData(p1Data);
            }
        }

        // Spawn P2 Ball & Assign HUD
        if (spawnPointP2 != null && ballPrefab != null)
        {
            GameObject ball2 = Instantiate(ballPrefab, spawnPointP2.position, Quaternion.identity);
            Combat combat2 = ball2.GetComponent<Combat>();
            if (combat2 != null)
            {
                combat2.debugHudText = p2PassiveHud; // <--- Linked here
                combat2.ApplyData(p2Data);
            }
        }
    }
}
using UnityEngine;
using TMPro;

public class BattleSpawner : MonoBehaviour
{
    public Transform spawnPointP1;
    public Transform spawnPointP2;

    [Header("UI HUD References")]
    public TMP_Text p1PassiveHud;
    public TMP_Text p2PassiveHud;

    [Header("Fallback Settings")]
    public GameObject defaultBallPrefab; // Fallback if classPrefab is unassigned
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

        // Spawn P1 Ball
        if (spawnPointP1 != null)
        {
            SpawnAndConfigureBall(spawnPointP1.position, p1Data, p1PassiveHud);
        }

        // Spawn P2 Ball
        if (spawnPointP2 != null)
        {
            SpawnAndConfigureBall(spawnPointP2.position, p2Data, p2PassiveHud);
        }
    }

    private void SpawnAndConfigureBall(Vector3 position, BallData data, TMP_Text hudText)
    {
        // 1. Try classPrefab first; fall back to defaultBallPrefab if null
        GameObject prefabToSpawn = null;

        if (data != null && data.classPrefab != null)
        {
            prefabToSpawn = data.classPrefab;
        }
        else
        {
            prefabToSpawn = defaultBallPrefab;
        }

        // Safety check if both are empty
        if (prefabToSpawn == null)
        {
            Debug.LogError($"[BattleSpawner] Cannot spawn! Both classPrefab on '{data?.name}' and defaultBallPrefab on BattleSpawner are unassigned.");
            return;
        }

        // 2. Instantiate and configure
        GameObject ballObj = Instantiate(prefabToSpawn, position, Quaternion.identity);

        Ball ballComponent = ballObj.GetComponent<Ball>();
        if (ballComponent != null)
        {
            ballComponent.debugHudText = hudText;
            ballComponent.ApplyData(data);
        }
    }
}
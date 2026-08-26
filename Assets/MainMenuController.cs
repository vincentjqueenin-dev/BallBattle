using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject selectionPanel;

    [Header("Dropdowns")]
    public TMP_Dropdown p1Dropdown;
    public TMP_Dropdown p2Dropdown;

    [Header("Available Fighter Options")]
    public List<BallData> availableBalls = new List<BallData>();

    void Start()
    {
        if (p1Dropdown == null || p2Dropdown == null) return;

        p1Dropdown.ClearOptions();
        p2Dropdown.ClearOptions();

        List<string> options = new List<string>();
        foreach (var ball in availableBalls)
        {
            if (ball != null) options.Add(ball.ballName);
        }

        p1Dropdown.AddOptions(options);
        p2Dropdown.AddOptions(options);
    }

    public void OnPlayButtonClicked()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (selectionPanel != null) selectionPanel.SetActive(true);
    }

    public void OnStartFightClicked()
    {
        if (MatchManager.Instance != null && availableBalls.Count > 0)
        {
            // Safely assign data from dropdown selection
            int p1Index = Mathf.Clamp(p1Dropdown.value, 0, availableBalls.Count - 1);
            int p2Index = Mathf.Clamp(p2Dropdown.value, 0, availableBalls.Count - 1);

            MatchManager.Instance.player1Data = availableBalls[p1Index];
            MatchManager.Instance.player2Data = availableBalls[p2Index];
        }

        SceneManager.LoadScene("SampleScene");
    }
}
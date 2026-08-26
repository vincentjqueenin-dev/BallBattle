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
    public BallData[] availableBalls;

    void Start()
    {
        p1Dropdown.ClearOptions();
        p2Dropdown.ClearOptions();

        List<string> options = new List<string>();
        foreach (var ball in availableBalls)
        {
            options.Add(ball.ballName);
        }

        p1Dropdown.AddOptions(options);
        p2Dropdown.AddOptions(options);
    }

    public void OnPlayButtonClicked()
    {
        mainPanel.SetActive(false);
        selectionPanel.SetActive(true);
    }

    public void OnStartFightClicked()
    {
        if (MatchManager.Instance != null && availableBalls.Length > 0)
        {
            MatchManager.Instance.player1Data = availableBalls[p1Dropdown.value];
            MatchManager.Instance.player2Data = availableBalls[p2Dropdown.value];
        }

        SceneManager.LoadScene("SampleScene");
    }
}
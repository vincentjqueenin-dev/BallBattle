using System.Collections;
using UnityEngine;
using TMPro;

public class BattleCountdownManager : MonoBehaviour
{
    public TMP_Text countdownText;

    void Start()
    {
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        Time.timeScale = 0f; // Freeze physics during countdown

        if (countdownText != null)
        {
            countdownText.text = "3";
            yield return new WaitForSecondsRealtime(1f);

            countdownText.text = "2";
            yield return new WaitForSecondsRealtime(1f);

            countdownText.text = "1";
            yield return new WaitForSecondsRealtime(1f);

            countdownText.text = "FIGHT!";

            Time.timeScale = 1f; // UNPAUSE PHYSICS

            yield return new WaitForSecondsRealtime(0.8f);
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            Time.timeScale = 1f; // Fallback unpause
        }
    }
}
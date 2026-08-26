using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionHelper : MonoBehaviour
{
    public void StartReturnSequence()
    {
        StartCoroutine(ReturnToMenuRoutine());
    }

    private IEnumerator ReturnToMenuRoutine()
    {
        // Wait 3 seconds for victory zoom/celebration
        yield return new WaitForSeconds(3f);

        // Load Menu Scene (Ensure exact name matches your scene)
        SceneManager.LoadScene("MenuScene");
    }
}
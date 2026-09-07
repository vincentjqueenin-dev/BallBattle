using UnityEngine;

public class UIController : MonoBehaviour
{
    // Assign this method to the OnClick() event of your UI Button
    public void QuitGame()
    {
        #if UNITY_EDITOR
                // Stop playing the scene in the Unity Editor
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                // Quit the built application
                Application.Quit();
        #endif
    }
}
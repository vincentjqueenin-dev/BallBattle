using UnityEngine;
using UnityEngine.InputSystem; // Added for the new Input System
using TMPro;

public class HoldToQuit : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI quitText;
    [SerializeField] private CanvasGroup textCanvasGroup;

    [Header("Hold Settings")]
    [SerializeField] private float holdDuration = 2.0f;
    [SerializeField] private float fadeSpeed = 3.0f;

    private float holdTimer = 0.0f;

    private void Start()
    {
        if (textCanvasGroup != null)
        {
            textCanvasGroup.alpha = 0f;
        }
    }

    private void Update()
    {
        // New Input System check for Escape key
        if (Keyboard.current != null && Keyboard.current.escapeKey.isPressed)
        {
            holdTimer += Time.deltaTime;

            float targetAlpha = Mathf.Clamp01(holdTimer / holdDuration);
            if (textCanvasGroup != null)
            {
                textCanvasGroup.alpha = Mathf.MoveTowards(textCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            }

            if (holdTimer >= holdDuration)
            {
                Quit();
            }
        }
        else
        {
            holdTimer = 0f;
            if (textCanvasGroup != null)
            {
                textCanvasGroup.alpha = Mathf.MoveTowards(textCanvasGroup.alpha, 0f, fadeSpeed * Time.deltaTime);
            }
        }
    }

    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
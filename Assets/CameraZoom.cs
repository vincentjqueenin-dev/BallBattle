using System.Collections;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public static CameraZoom Instance;
    private Camera cam;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        cam = GetComponent<Camera>();
    }

    public void ZoomToTarget(Transform target, float targetSize = 2.5f, float duration = 2f)
    {
        if (target == null || cam == null) return;
        StartCoroutine(ZoomRoutine(target, targetSize, duration));
    }

    private IEnumerator ZoomRoutine(Transform target, float targetSize, float duration)
    {
        Vector3 startPos = transform.position;
        float startSize = cam.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (target != null)
            {
                Vector3 targetPos = new Vector3(target.position.x, target.position.y, startPos.z);
                transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            }

            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, elapsed / duration);
            elapsed += Time.unscaledDeltaTime; // Uses unscaled time in case Time.timeScale is altered
            yield return null;
        }
    }
}
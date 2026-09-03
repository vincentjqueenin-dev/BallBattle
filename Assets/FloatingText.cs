using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public TextMeshPro textMesh;
    private float floatSpeed = 2f;
    private float fadeSpeed = 2f;
    private Color textColor;

    public void Setup(string text, Color color)
    {
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
        textMesh.text = text;
        textColor = color;
        textMesh.color = textColor;
        Destroy(gameObject, 0.8f);
    }

    private void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        textColor.a -= fadeSpeed * Time.deltaTime;
        if (textMesh != null) textMesh.color = textColor;
    }
}
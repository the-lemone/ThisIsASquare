using TMPro;
using UnityEngine;

public class BlinkingText : MonoBehaviour
{
    [SerializeField] private float blinkInterval = 0.5f;
    private TextMeshPro tmp;
    private bool isVisible = true;
    private float timer;

    private void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= blinkInterval)
        {
            isVisible = !isVisible;
            tmp.text = isVisible ? "^" : "";
            timer = 0f;
        }
    }
}

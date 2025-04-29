using System.Collections;
using UnityEngine;
public class FadeOut : MonoBehaviour
{
    public float fadeDuration = 1f;
    public CanvasGroup fadeCanvas; // UI Canvas Group for fading
    
    void Start()
    {
        StartCoroutine(FadeScreen(0f));
    }
    
    IEnumerator FadeScreen(float targetAlpha)
    {
        float startAlpha = fadeCanvas.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            fadeCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        fadeCanvas.alpha = targetAlpha;
    }
}

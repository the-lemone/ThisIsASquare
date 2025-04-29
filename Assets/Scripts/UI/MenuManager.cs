using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Buttons")]
    public GameObject start;
    public GameObject settings;
    public GameObject quit;
    public GameObject credits;
    
    [Header("Sub Menus")]
    public GameObject creditsMenu;
    public GameObject settingsMenu;
    
    [Header("Level Transitions")]
    public float fadeDuration = 1f;
    public string nextLevel;
    public CanvasGroup fadeCanvas; // UI Canvas Group for fading
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (start.activeSelf)
                StartCoroutine(TransitionToNextLevel());
            if (settings.activeSelf)
            {
                settingsMenu.SetActive(true);
            }
            if (quit.activeSelf)
            {
                Application.Quit();
                Debug.Log("Quit");
            }
            if (credits.activeSelf)
                creditsMenu.SetActive(true);
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if(creditsMenu.activeSelf)
                creditsMenu.SetActive(false);
            if(settingsMenu.activeSelf)
                settingsMenu.SetActive(false);
        }
        
        if (!credits.activeSelf)
            creditsMenu.SetActive(false);
        if (!settings.activeSelf)
            settingsMenu.SetActive(false);
    }
    
    IEnumerator TransitionToNextLevel()
    {
        // Start fade to black
        yield return StartCoroutine(FadeScreen(1f));
        SceneManager.LoadScene(nextLevel);
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

using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitGame : MonoBehaviour
{
    void Update()
    {
        #if UNITY_STANDALONE || UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        #endif
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            SceneManager.LoadScene("Level 0.1");
        }
    }
}

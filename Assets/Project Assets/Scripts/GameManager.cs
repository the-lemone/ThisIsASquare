using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
            SceneManager.LoadScene(0);
        
        if(Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
}

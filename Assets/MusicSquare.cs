using UnityEngine;

public class MusicSquare : MonoBehaviour
{
    public static MusicSquare instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject); // destroy duplicate
        }
    }
}

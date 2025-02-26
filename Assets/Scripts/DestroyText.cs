using UnityEngine;

public class DestroyText : MonoBehaviour
{
    private float secondsToDestroy = 5f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, secondsToDestroy);
    }
}

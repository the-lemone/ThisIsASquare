using UnityEngine;

public class TextScript : MonoBehaviour
{
    private Vector3 offset;
    private Vector3 newPos;
    public GameObject player;
    
    private float secondsToDestroy = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        offset = player.transform.position - transform.position;
        Destroy(gameObject, secondsToDestroy);
    }

    void Update()
    {
        newPos = player.transform.position - offset;
        transform.position = newPos;
    }
}

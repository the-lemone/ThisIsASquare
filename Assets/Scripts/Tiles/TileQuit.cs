using UnityEngine;

public class TileQuit : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Application.Quit();
            Debug.Log("Quit");
        }
    }
}

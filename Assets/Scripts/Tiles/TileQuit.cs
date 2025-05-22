using UnityEngine;

public class TileQuit : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            #if UNITY_STANDALONE || UNITY_EDITOR
            Application.Quit();
            Debug.Log("Quit");
            #endif
        }
    }
}

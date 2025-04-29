using UnityEngine;

public class MenuTrigger : MonoBehaviour
{
    public GameObject textMenu;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            textMenu.SetActive(true);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            textMenu.SetActive(false);
        }
    }
}

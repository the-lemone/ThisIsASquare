using UnityEngine;

public class MenuTrigger : MonoBehaviour
{
    public GameObject textMenu;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log(other.gameObject.name);
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

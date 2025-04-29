using UnityEngine;

public class TitleTrigger : MonoBehaviour
{
    public GameObject title;
    public GameObject[] indicators;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            title.SetActive(true);
            indicators[0].SetActive(true);
            indicators[1].SetActive(true);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            title.SetActive(false);
            indicators[0].SetActive(false);
            indicators[1].SetActive(false);
        }
    }
}

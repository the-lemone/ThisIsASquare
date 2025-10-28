using UnityEngine;

public class VerticalRotationUnlock : MonoBehaviour
{
    [SerializeField] private float speed;
    void Update()
    {
        transform.Rotate((Time.deltaTime * speed), 0, 0);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<WorldController>().UnlockVerticalRotation();
            Destroy(gameObject);
        }
    }
}

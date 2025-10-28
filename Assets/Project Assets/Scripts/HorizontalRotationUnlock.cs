using UnityEngine;

public class HorizontalRotationUnlock : MonoBehaviour
{
    [SerializeField] private float speed;
    void Update()
    {
        transform.Rotate(0, 0, -(Time.deltaTime * speed));
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FindFirstObjectByType<WorldController>().UnlockHorizontalRotation();
            Destroy(gameObject);
        }
    }
}

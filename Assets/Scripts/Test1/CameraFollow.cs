using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The object the camera follows
    public Vector3 offset; // Default camera offset
    public float smoothSpeed = 5f; // Speed of camera smoothing
    private Quaternion targetRotation;
    private bool isRotating = false;

    void Start()
    {
        targetRotation = transform.rotation; // Store initial camera rotation
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (gameObject.transform.rotation == Quaternion.Euler(90, 0, 0))
        {
            offset = new Vector3(0, 10, 0);
        }
        else if (gameObject.transform.rotation == Quaternion.Euler(0, 180, 0))
        {
            offset = new Vector3(0, 0, 10);
        }
        
        // Follow the player without rotation changes
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Only rotate when explicitly told to
        if (!isRotating)
        {
            transform.rotation = targetRotation;
        }
        
    }

    public void RotateCamera(Quaternion newRotation)
    {
        StartCoroutine(SmoothRotate(newRotation));
    }

    private System.Collections.IEnumerator SmoothRotate(Quaternion newRotation)
    {
        isRotating = true;
        Quaternion startRotation = transform.rotation;
        float elapsedTime = 0f;
        float rotationSpeed = 2f; // Adjust rotation speed as needed

        while (elapsedTime < 1f)
        {
            transform.rotation = Quaternion.Slerp(startRotation, newRotation, elapsedTime);
            elapsedTime += Time.deltaTime * rotationSpeed;
            yield return null;
        }

        transform.rotation = newRotation;
        targetRotation = newRotation; // Store new target rotation
        isRotating = false;
    }
    
}

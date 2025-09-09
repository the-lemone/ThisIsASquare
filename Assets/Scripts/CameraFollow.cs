using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform target;
    public float positionSmoothTime = 0.2f;
    public float rotationSmoothTime = 0.5f;

    private Vector3 velocity;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Pivot").transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!target) return;
        
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, positionSmoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, rotationSmoothTime * Time.deltaTime);
    }
}

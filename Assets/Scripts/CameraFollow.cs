using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform _target; // Set to CameraPivot to follow
    public float positionSmoothTime = 0.2f;
    public float rotationSmoothTime = 5f;
    
    private Vector3 _velocity;

    void Start()
    {
        _target = GameObject.FindGameObjectWithTag("Pivot").transform;
    }
    
    void LateUpdate()
    {
        if (!_target) return;
        
        // Smooth position toward the pivot
        transform.position = Vector3.SmoothDamp(transform.position, _target.position, ref _velocity, positionSmoothTime);

        // Smooth rotation to match pivot's rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, _target.rotation, rotationSmoothTime * Time.deltaTime);
    }
}
using UnityEngine;

public class WorldController : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 5f; // how fast rotation happens
    [SerializeField] private Transform cameraTransform; // reference to camera

    private Quaternion _startRotation, _targetRotation;
    private float _t;
    private bool _isRotating;

    private PlayerControls _controls;

    void Awake()
    {
        _controls = new PlayerControls();

        // Rotations around camera horizontal
        _controls.Player.RotateLeft.performed += _ => TryRotate(-90f, cameraTransform.up);
        _controls.Player.RotateRight.performed += _ => TryRotate(90f, cameraTransform.up);
        
        // Rotations around camera vertical
        _controls.Player.RotateUp.performed += _ => TryRotate(-90f, cameraTransform.right);
        _controls.Player.RotateDown.performed += _ => TryRotate(90f, cameraTransform.right);
    }
    
    void OnEnable() => _controls.Enable();
    void OnDisable() => _controls.Disable();

    void Start()
    {
        // Automatically assign main camera if not set in inspector
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }
    
    void Update()
    {
        if (_isRotating)
        {
            _t += Time.deltaTime * rotateSpeed;
            transform.rotation = Quaternion.Slerp(_startRotation, _targetRotation, _t);

            if (_t >= 1f)
                _isRotating = false;
        }
    }
    
    private void TryRotate(float angle, Vector3 axis)
    {
        if(!_isRotating)
            StartRotation(angle, axis);
    }
    
    private void StartRotation(float angle, Vector3 axis)
    {
        _startRotation = transform.rotation;
        _targetRotation = Quaternion.AngleAxis(angle, axis) * _startRotation;
        _t = 0f;
        _isRotating = true;
    }
}

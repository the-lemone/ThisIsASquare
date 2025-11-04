using UnityEngine;

public class WorldController : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 5f; // how fast rotation happens
    [SerializeField] private Transform cameraTransform; // reference to camera
    [SerializeField] private Collider areaBounds; // Box collider defining control area

    private Quaternion _startRotation, _targetRotation;
    private float _t;
    private bool _isRotating;
    private bool _isActive; // Only true if player is inside areaBounds

    private PlayerControls _controls;
    [SerializeField] private GameObject player;
    
    private bool canRotateHorizontal;
    private bool canRotateVertical;

    void Awake()
    {
#if UNITY_EDITOR
        PlayerPrefs.DeleteAll();
#endif
        
        _controls = new PlayerControls();

        // Rotations around camera horizontal
        _controls.Player.RotateLeft.performed += _ =>
        {
            if(canRotateHorizontal && gameObject.activeInHierarchy)
                TryRotate(-90f, cameraTransform.up);
        };
        _controls.Player.RotateRight.performed += _ =>
        {
            if(canRotateHorizontal && gameObject.activeInHierarchy)
                TryRotate(90f, cameraTransform.up);
        };
        
        // Rotations around camera vertical
        _controls.Player.RotateUp.performed += _ =>
        {
            if(canRotateVertical && gameObject.activeInHierarchy)
                TryRotate(-90f, cameraTransform.right);
        };
        _controls.Player.RotateDown.performed += _ =>
        {
            if(canRotateVertical && gameObject.activeInHierarchy)
                TryRotate(90f, cameraTransform.right);
        };
    }
    
    void OnEnable() => _controls.Enable();
    void OnDisable() => _controls.Disable();

    void Start()
    {
        // Automatically assign main camera if not set in inspector
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
        if(areaBounds == null)
            areaBounds = GetComponent<Collider>();
        
        canRotateHorizontal = PlayerPrefs.GetInt("CanRotateHorizontal", 0) == 1;
        canRotateVertical = PlayerPrefs.GetInt("CanRotateVertical", 0) == 1;
    }
    
    void Update()
    {
        if (!_isActive || !_isRotating) return;
        
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
        if (_isRotating) return;
            StartRotation(angle, axis);
    }
    
    private void StartRotation(float angle, Vector3 axis)
    {
        _startRotation = transform.rotation;
        _targetRotation = Quaternion.AngleAxis(angle, axis) * _startRotation;
        _t = 0f;
        _isRotating = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetActive(true);
            Debug.Log("Player entered");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetActive(false);
            Debug.Log("Player exited");
        }
    }
    
    public void SetActive(bool state)
    {
        _isActive = state;
        
    }
    
    public void UnlockHorizontalRotation()
    { 
        canRotateHorizontal = true;
        PlayerPrefs.SetInt("CanRotateHorizontal", 1);
        PlayerPrefs.Save();
    }

    public void UnlockVerticalRotation()
    {
        canRotateVertical = true;
        PlayerPrefs.SetInt("CanRotateVerical", 1);
        PlayerPrefs.Save();
    }
}

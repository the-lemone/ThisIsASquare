using System.Collections;
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
    
    private bool canMove = true;

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
        if(!canMove)
        {
            player.GetComponent<Player>()._controls.Disable();
        }
        else
        {
            player.GetComponent<Player>()._controls.Enable();
        }
    }
    
    private void TryRotate(float angle, Vector3 axis)
    {
        if (_isRotating || !_isActive) return;
        StartCoroutine(RotateRoutine(angle, axis));
    }
    
    private IEnumerator RotateRoutine(float angle, Vector3 axis)
    {
        _isRotating = true;
        canMove = false;

        _startRotation = transform.rotation;
        _targetRotation = Quaternion.AngleAxis(angle, axis) * _startRotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * rotateSpeed;
            transform.rotation = Quaternion.Slerp(_startRotation, _targetRotation, t);
            yield return null;
        }

        transform.rotation = _targetRotation; // ensure perfect final rotation
        _isRotating = false;

        // Small delay after rotation before enabling movement
        yield return new WaitForSeconds(0.5f);

        canMove = true;
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

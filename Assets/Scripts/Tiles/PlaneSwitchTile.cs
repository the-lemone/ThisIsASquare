using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneSwitchTile : MonoBehaviour
{
    public float shiftSpeed = 2f;
    public float switchCooldown = 0.5f; // Cooldown duration before allowing switching
    private bool _playerOnTile; // bool for detecting if the player is on a switch tile, used in TriggerEnter/Exit and Update
    private bool _isShifting; // used in Update, ShiftPlayer, and SmoothShift
    private bool _canShift; // Prevents instant triggering upon entry
    private Transform _player;
    private Dictionary<KeyCode, Transform> _availableShifts = new Dictionary<KeyCode, Transform>();
    
    private Dictionary<KeyCode, Vector3> _shiftDirections = new Dictionary<KeyCode, Vector3>
    {
        { KeyCode.W, new Vector3(0, -0.5f, 0.5f) },  // Forward
        { KeyCode.UpArrow, new Vector3(0, -0.5f, 0.5f) },  // Forward (Arrow Up)
        { KeyCode.D, new Vector3(0.5f, -0.5f, 0) },  // Right (D)
        { KeyCode.RightArrow, new Vector3(0.5f, -0.5f, 0) },  // Right (Arrow Right)
        { KeyCode.S, new Vector3(0, -0.5f, -0.5f) }, // Backward (S)
        { KeyCode.DownArrow, new Vector3(0, -0.5f, -0.5f) }, // Backward (Arrow Down)
        { KeyCode.A, new Vector3(-0.5f, -0.5f, 0) },  // Left (A)
        { KeyCode.LeftArrow, new Vector3(-0.5f, -0.5f, 0) }  // Left (Arrow Left)
    };

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Player On Tile");
            _playerOnTile = true;
            StartCoroutine(StartCooldown()); // Prevents immediate switching
            DetectAvailableShifts();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerOnTile = false;
            _canShift = false; // Reset cooldown on exit
        }
    }

    void Update()
    {
        if (_playerOnTile && !_isShifting && _canShift)
        {
            foreach (var entry in _availableShifts)
            {
                if (Input.GetKeyDown(entry.Key))
                {
                    ShiftPlayer(entry.Value);
                    break;
                }
            }
        }
    }

    void DetectAvailableShifts()
    {
        _availableShifts.Clear();
        
        foreach (var entry in _shiftDirections)
        {
            Vector3 worldOffset = transform.TransformPoint(entry.Value);
            Collider[] hitColliders = Physics.OverlapSphere(worldOffset, 0.1f);
            
            foreach (Collider hit in hitColliders)
            {
                if (hit.CompareTag("SwitchTile") && hit.transform != transform)
                {
                    _availableShifts[entry.Key] = hit.transform;
                    break;
                }
            }
        }
    }

    void ShiftPlayer(Transform target)
    {
        if (!_player) return;
        _isShifting = true;
        _canShift = false; // Prevent further inputs during shifting
        StartCoroutine(SmoothShift(target));
    }

    IEnumerator SmoothShift(Transform target)
    {
        Vector3 startPosition = _player.position;
        Quaternion startRotation = _player.rotation;
        Quaternion targetRotation = target.rotation; // Ensure player matches tile rotation
        
        float elapsedTime = 0f;
        
        while (elapsedTime < 1f)
        {
            _player.position = Vector3.Lerp(startPosition, target.position, elapsedTime);
            _player.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime);
            elapsedTime += Time.deltaTime * shiftSpeed;
            yield return null;
        }
        //Debug.Log("Shifting finished");
        _player.position = target.position;
        _player.rotation = targetRotation;
        _isShifting = false;
    }
    
    IEnumerator StartCooldown()
    {
        yield return new WaitForSeconds(switchCooldown);
        _canShift = true;
    }
}

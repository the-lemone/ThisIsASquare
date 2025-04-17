using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneSwitchTile : MonoBehaviour
{
    private const float ShiftSpeed = 5f;
    private const float SwitchCooldown = 0.25f; // Cooldown duration before allowing switching
    private bool _playerOnTile; // bool for detecting if the player is on a switch tile, used in TriggerEnter/Exit and Update
    public static bool isShifting; // used in Update, ShiftPlayer, and SmoothShift
    private bool _canShift; // Prevents instant triggering upon entry
    private Transform _player;
    private bool _hasShiftedThisEntry;
    private readonly Dictionary<KeyCode, Transform> _availableShifts = new Dictionary<KeyCode, Transform>();
    
    private readonly Dictionary<KeyCode, Vector3> _shiftDirections = new Dictionary<KeyCode, Vector3>
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
            _playerOnTile = true;
            _hasShiftedThisEntry = false; // Reset when entering a new tile
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
            _hasShiftedThisEntry = false;
        }
    }

    void Update()
    {
        if (_playerOnTile && !isShifting && _canShift && !_hasShiftedThisEntry)
        {
            foreach (var entry in _availableShifts)
            {
                if (Input.GetKey(entry.Key))
                {
                    _hasShiftedThisEntry = true; // Lock this tile after one shift
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
        isShifting = true;
        _canShift = false; // Prevent further inputs during shifting
        StartCoroutine(SmoothShift(target));
    }

    IEnumerator SmoothShift(Transform target)
    {
        Vector3 startPosition = _player.position;
        Quaternion startRotation = _player.rotation;
        Quaternion targetRotation = target.rotation; // Ensure player matches tile rotation

        float duration = 1f/ShiftSpeed;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration; // Normalized time from 0 to 1
            _player.position = Vector3.Lerp(startPosition, target.position, t);
            _player.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        _player.position = target.position;
        _player.rotation = targetRotation;
        yield return new WaitForSeconds(0.2f); // Small buffer after shift
        isShifting = false;
    }
    
    IEnumerator StartCooldown()
    {
        yield return new WaitForSeconds(SwitchCooldown);
        _canShift = true;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        foreach (var offset in _shiftDirections.Values)
        {
            Vector3 worldOffset = transform.TransformPoint(offset);
            Gizmos.DrawWireSphere(worldOffset, 0.1f);
        }
    }
}

using System.Collections;
using UnityEngine;

public class PlaneShifter : MonoBehaviour
{
    public Transform worldContainer; // The parent object containing all tiles and world elements
    public Transform player;
    public Transform[] planeOrigins; // Positions that represent different plane alignments
    private int _currentPlaneIndex;
    public float shiftSpeed = 2f;
    private bool _isShifting;
    private bool _canShift; // Tracks if the player is on a PlaneSwitch tile
    private Transform _currentSwitchTile; // Stores the current PlaneSwitch tile

    void Update()
    {
        if (!_isShifting)
        {
            DetectPlaneSwitch();
            HandlePlaneShiftInput();
        }
    }

    void DetectPlaneSwitch()
    {
        Collider[] hits = Physics.OverlapBox(player.position, new Vector3(1f, 1f, 1f)); // 3D overlap detection

        Debug.DrawRay(player.position, new Vector3(1f, 1f, 1f), Color.green);
        
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("PlaneSwitch") && Vector3.Distance(hit.transform.position, player.position) < 1f)
            {
                _canShift = true;
                _currentSwitchTile = hit.transform;
                return; // Stop checking after finding a valid tile
                
            }
        }

        // If no exact match was found, reset canShift
        _canShift = false;
        _currentSwitchTile = null;
    }

    void HandlePlaneShiftInput()
    {
        if (_canShift && !_isShifting)
        {
            if (Input.GetKeyDown(KeyCode.W)) ShiftPlane(Vector3.forward);
            if (Input.GetKeyDown(KeyCode.S)) ShiftPlane(Vector3.back);
            if (Input.GetKeyDown(KeyCode.A)) ShiftPlane(Vector3.left);
            if (Input.GetKeyDown(KeyCode.D)) ShiftPlane(Vector3.right);
        }
    }

    void ShiftPlane(Vector3 direction)
    {
        if (!_currentSwitchTile || _isShifting) return;

        int newPlaneIndex = (_currentPlaneIndex + 1) % planeOrigins.Length;
        StartCoroutine(SmoothShift(newPlaneIndex, _currentSwitchTile));
    }

    IEnumerator SmoothShift(int newPlaneIndex, Transform targetTile)
    {
        _isShifting = true;
        _canShift = false;
        Vector3 startPosition = player.position;
        Quaternion startRotation = player.rotation;
        
        // Set target rotation to match the tile's rotation
        Quaternion targetRotation = targetTile.rotation;
        Vector3 targetPosition = targetTile.position;
        
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            player.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
            player.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime);
            elapsedTime += Time.deltaTime * shiftSpeed;
            yield return null;
        }

        player.position = targetPosition;
        player.rotation = targetRotation;

        _currentPlaneIndex = newPlaneIndex;
        _isShifting = false;
    }
}

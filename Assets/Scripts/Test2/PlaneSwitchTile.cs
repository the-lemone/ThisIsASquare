using System.Collections;
using UnityEngine;

public class PlaneSwitchTile : MonoBehaviour
{
    public float shiftSpeed = 2f;
    public float switchCooldown = 0.5f; // Cooldown duration before allowing switching
    private bool playerOnTile;
    private bool isShifting;
    private bool canShift; // Prevents instant triggering upon entry
    private Transform player;
    private Transform targetTile;
    private GridMover gridMover; // Reference to GridMover3D for rotation alignment

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        gridMover = player.GetComponent<GridMover>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player On Tile");
            playerOnTile = true;
            StartCoroutine(StartCooldown()); // Prevents immediate switching
            DetectAvailableShifts();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnTile = false;
            canShift = false; // Reset cooldown on exit
        }
    }

    void Update()
    {
        if (playerOnTile && targetTile && !isShifting && canShift)
        {
            if (Input.GetKeyDown(KeyCode.W)) ShiftPlayer(targetTile);
            if (Input.GetKeyDown(KeyCode.S)) ShiftPlayer(targetTile);
            if (Input.GetKeyDown(KeyCode.A)) ShiftPlayer(targetTile);
            if (Input.GetKeyDown(KeyCode.D)) ShiftPlayer(targetTile);
        }
    }

    void DetectAvailableShifts()
    {
        Collider[] nearbyTiles = Physics.OverlapBox(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
        
        foreach (Collider tile in nearbyTiles)
        {
            if (tile.CompareTag("PlaneSwitch") && tile.transform != transform)
            {
                targetTile = tile.transform;
                break;
            }
        }
    }

    void ShiftPlayer(Transform target)
    {
        if (!player) return;
        isShifting = true;
        canShift = false; // Prevent further inputs during shifting
        StartCoroutine(SmoothShift(target));
    }

    IEnumerator SmoothShift(Transform target)
    {
        Vector3 startPosition = player.position;
        Quaternion startRotation = player.rotation;
        Quaternion targetRotation = target.rotation; // Ensure player matches tile rotation
        
        float elapsedTime = 0f;
        
        while (elapsedTime < 1f)
        {
            player.position = Vector3.Lerp(startPosition, target.position, elapsedTime);
            player.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime);
            elapsedTime += Time.deltaTime * shiftSpeed;
            yield return null;
        }
        Debug.Log("Shifting finished");
        player.position = target.position;
        player.rotation = targetRotation;
        isShifting = false;
    }
    
    IEnumerator StartCooldown()
    {
        yield return new WaitForSeconds(switchCooldown);
        canShift = true;
    }
}

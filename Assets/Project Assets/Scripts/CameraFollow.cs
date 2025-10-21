using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 10, 0);
    [SerializeField] private float followSpeed;

    [Header("Dead Zone Settings")] [Range(0f, 0.5f)]
    [SerializeField] private float deadZoneX, deadZoneY, returnZonePadding;

    [Header("Zoom Settings")]
    [SerializeField] private Vector3 zoomedOffset = new Vector3(0, 5, 0);
    [SerializeField] private float zoomSpeed;
    
    private Camera cam;
    private bool isFollowingX;
    private bool isFollowingY;

    private Vector3 currentOffset;
    private Coroutine zoomRoutine;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!target)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform;
        }
        
        cam = Camera.main;
        currentOffset = offset;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!target || !cam) return;
        
        Vector3 targetPos = target.position + offset;
        Vector3 viewportPos = cam.WorldToViewportPoint(target.position); // Convert target position to viewport space (0-1 range)
        Vector3 desiredPos = transform.position;
        
        // --- X AXIS LOGIC ---
        if (!isFollowingX)
        {
            // Start following if target exits the main dead zone
            if (viewportPos.x < (0.5f - deadZoneX) || viewportPos.x > (0.5f + deadZoneX))
                isFollowingX = true;
        }
        else
        {
            // Stop following only if target is fully back inside the *return zone*
            if (viewportPos.x > (0.5f - deadZoneX + returnZonePadding) &&
                viewportPos.x < (0.5f + deadZoneX - returnZonePadding))
                isFollowingX = false;
        }

        // --- Y AXIS LOGIC ---
        if (!isFollowingY)
        {
            if (viewportPos.y < (0.5f - deadZoneY) || viewportPos.y > (0.5f + deadZoneY))
                isFollowingY = true;
        }
        else
        {
            if (viewportPos.y > (0.5f - deadZoneY + returnZonePadding) &&
                viewportPos.y < (0.5f + deadZoneY - returnZonePadding))
                isFollowingY = false;
        }

        // --- MOVE CAMERA ONLY ON ACTIVE AXES ---
        if (isFollowingX || isFollowingY)
            desiredPos = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        transform.position = desiredPos;
    }

    public void TriggerSelfDestructZoom(bool isActive)
    {
        Debug.Log("Triggered self-destruct zoom!");
        if (zoomRoutine != null)
            StopCoroutine(zoomRoutine);

        zoomRoutine = StartCoroutine(SmoothZoom(isActive ? zoomedOffset : offset));
    }
    
    private IEnumerator SmoothZoom(Vector3 targetOffset)
    {
        Vector3 startOffset = currentOffset;
        float t = 0f;
        
        while (t < 1f)
        {
            t += Time.deltaTime * zoomSpeed;
            currentOffset = Vector3.Lerp(startOffset, targetOffset, t);
            yield return null;
        }
        
        currentOffset = targetOffset;
    }
}

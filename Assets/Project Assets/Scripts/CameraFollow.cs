using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 10, 0);
    [SerializeField] private float followSpeed;

    [Header("Dead Zone Settings")]
    [Range(0f, 0.5f)] [SerializeField]
    [Tooltip("Range (in viewport%) before the camera starts following")]
    private float deadZoneX;
    [Range(0f, 0.5f)] [SerializeField]
    [Tooltip("Range (in viewport%) before the camera starts following")]
    private float deadZoneY;
    [Range(0f, 0.5f)] [SerializeField]
    [Tooltip("Extra padding inside the dead zone before the camera stops following")]
    private float returnZonePadding;

    [Header("Zoom Settings")] [SerializeField]
    private float normalFOV, zoomedFOV, zoomDuration;
    
    private Camera cam;
    private bool isFollowingX;
    private bool isFollowingY;
    private Coroutine zoomRoutine;

    private Vector3 originalOffset; // store original offset reference
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!target)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player) target = player.transform;
        }
        
        cam = Camera.main;
        cam.fieldOfView = normalFOV;
        originalOffset = offset;
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

    public void TriggerSelfDestructZoom(bool zoomIn)
    {
        if (zoomRoutine != null)
            StopCoroutine(zoomRoutine);

        zoomRoutine = StartCoroutine(ZoomCoroutine(zoomIn));
    }
    
    private IEnumerator ZoomCoroutine(bool zoomIn)
    {
        float startFOV = cam.fieldOfView;
        float endFOV = zoomIn ? zoomedFOV : normalFOV;
        float elapsed = 0f;

        Vector3 startPos = transform.position;
        Vector3 endPos;
        
        if (zoomIn)
        {
            // Move closer to player’s XZ, but maintain Y (height)
            endPos = new Vector3(target.position.x, startPos.y, target.position.z);
        }
        else
        {
            // Return to player + original offset
            endPos = target.position + originalOffset;
        }

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed/zoomDuration); // adds nice easing
            
            cam.fieldOfView = Mathf.Lerp(startFOV, endFOV, t);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        cam.fieldOfView = endFOV;
        transform.position = endPos;
        zoomRoutine = null;
    }

    public void ResetFOV(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(ResetFOVRoutine(duration));
    }

    private IEnumerator ResetFOVRoutine(float duration)
    {
        float startFOV = cam.fieldOfView;
        float time = 0f;
        
        Vector3 startPos = transform.position;
        Vector3 endPos = target.position + originalOffset;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            cam.fieldOfView = Mathf.Lerp(startFOV, normalFOV, t);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        cam.fieldOfView = normalFOV; // make sure it ends cleanly
        transform.position = endPos;
    }
}

using UnityEngine;

public class Player : MonoBehaviour
{
    private float moveSpeed = 5;
    private float rayDistance = 1f; // Detection range
    private float tileSize = 1f; // Match with singular tile world size
    private float returnSpeed = 5f; // Higher value snaps back faster
    private float maxEdgeOffset = 0.125f; // Make max edge offset
    
    private Vector3 velocity;
    private Vector3 lastSafePosition;
    private Vector3 edgeOffset;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastSafePosition = transform.position;
        edgeOffset = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        float moveHor = Input.GetAxis("Horizontal");
        float moveVer = Input.GetAxis("Vertical");
        
        //Use raw input for velocity and normalizes
        Vector3 input = new Vector3(moveHor, 0, moveVer) * moveSpeed;
        velocity = Vector3.ClampMagnitude(input, moveSpeed);
        
        Vector3 move = velocity * Time.deltaTime;
        
        // Test rotation
        if (Input.GetKey(KeyCode.E))
            transform.rotation = Quaternion.Euler(0, 0, -90);
        if (Input.GetKey(KeyCode.Q))
            transform.rotation = Quaternion.Euler(0, 0, 0);
        
        // Movement and edge bump
        if(velocity != Vector3.zero)
        {
            if (Physics.Raycast(transform.position, transform.TransformDirection(move.normalized),
                    out RaycastHit hit, rayDistance))
            {
                if (hit.collider.CompareTag("NormalTile"))
                {
                    transform.Translate(move);

                    lastSafePosition = new Vector3(Mathf.Round(transform.position.x / tileSize) * tileSize,
                        transform.position.y, Mathf.Round(transform.position.z / tileSize) * tileSize);
                    edgeOffset = Vector3.zero;
                }
            }
            else
            {
                edgeOffset += move;
                if (edgeOffset.magnitude > maxEdgeOffset)
                    edgeOffset = edgeOffset.normalized * maxEdgeOffset;
            
                Vector3 targetPosition = lastSafePosition + edgeOffset;
                transform.position = Vector3.Lerp(transform.position, targetPosition, returnSpeed * Time.deltaTime);
            }
        }
        else
        {
            // No input -> lerp back to tile center
            edgeOffset = Vector3.Lerp(edgeOffset, Vector3.zero, returnSpeed * Time.deltaTime);
            
            Vector3 targetPosition = lastSafePosition + edgeOffset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, returnSpeed * 2f * Time.deltaTime);
        }
            
        Debug.DrawRay(transform.position, transform.TransformDirection(velocity/moveSpeed), Color.red);
    }
}

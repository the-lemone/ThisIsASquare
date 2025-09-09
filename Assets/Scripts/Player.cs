using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rayDistance = 1f; // Detection range
    [SerializeField] private float returnSpeed = 5f; // Higher value snaps back faster
    [SerializeField] private float tileSize = 1f; // Match with singular tile world size
    [SerializeField] private float maxEdgeOffset = 0.125f; // Make max edge offset

    private Vector3 _velocity;
    private Vector3 _lastSafePosition;
    private Vector3 _edgeOffset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _lastSafePosition = transform.position;
        _edgeOffset = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        float moveHor = Input.GetAxis("Horizontal");
        float moveVer = Input.GetAxis("Vertical");
        
        bool canMoveX = true;
        bool canMoveZ = true;

        //Use raw input for velocity and normalizes
        Vector3 input = new Vector3(moveHor, 0, moveVer) * moveSpeed;
        _velocity = Vector3.ClampMagnitude(input, moveSpeed);

        Vector3 move = _velocity * Time.deltaTime;
        
        // Movement and edge bump
        if (_velocity != Vector3.zero)
        {
            // Raycast cardinal directions to see if void exists
            if (Mathf.Abs(move.x) > 0.0001f)
            {
                Vector3 dirX = new Vector3(Mathf.Sign(move.x), 0, 0); // cardinal X direction
                canMoveX = Physics.Raycast(transform.position, transform.TransformDirection(dirX), out RaycastHit hitX, rayDistance)
                           && hitX.collider.CompareTag("NormalTile");
            }
            if (Mathf.Abs(move.z) > 0.0001f)
            {
                Vector3 dirZ = new Vector3(0, 0, Mathf.Sign(move.z)); // cardinal Z direction
                canMoveZ = Physics.Raycast(transform.position, transform.TransformDirection(dirZ), out RaycastHit hitZ, rayDistance)
                           && hitZ.collider.CompareTag("NormalTile");
            }
            
            Vector3 moveToApply = Vector3.zero;

            if (canMoveX) moveToApply.x = move.x;
            if (canMoveZ) moveToApply.z = move.z;

            transform.Translate(moveToApply);
            
            if (moveToApply != Vector3.zero)
            {
                // Set last safe position to tile center
                _lastSafePosition = new Vector3(
                    Mathf.Round(transform.position.x / tileSize) * tileSize,
                    transform.position.y, Mathf.Round(transform.position.z / tileSize) * tileSize
                );
                _edgeOffset = Vector3.zero;
            }
            else
            {
                _edgeOffset += move;
                
                // Void detected → pushback offset
                if (_edgeOffset.magnitude > maxEdgeOffset)
                    _edgeOffset = _edgeOffset.normalized * maxEdgeOffset;
                
                transform.position = Vector3.Lerp(transform.position, _lastSafePosition + _edgeOffset, returnSpeed * Time.deltaTime);
            }
        }
    }
}
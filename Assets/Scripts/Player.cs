using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private LayerMask tileLayer; // Layer for NormalTile

    private Rigidbody _rb;
    private Vector3 _moveInput;
    private Vector3 _velocity;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        _moveInput.x = Input.GetAxis("Horizontal");
        _moveInput.z = Input.GetAxis("Vertical");

        Vector3 input = new Vector3(_moveInput.x, 0, _moveInput.z) * moveSpeed;
        _velocity = Vector3.ClampMagnitude(input, moveSpeed);
    }

    void FixedUpdate()
    {
        if (_velocity != Vector3.zero)
            MovePlayer(_velocity);
    }

    private void MovePlayer(Vector3 direction)
    {
        Vector3 move = direction * (moveSpeed * Time.fixedDeltaTime); // grid step per input

        // Test X axis
        if (Mathf.Abs(move.x) > 0.01f)
        {
            if (!TileExistsAtOffset(new Vector3(Mathf.Sign(move.x), 0, 0)))
                move.x = 0;
        }

        // Test Z axis
        if (Mathf.Abs(move.z) > 0.01f)
        {
            if (!TileExistsAtOffset(new Vector3(0, 0, Mathf.Sign(move.z))))
                move.z = 0;
        }

        // Apply movement
        Vector3 targetPos = _rb.position + move;
        _rb.MovePosition(Vector3.Lerp(_rb.position, targetPos, moveSpeed * Time.fixedDeltaTime));
    }

    private bool TileExistsAtOffset(Vector3 offset)
    {
        // Check for tile collider in the target cell
        float offsetMultiplier = 0.75f; // Set to size of player
        Vector3 checkPos = transform.position + offset * tileSize;
        Vector3 halfExtents = transform.localScale * offsetMultiplier;
        Collider[] hits = Physics.OverlapBox(checkPos, halfExtents, Quaternion.identity, tileLayer);
        return hits.Length > 0;
    }
}
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Move/Tile Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private LayerMask tileLayer; // Layer for NormalTile

    [Header("Self-Destruct Settings")]
    [SerializeField] private float twitchDuration;
    [SerializeField] private float twitchSpeed;
    [SerializeField] private float twitchIntensity;
    
    private Rigidbody _rb;
    private Vector3 _moveInput;
    private Vector3 _velocity;

    private Coroutine _selfDestructRoutine;
    private Vector3 _originalPosition;
    private bool _onTile = true;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        bool tileBelow = TileExistsBelow();
        
        // Transition from on-tile → off-tile
        if (!tileBelow && _onTile)
        {
            _onTile = false;
            if (_selfDestructRoutine == null)
                _selfDestructRoutine = StartCoroutine(SelfDestructSequence());
        }
        // Transition from off-tile → on-tile
        else if (tileBelow && !_onTile)
        {
            _onTile = true;
            if (_selfDestructRoutine != null)
            {
                StopCoroutine(_selfDestructRoutine);
                _selfDestructRoutine = null;
                Debug.Log("Self-destruct aborted!");
            }
        }

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
    
    /*
    // Input System callback
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }
    */

    private void MovePlayer(Vector3 direction)
    {
        Vector3 move = direction * Time.fixedDeltaTime; // grid step per input

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

    private bool TileExistsBelow()
    {
        // Start slightly above the player’s feet, so the ray always starts clear of the tile surface
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        // Cast a short ray straight down to check for tile
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1f, tileLayer))
        {
            // Optional: make sure the tile matches the player’s rotation
            if (Quaternion.Angle(hit.transform.rotation, transform.rotation) < 1f)
                return true;
        }
        
        return false;
    }
    
    private bool TileExistsAtOffset(Vector3 offset)
    {
        // Check for tile collider in the target cell
        float offsetMultiplier = 0.75f; // Set to size of player
        Vector3 checkPos = transform.position + offset * tileSize;
        Vector3 halfExtents = transform.localScale * offsetMultiplier;
        Collider[] hits = Physics.OverlapBox(checkPos, halfExtents, Quaternion.identity, tileLayer);
        hits = hits.Where(hit => Quaternion.Angle(hit.transform.rotation, transform.rotation) < 1f).ToArray();
        return hits.Length > 0;
    }
    
    private IEnumerator SelfDestructSequence()
    {
        _originalPosition = transform.position;
        float duration = twitchDuration;
        float timer = 0f;
        float twitchTimer = 0f;

        Debug.Log("Self-destruct initiated!");

        while (timer < duration && !_onTile)
        {
            timer += Time.deltaTime;
            twitchTimer += Time.deltaTime;

            // Ramp up intensity (0 to 1 over duration)
            float intensityMultiplier = Mathf.Clamp01(timer / duration);
            float currentIntensity = twitchIntensity * intensityMultiplier;
            
            if(twitchTimer >= 1f / twitchSpeed)
            {
                twitchTimer = 0f;
                
                // Small twitch movement along XZ plane
                Vector3 twitchOffset = new Vector3(
                    Random.Range(-currentIntensity, currentIntensity),
                    0,
                    Random.Range(-currentIntensity, currentIntensity)
                );
                // Move to offset position
                transform.position = _originalPosition + twitchOffset;
            }
            else
            {
                // Snap back to center between twitches
                transform.position = Vector3.Lerp(transform.position, _originalPosition, Time.deltaTime * twitchSpeed);
            }
            
            yield return null;
        }

        if (!_onTile)
        {
            Debug.Log("Player lost! Self-destruct complete.");
            // Add lose sequence or respawn logic here
        }
        
        _selfDestructRoutine = null;
    }
}
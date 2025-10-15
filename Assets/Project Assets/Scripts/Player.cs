using System.Linq;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour 
{ 
    [Header("Move/Tile Settings")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private LayerMask tileLayer; // Layer for NormalTile
    
    [Header("Self-Destruct Settings")]
    [SerializeField] private float twitchDuration;
    [SerializeField] private float twitchSpeed;
    [SerializeField] private float twitchIntensity;
    
    private Rigidbody _rb;
    private Vector3 _moveInput;
    private Vector3 _velocity;
    private PlayerControls _controls;
    
    private Coroutine _selfDestructRoutine;
    private Vector3 _originalPosition;
    private bool _onTile = true;
    void Awake()
    { 
        _controls = new PlayerControls(); // Subscribe to Move input
        _controls.Player.Move.performed += ctx => { Vector2 input = ctx.ReadValue<Vector2>();
            _moveInput = new Vector3(input.x, 0f, input.y); };
        _controls.Player.Move.canceled += _ => _moveInput = Vector3.zero;
        
    }
    void OnEnable() => _controls.Enable();
    void OnDisable() => _controls.Disable();
    
    void Start() { _rb = GetComponent<Rigidbody>(); } 
    void Update()
    { 
        bool tileBelow = TileExistsBelow(); // Transition from on-tile → off-tile
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
    }

    void FixedUpdate()
    {
        Vector3 desiredVelocity = new Vector3(_moveInput.x, 0, _moveInput.z) * moveSpeed;
        float accel = (_moveInput.sqrMagnitude > 0.01f) ? acceleration : deceleration;
        Vector3 targetVel = (_moveInput.sqrMagnitude > 0.01f) ? desiredVelocity : Vector3.zero;
        
        _velocity = Vector3.MoveTowards(_velocity, targetVel, accel * Time.fixedDeltaTime);
        
        // Predictive edge check
        if (Mathf.Abs(_velocity.x) > 0.01f && !TileExistsAtOffset(new Vector3(Mathf.Sign(_velocity.x), 0, 0)))
            _velocity.x = 0;

        if (Mathf.Abs(_velocity.z) > 0.01f && !TileExistsAtOffset(new Vector3(0, 0, Mathf.Sign(_velocity.z))))
            _velocity.z = 0;
        
        // Apply movement
        _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);
    }
    
    
    private bool TileExistsBelow() 
    {
        // Start slightly above the player’s feet, so the ray always starts clear of the tile surface
        Vector3 origin = transform.position + Vector3.up * 0.1f; // Cast a short ray straight down to check for tile
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1f, tileLayer))
        {
            return true;
        }
        return false;
    }
    
    private bool TileExistsAtOffset(Vector3 offset) 
    {
        // Check for tile collider in the target cell
        float offsetMultiplier = 0.6f; // Set to size of player
        Vector3 checkPos = transform.position + offset * tileSize;
        Vector3 halfExtents = transform.localScale * offsetMultiplier;
        Collider[] hits = Physics.OverlapBox(checkPos, halfExtents, Quaternion.identity, tileLayer);
            hits = hits.Where(hit =>
            {
                float angle = Vector3.Angle(hit.transform.up, hit.transform.up);
                return angle < 1f;
            }).ToArray();
        return hits.Length > 0;
    }
    
    private IEnumerator SelfDestructSequence() 
    {
        _originalPosition = transform.position; float duration = twitchDuration;
        float timer = 0f;
        float twitchTimer = 0f; 
        Debug.Log("Self-destruct initiated!");
        
        while (timer < duration && !_onTile) 
        { 
            timer += Time.deltaTime; twitchTimer += Time.deltaTime; // Ramp up intensity (0 to 1 over duration)
            float intensityMultiplier = Mathf.Clamp01(timer / duration); 
            float currentIntensity = twitchIntensity * intensityMultiplier;
            if(twitchTimer >= 1f / twitchSpeed)
            {
                twitchTimer = 0f; // Small twitch movement along XZ plane
                Vector3 twitchOffset = new Vector3(
                    Random.Range(-currentIntensity, currentIntensity),
                    0,
                    Random.Range(-currentIntensity, currentIntensity) ); // Move to offset position
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
            // TODO: Add lose sequence or respawn logic here
        }
        _selfDestructRoutine = null;
    }
}
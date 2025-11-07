using System.Linq;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour 
{ 
    [Header("Move Settings")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float bumpStrength;
    
    [Header("Self-Destruct Settings")]
    [SerializeField] private float twitchDuration;
    [SerializeField] private float twitchSpeed;
    [SerializeField] private float twitchIntensity;
    
    [Header("Layers")]
    [SerializeField] private LayerMask tileLayer; // Layer for NormalTile
    [SerializeField] private LayerMask wallLayer;
    
    private float tileSize = 1f;
    
    private Rigidbody _rb;
    private Vector3 _moveInput;
    private Vector3 _velocity;
    public PlayerControls _controls;
    
    private Coroutine _selfDestructRoutine;
    private Vector3 _originalPosition;
    private bool _onTile = true;
    
    private CameraFollow _cameraFollow;
    void Awake()
    { 
        _controls = new PlayerControls(); // Subscribe to Move input
        _controls.Player.Move.performed += ctx => { Vector2 input = ctx.ReadValue<Vector2>();
            _moveInput = new Vector3(input.x, 0f, input.y); };
        _controls.Player.Move.canceled += _ => _moveInput = Vector3.zero;
    }
    void OnEnable() => _controls.Enable();
    void OnDisable() => _controls.Disable();

    void Start()
    {
        _cameraFollow = FindFirstObjectByType<CameraFollow>();
        _rb = GetComponent<Rigidbody>();
    } 
    void Update()
    { 
        bool tileBelow = TileExistsBelow(); // Transition from on-tile → off-tile
        if (!tileBelow && _onTile) 
        {
            _onTile = false;
            if (_selfDestructRoutine == null)
            {
                _selfDestructRoutine = StartCoroutine(SelfDestructSequence());
            }
        } 
        // Transition from off-tile → on-tile
        else if (tileBelow && !_onTile)
        {
            _onTile = true;
            if (_selfDestructRoutine != null)
            {
                StopCoroutine(_selfDestructRoutine);
                _selfDestructRoutine = null;
                _cameraFollow.TriggerSelfDestructZoom(false); // Zoom out
                Debug.Log("Self-destruct aborted!");
            }
        }

        if (transform.position.y != 0f)
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
    }

    void FixedUpdate()
    {
        Vector3 desiredVelocity = new Vector3(_moveInput.x, 0, _moveInput.z) * moveSpeed;
        float accel = (_moveInput.sqrMagnitude > 0.01f) ? acceleration : deceleration;
        Vector3 targetVel = (_moveInput.sqrMagnitude > 0.01f) ? desiredVelocity : Vector3.zero;
        
        _velocity = Vector3.MoveTowards(_velocity, targetVel, accel * Time.fixedDeltaTime);
        
        // Wall
        if (IsTouchingWall(out Vector3 wallNormal))
        {
            // Push the player slightly opposite to the wall normal
            transform.position += wallNormal * (bumpStrength * Time.fixedDeltaTime);

            _velocity -= Vector3.Project(_velocity, -wallNormal);
        }
        
        // Apply movement
        _rb.MovePosition(_rb.position + _velocity * Time.fixedDeltaTime);
    }

    private bool IsTouchingWall(out Vector3 wallNormal)
    {
        wallNormal = Vector3.zero;
        float radius = 0.4f * tileSize;
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, wallLayer);

        if (hits.Length > 0)
        {
            // Find average normal from all hit walls
            Vector3 sumNormals = Vector3.zero;
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out Collider col))
                {
                    // Get closest point normal
                    Vector3 dirToWall = (transform.position - col.ClosestPoint(transform.position));
                    sumNormals += dirToWall;
                }
            }
            
            wallNormal = sumNormals.normalized;
            return true;
        }
        return false;
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
    
    private IEnumerator SelfDestructSequence() 
    {
        _originalPosition = transform.position; float duration = twitchDuration;
        float timer = 0f;
        float twitchTimer = 0f;

        _cameraFollow.TriggerSelfDestructZoom(true); // Zoom in
        
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
            Kill();
        }
        _selfDestructRoutine = null;
    }

    void Kill()
    {
        _cameraFollow.ResetFOV(0.5f);
        gameObject.SetActive(false);
    }
}
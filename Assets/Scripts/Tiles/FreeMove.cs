using UnityEngine;

public class FreeMove : GridBase
{
    private Vector3 _inputDirection;
    private Transform _closestTile;
    private Transform _tileInDirection;
    private Transform _lastTile;
    private bool _hasBumpedThisPress;
    
    private Vector3 _currentVelocity = Vector3.zero;
    private Vector3 _snapVelocity = Vector3.zero;
    public float acceleration = 10f;   // How quickly to ramp up to target speed
    public float deceleration = 10f;   // How quickly to slow down to zero
    
    protected override void Start()
    {
        base.Start();
        pitch = 1f;
    }
    
    protected override void Update()
    {
        base.Update();
        
        HandleInput();
        UpdateVelocity();
        MoveFreely();
        
        if (isMoving)
        {
            SmoothSnapToAxis(_inputDirection);
        }
        else
        {
            SnapToNearestTile();
        }
    }

    private void HandleInput()
    {
        Vector3 inputDirection = Vector3.zero;
        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputDirection = player.forward; // Move forward in 3D
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputDirection = -player.forward; // Move backward
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputDirection = -player.right; // Move left
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputDirection = player.right; // Move right

        _inputDirection = inputDirection.normalized;

        if (_inputDirection != Vector3.zero)
        {
            isMoving = true;
            isMovingAnim = true;
        }
        else if (_inputDirection == Vector3.zero)
        {
            isMoving = false;
            isMovingAnim = false;
        }
        
        _tileInDirection = FindTileInDirection(_inputDirection);
        _closestTile = FindClosestTile(transform.position);
    }
    
    void MoveFreely()
    {
        if (_inputDirection == Vector3.zero) return;
        
        // Keep within bounds of closest tile grid

        Transform closestTile = _closestTile;
        
        if (!closestTile) return;

        currentTile = closestTile;
        
        if (_lastTile != closestTile)
        {
            if (SoundFXManager.instance) 
                SoundFXManager.instance.PlaySoundMove(moveSound, transform, 1f, pitch);
            _lastTile = closestTile;
        }

        var newPosition = transform.position + _currentVelocity * Time.deltaTime;
        var tileCenter = closestTile.position;

        // Local offset from tile center
        var localOffset = closestTile.InverseTransformDirection(newPosition - tileCenter);

        // Determine the direction of movement in local space
        var localInputDir = closestTile.InverseTransformDirection(_inputDirection);

        // Block movement **past** the edge of the tile if there's no tile in that direction
        if (!_tileInDirection)
        {
            if ((localInputDir.x > 0 && localOffset.x > 0.1f) ||
                (localInputDir.x < 0 && localOffset.x < -0.1f) ||
                (localInputDir.z > 0 && localOffset.z > 0.1f) ||
                (localInputDir.z < 0 && localOffset.z < -0.1f))
            {
                if (!_hasBumpedThisPress && !PlaneSwitchTile.isShifting)
                {
                    if (SoundFXManager.instance)
                        SoundFXManager.instance.PlaySoundMove(bumpSound, transform, 1f, pitch);
                    _hasBumpedThisPress = true;
                }
                return;
            }
        }
        _hasBumpedThisPress = false;

        
        // Clamp for bounds regardless
        float maxOffset = 0.6f;
        localOffset.x = Mathf.Clamp(localOffset.x, -maxOffset, maxOffset);
        localOffset.y = Mathf.Clamp(localOffset.y, -maxOffset, maxOffset);
        localOffset.z = Mathf.Clamp(localOffset.z, -maxOffset, maxOffset);
        
        transform.position = tileCenter + closestTile.TransformDirection(localOffset);
    }
    
    private void UpdateVelocity()
    {
        if (_inputDirection != Vector3.zero)
        {
            // Accelerate toward desired velocity
            Vector3 targetVelocity = _inputDirection * moveSpeed;
            _currentVelocity = Vector3.Lerp(_currentVelocity, targetVelocity, Time.deltaTime * acceleration);
        }
        else
        {
            // Decelerate to zero
            _currentVelocity = Vector3.Lerp(_currentVelocity, Vector3.zero, Time.deltaTime * deceleration);
        }
    }

    void SnapToNearestTile()
    {
        Transform closestTile = FindClosestTile(transform.position);
        if (closestTile)
        {
            var tileCenter = closestTile.position;
            const float snapTime = 0.1f; // Lower = snappier

            player.position = Vector3.SmoothDamp(
                player.position,
                tileCenter,
                ref _snapVelocity,
                snapTime,
                moveSpeed * 5f // max speed
            );

            if (Vector3.Distance(player.position, tileCenter) < 0.01f)
            {
                player.position = tileCenter;
                _currentVelocity = Vector3.zero;
                _snapVelocity = Vector3.zero;
                isMoving = false;
                MatchRotationToTile();
            }
        }
    }
    
    private void SmoothSnapToAxis(Vector3 direction)
    {
        if (!_closestTile) return;

        Vector3 currentPos = transform.position;
        Vector3 targetPos = currentPos;
        Vector3 tileCenter = _closestTile.position;

        // Lerp speed based on current movement speed (makes snapping more responsive at higher speeds)
        float lerpSpeed = moveSpeed * 4f + _currentVelocity.magnitude * 2f;

        if (Mathf.Abs(direction.x) > 0)
        {
            // Moving left/right: snap Z toward tile center
            targetPos.z = Mathf.Lerp(currentPos.z, tileCenter.z, Time.deltaTime * lerpSpeed);
        }
        else if (Mathf.Abs(direction.z) > 0)
        {
            // Moving forward/backward: snap X toward tile center
            targetPos.x = Mathf.Lerp(currentPos.x, tileCenter.x, Time.deltaTime * lerpSpeed);
        }

        transform.position = targetPos;
    }
}

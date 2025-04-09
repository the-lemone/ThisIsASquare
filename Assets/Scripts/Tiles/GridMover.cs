using System.Collections.Generic;
using UnityEngine;

public class GridMover : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    public Transform player;
    public float moveSpeed = 5f; // Used in MoveToTarget
    public List<Transform> tiles; // List of tile positions
    private Transform _currentTile; // Whatever tile the player is on right now
    private bool _isMovingAnim;
    private bool _isMoving;
    private float _cooldown;
    private int _timesMoved;
    
    private Animator _anim;
    [SerializeField] private AudioClip moveSound;
    private float _pitch;

    void Start()
    {
        _anim = GetComponent<Animator>();
        GetAllTiles();
        
        if (tiles.Count > 0)
        {
            _currentTile = FindClosestTile(player.position);
            player.position = _currentTile.position;
        }
    }

    void Update()
    {
        _anim.SetBool(IsMoving, _isMovingAnim);
        if(!_isMoving)
            HandleInput();
        else if (_isMoving)
            MoveToTarget();

        if (_cooldown > 0)
            _cooldown -= Time.deltaTime;

        if (_cooldown <= 0)
            _cooldown = 0;
    }
    
    void HandleInput()
    {
        Vector3 inputDirection = Vector3.zero;
        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputDirection = player.forward; // Move forward in 3D
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputDirection = -player.forward; // Move backward
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputDirection = -player.right; // Move left
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputDirection = player.right; // Move right

        if (inputDirection == Vector3.zero)
        {
            _isMovingAnim = false;
            _timesMoved = 0;
        }
        
        if (inputDirection != Vector3.zero)
        {
            _isMovingAnim = true;
            Transform targetTile = FindTileInDirection(inputDirection);
            if (targetTile && _cooldown == 0)
            {
                _currentTile = targetTile;
                _isMoving = true;
                _timesMoved++;
                float cooldown = 0.5f - (_timesMoved * 0.1f);
                float limit = Mathf.Clamp(cooldown, 0.01f, 0.5f);
                _cooldown = limit;
                _pitch = 1f + _timesMoved * 0.05f;
                SoundFXManager.instance.PlaySoundMove(moveSound, transform, 1f, _pitch);
            }
        }
    }
    
    void MoveToTarget()
    {
        float distance = Vector3.Distance(player.position, _currentTile.position);
        
        // Exponential movement factor
        float t = 1f - Mathf.Exp(-moveSpeed * Time.deltaTime);
        
        // Smoothly interpolate player's position towards target
        player.position = Vector3.Lerp(player.position, _currentTile.position, t);
        if (distance < 0.01f)
        {
            player.position = _currentTile.position;
            _isMoving = false;
            MatchRotationToTile();
        }
    }

    Transform FindTileInDirection(Vector3 direction)
    {
        float minDistance = Mathf.Infinity;
        Transform closestTile = null;

        foreach (Transform tile in tiles)
        {
            Vector3 tileOffset = (tile.position - _currentTile.position).normalized;
            float distance = Vector3.Distance(_currentTile.position + direction.normalized, tile.position);

            if (Vector3.Dot(tileOffset, direction.normalized) > 0.9f && distance <= 0.8f) // Ensure movement aligns with tile direction
            {
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTile = tile;
                }
            }
        }
        return closestTile;
    }

    Transform FindClosestTile(Vector3 position)
    {
        Transform closestTile = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform tile in tiles)
        {
            float distance = Vector3.Distance(position, tile.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTile = tile;
            }
        }

        return closestTile;
    }
    
    void GetAllTiles()
    {
        tiles.Clear();
        GameObject[] tileObjects = GameObject.FindGameObjectsWithTag("NormalTile");
        foreach (GameObject tile in tileObjects)
        {
            tiles.Add(tile.transform);
        }
        
        GameObject[] switchTiles = GameObject.FindGameObjectsWithTag("SwitchTile");
        foreach (GameObject tile in switchTiles)
        {
            tiles.Add(tile.transform);
        }
    }
    
    public void MatchRotationToTile()
    {
        if (_currentTile != null)
        {
            transform.rotation = _currentTile.rotation; // Match the player's rotation to the tile
        }
    }
}

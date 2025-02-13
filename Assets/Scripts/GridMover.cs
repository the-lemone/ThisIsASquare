using System.Collections.Generic;
using UnityEngine;

public class GridMover : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 5f; // Used in MoveToTarget
    public List<Transform> tiles; // List of tile positions
    private Transform _currentTile; // Whatever tile the player is on right now
    private bool _isMoving;

    void Start()
    {
        GetAllTiles();
        
        if (tiles.Count > 0)
        {
            _currentTile = FindClosestTile(player.position);
            player.position = _currentTile.position;
        }
    }

    void Update()
    {
        if (!_isMoving)
        {
            HandleInput();
        }
        else
        {
            MoveToTarget();
        }
    }
    
    void HandleInput()
    {
        Vector3 inputDirection = Vector3.zero;
        
        if (Input.GetKeyDown(KeyCode.W)) inputDirection = player.forward; // Move forward in 3D
        if (Input.GetKeyDown(KeyCode.S)) inputDirection = -player.forward; // Move backward
        if (Input.GetKeyDown(KeyCode.A)) inputDirection = -player.right; // Move left
        if (Input.GetKeyDown(KeyCode.D)) inputDirection = player.right; // Move right
        
        if (inputDirection != Vector3.zero)
        {
            Transform targetTile = FindTileInDirection(inputDirection);
            if (targetTile != null)
            {
                _currentTile = targetTile;
                _isMoving = true;
            }
        }
    }
    
    void MoveToTarget()
    {
        player.position = Vector3.MoveTowards(player.position, _currentTile.position, moveSpeed * Time.deltaTime);
        if (Vector3.Distance(player.position, _currentTile.position) < 0.01f)
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

            if (Vector3.Dot(tileOffset, direction.normalized) > 0.9f && distance <= 1f) // Ensure movement aligns with tile direction
            {
                //float distance = Vector3.Distance(_currentTile.position + direction.normalized, tile.position);
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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GridBase : MonoBehaviour
{
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    public Transform player;
    [Header("SFX")]
    [SerializeField] public AudioClip moveSound;
    [SerializeField] public AudioClip bumpSound;
    [Header("Movement")]
    public float moveSpeed = 5f; // Used in MoveToTarget
    
    private Animator _anim;
    
    [HideInInspector]
    public float pitch;
    [HideInInspector]
    public List<Transform> tiles; // List of tile positions
    [HideInInspector]
    public Transform currentTile; // Whatever tile the player is on right now
    [HideInInspector]
    public bool isMovingAnim;
    [HideInInspector]
    public bool isMoving;

    protected virtual void Start()
    {
        _anim = GetComponent<Animator>();
        GetAllTiles();
        
        if (tiles.Count > 0)
        {
            currentTile = FindClosestTile(player.position);
            player.position = currentTile.position;
        }
    }

    protected virtual void Update()
    {
        _anim.SetBool(IsMoving, isMovingAnim);
    }

    public Transform FindTileInDirection(Vector3 direction)
    {
        float minDistance = Mathf.Infinity;
        Transform closestTile = null;

        foreach (Transform tile in tiles)
        {
            Vector3 tileOffset = (tile.position - currentTile.position).normalized;
            float distance = Vector3.Distance(currentTile.position + direction.normalized, tile.position);

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

    public Transform FindClosestTile(Vector3 position)
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

    private void GetAllTiles()
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

    protected void MatchRotationToTile()
    {
        if (currentTile != null)
        {
            transform.rotation = currentTile.rotation; // Match the player's rotation to the tile
        }
    }
}

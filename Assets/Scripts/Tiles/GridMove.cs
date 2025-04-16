using System.Collections;
using UnityEngine;

public class GridMove : GridBase
{
    private float _cooldown;
    private int _timesMoved;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        pitch = 1f;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if(!isMoving)
            HandleInput();
        else if (isMoving)
            MoveToTarget();

        if (_cooldown > 0)
            _cooldown -= Time.deltaTime;

        if (_cooldown <= 0)
            _cooldown = 0;
    }
    
    private void HandleInput()
    {
        Vector3 inputDirection = Vector3.zero;
        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) inputDirection = player.forward; // Move forward in 3D
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) inputDirection = -player.forward; // Move backward
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) inputDirection = -player.right; // Move left
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) inputDirection = player.right; // Move right

        if (inputDirection == Vector3.zero)
        {
            isMovingAnim = false;
        }
        
        if (inputDirection != Vector3.zero)
        {
            isMovingAnim = true;
            Transform targetTile = FindTileInDirection(inputDirection);
            if (targetTile && _cooldown == 0)
            {
                currentTile = targetTile;
                isMoving = true;
                _cooldown = 0.25f;
                if (SoundFXManager.instance)
                    SoundFXManager.instance.PlaySoundMove(moveSound, transform, 1f, pitch);
            }
            else if (!targetTile && _cooldown == 0)
            {
                isMoving = true;
                _cooldown = 0.25f;
                
                Vector3 bumpOffset = inputDirection.normalized * 0.2f;
                Vector3 originalPosition = currentTile.position;

                // Move player slightly in the intended direction
                player.position = originalPosition + bumpOffset;
                if (SoundFXManager.instance)
                    SoundFXManager.instance.PlaySoundMove(bumpSound, transform, 1f, pitch);

                // Immediately snap back to current tile
                StartCoroutine(BumpBack(originalPosition));
            }
        }
    }
    
    private void MoveToTarget()
    {
        float distance = Vector3.Distance(player.position, currentTile.position);
        
        // Exponential movement factor
        float t = 1f - Mathf.Exp(-moveSpeed * Time.deltaTime);
        
        // Smoothly interpolate player's position towards target
        player.position = Vector3.Lerp(player.position, currentTile.position, t);
        if (distance < 0.01f)
        {
            player.position = currentTile.position;
            isMoving = false;
            MatchRotationToTile();
        }
    }
    
    private IEnumerator BumpBack(Vector3 originalPosition)
    {
        yield return new WaitForSeconds(0.1f); // Small delay to make the bump noticeable
        player.position = originalPosition;
        isMoving = false;
    }
}

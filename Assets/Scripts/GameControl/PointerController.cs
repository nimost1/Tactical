using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(PlayerInput))]
public class PointerController : MonoBehaviour
{
    
    
    [SerializeField] private SpriteRenderer _pointerRenderer;

    [SerializeField] private TileBase _movableOverlayTile;
    [SerializeField] private TileBase _attackableOverlayTile;

    private Vector2Int _pointerPosition;

    public void SetPointerPosition(Vector2Int pos)
    {
        _pointerPosition = pos;
    }

    public Vector2Int GetPointerPosition()
    {
        return _pointerPosition;
    }

    public void ShowPointer()
    {
        _pointerRenderer.enabled = true;
    }

    public void HidePointer()
    {
        _pointerRenderer.enabled = false;
    }

    private void ShowMovementAndAttackUI(Vector2Int pos, int movementRange, int attackRange, Tilemap overlayTilemap)
    {
        var movableTiles = GridController.GetMovableTilesInRange(pos, movementRange, false);
        
        ShowValidMovementSpaces(movableTiles, overlayTilemap);

        var attackableTiles = GridController.GetNeighbors(movableTiles, attackRange);
        
        ShowAttackableSpaces(attackableTiles, overlayTilemap);
    }

    private void ShowValidMovementSpaces(List<Vector2Int> positions, Tilemap overlayTilemap)
    {
        foreach (Vector3Int tile in positions)
        {
            overlayTilemap.SetTile(tile, _movableOverlayTile);
        }
    }

    private void ShowAttackableSpaces(List<Vector2Int> positions, Tilemap overlayTilemap)
    {
        foreach (Vector3Int tile in positions)
        {
            overlayTilemap.SetTile(tile, _attackableOverlayTile);
        }
    }

    private void ClearOverlayTilemap(Tilemap overlayTilemap)
    {
        overlayTilemap.ClearAllTiles();
    }

    public IEnumerator SelectPositionWithPointer(Vector2Int pos, int movementRange, int attackRange, Tilemap overlayTilemap)
    {
        ShowMovementAndAttackUI(pos, movementRange, attackRange, overlayTilemap);
        ShowPointer();
        while (true)
        {
            while (GameController.CurrentGameController.isPaused)
            {
                yield return null;
            }
            
            //Get input
            if (GameController.Input.SelectPressed)
            {
                break;
            }
            if (Keyboard.current.wKey.wasPressedThisFrame && _pointerPosition.y != GameController.CurrentGameController.upperBorder - 1)
            {
                _pointerPosition.y += 1;
            }else if (Keyboard.current.sKey.wasPressedThisFrame && _pointerPosition.y != 0)
            {
                _pointerPosition.y -= 1;
            }else if (Keyboard.current.dKey.wasPressedThisFrame && _pointerPosition.x != GameController.CurrentGameController.easternBorder - 1)
            {
                _pointerPosition.x += 1;
            }else if (Keyboard.current.aKey.wasPressedThisFrame && _pointerPosition.x != 0)
            {
                _pointerPosition.x -= 1;
            }

            _pointerRenderer.transform.position = GridController.GridCoordinatesToWorldCoordinates(_pointerPosition);
            
            yield return null;
        }
        
        HidePointer();
        ClearOverlayTilemap(overlayTilemap);
    }
}

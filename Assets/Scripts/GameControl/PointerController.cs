using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(PlayerInput))]
public class PointerController : MonoBehaviour
{
    //TODO: Record the path to take.
    
    [SerializeField] private SpriteRenderer _pointerRenderer;

    [SerializeField] private TileBase _movableOverlayTile;
    [SerializeField] private TileBase _attackableOverlayTile;

    private Vector2Int _pointerPosition;
    public int targetIndex;

    public void SetPointerPosition(Vector2Int pos)
    {
        _pointerPosition = pos;
        _pointerRenderer.transform.position = GridController.GridCoordinatesToWorldCoordinates(_pointerPosition);
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

    private void ShowMovementAndAttackUI(List<Vector2Int> movableTiles, List<Vector2Int> attackableTiles, Tilemap overlayTilemap)
    {
        ShowValidMovementSpaces(movableTiles, overlayTilemap);
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
        var movableTiles = GridController.GetMovableTilesInRange(pos, movementRange);
        var attackableTiles = GridController.GetAttackableTiles(pos, movementRange, attackRange);
        attackableTiles = attackableTiles.Except(movableTiles).ToList();
        
        ShowMovementAndAttackUI(movableTiles, attackableTiles, overlayTilemap);
        ShowPointer();
        while (true)
        {
            if (GameController.CurrentGameController.Input.BackPressed)
            {
                SetPointerPosition(pos);
            }
            
            while (GameController.CurrentGameController.isPaused)
            {
                yield return null;
            }
            
            //Get input
            if (GameController.CurrentGameController.Input.SelectPressed)
            {
                if (movableTiles.Contains(_pointerPosition)) break;
                
                if (GridController.IsTileOccupied(_pointerPosition) && attackableTiles.Contains(_pointerPosition)) break;
            }

            var moveVector = GameController.CurrentGameController.Input.MoveVector;
            
            if (moveVector.y > 0.7f && _pointerPosition.y != GameController.CurrentGameController.upperBorder - 1)
            {
                _pointerPosition.y += 1;
            }else if (moveVector.y < -0.7f && _pointerPosition.y != 0)
            {
                _pointerPosition.y -= 1;
            }else if (moveVector.x > 0.7f && _pointerPosition.x != GameController.CurrentGameController.easternBorder - 1)
            {
                _pointerPosition.x += 1;
            }else if (moveVector.x < -0.7f && _pointerPosition.x != 0)
            {
                _pointerPosition.x -= 1;
            }

            _pointerRenderer.transform.position = GridController.GridCoordinatesToWorldCoordinates(_pointerPosition);
            
            yield return null;
        }
        
        HidePointer();
        ClearOverlayTilemap(overlayTilemap);
    }

    public IEnumerator SelectFromListOfTargets(List<UnitController> targets)
    {
        targetIndex = 0;
        
        SetPointerPosition(targets[0].position);
        ShowPointer();

        while (true)
        {
            if (GameController.CurrentGameController.Input.BackPressed)
            {
                HidePointer();
                break;
            }
            
            if (GameController.CurrentGameController.Input.SelectPressed)
            {
                break;
            }
            
            if (GameController.CurrentGameController.Input.CycleLeftPressed)
            {
                if (targetIndex == 0)
                {
                    targetIndex = targets.Count - 1;
                }
                else
                {
                    targetIndex--;
                }
                
                SetPointerPosition(targets[targetIndex].position);
            }
            
            if (GameController.CurrentGameController.Input.CycleRightPressed)
            {
                if (targetIndex == targets.Count - 1)
                {
                    targetIndex = 0;
                }
                else
                {
                    targetIndex++;
                }
                
                SetPointerPosition(targets[targetIndex].position);
            }

            yield return null;
        }
    }
}

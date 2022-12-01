using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PointerController : MonoBehaviour
{
    public GameObject pointerObject;
    public SpriteRenderer pointerRenderer;
    
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
        pointerRenderer.enabled = true;
    }

    public void HidePointer()
    {
        pointerRenderer.enabled = false;
    }

    public IEnumerator SelectPositionWithPointer()
    {
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

            pointerObject.transform.position = GridController.GridCoordinatesToWorldCoordinates(_pointerPosition);
            
            yield return null;
        }
    }
}

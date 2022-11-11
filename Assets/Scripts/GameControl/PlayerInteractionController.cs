using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInteractionController : MonoBehaviour
{
    public GameObject pointerVisual;
    
    public Vector2Int pointerPosition;
    public bool isFinished = false;

    public IEnumerator SelectPositionWithPointer(Vector2Int pos)
    {
        while (!isFinished)
        {
            //Get input
            if (GameController.Input.SelectPressed)
            {
                isFinished = true;
                break;
            }
            if (Keyboard.current.wKey.wasPressedThisFrame && pointerPosition.y != GameController.UpperBorder - 1)
            {
                pointerPosition.y += 1;
            }else if (Keyboard.current.sKey.wasPressedThisFrame && pointerPosition.y != 0)
            {
                pointerPosition.y -= 1;
            }else if (Keyboard.current.dKey.wasPressedThisFrame && pointerPosition.x != GameController.EasternBorder - 1)
            {
                pointerPosition.x += 1;
            }else if (Keyboard.current.aKey.wasPressedThisFrame && pointerPosition.x != 0)
            {
                pointerPosition.x -= 1;
            }

            pointerVisual.transform.position = GridController.GridCoordinatesToWorldCoordinates(pointerPosition);
            
            yield return null;
        }
    }
}

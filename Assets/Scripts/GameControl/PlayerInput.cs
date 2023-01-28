using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    #region INITIALIZE INPUT
    
    private InputActions _inputActions;
    private void Awake() => _inputActions = new InputActions();
    private void OnEnable() => _inputActions.Enable();
    private void OnDisable() => _inputActions.Disable();

    #endregion

    #region INPUT VARIABLES
        
    public Vector2 MoveVector { get; private set; }
    public bool SelectPressed { get; private set; }
    public bool PausePressed { get; private set; }
    public bool BackPressed { get; private set; }
    public bool CycleLeftPressed { get; private set; }
    public bool CycleRightPressed { get; private set; }
    
    public Vector2 MousePosition { get; private set; }

    
    #endregion
    
    private void Update()
    {
        if (_inputActions.Player.Move.triggered)
        {
            MoveVector = _inputActions.Player.Move.ReadValue<Vector2>();
        }
        else
        {
            MoveVector = Vector2.zero;
        }

        SelectPressed = _inputActions.Player.Select.triggered;
        PausePressed = _inputActions.Player.Pause.triggered;
        BackPressed = _inputActions.Player.Back.triggered;

        CycleLeftPressed = _inputActions.Player.CycleLeft.triggered;
        CycleRightPressed = _inputActions.Player.CycleRight.triggered;

        MousePosition = _inputActions.Player.MousePosition.ReadValue<Vector2>();
    }
}

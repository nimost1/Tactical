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
    public bool HugPressed { get; private set; }
    public bool PausePressed { get; private set; }

    #endregion
    
    private void Update()
    {
        MoveVector = _inputActions.Player.Move.ReadValue<Vector2>();
        
        SelectPressed = _inputActions.Player.Select.triggered;
        HugPressed = _inputActions.Player.Hug.triggered;
        PausePressed = _inputActions.Player.Pause.triggered;
    }
}

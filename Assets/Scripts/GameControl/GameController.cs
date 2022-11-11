using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(GridController), typeof(TurnController),
    typeof(PlayerInteractionController))]
[RequireComponent(typeof(PlayerInput))]
public class GameController : MonoBehaviour
{
    public static GridController Grid;
    public static TurnController TurnController;
    public static PlayerInteractionController PlayerInteraction;
    public static PlayerInput Input;

    //The height and width of the tilemap, i.e., index [EasternBorder, UpperBorder] does not exist.
    public static int UpperBorder = 10;
    public static int EasternBorder = 10;

    public static bool IsHugEnabled;
    public TMP_Text huggingText;
    
    private void Awake()
    {
        Grid = GetComponent<GridController>();
        TurnController = GetComponent<TurnController>();
        PlayerInteraction = GetComponent<PlayerInteractionController>();
        Input = GetComponent<PlayerInput>();

        huggingText.enabled = false;
    }

    private void Update()
    {
        if (Input.HugPressed)
        {
            IsHugEnabled = !IsHugEnabled;
            if (IsHugEnabled)
            {
                huggingText.enabled = true;
            }
            else
            {
                huggingText.enabled = false;
            }
        }
    }
}

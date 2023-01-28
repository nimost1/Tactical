using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(PlayerInteractionController), typeof(PlayerInput), typeof(Reactions))]
public class GameController : MonoBehaviour
{
    public static GameController CurrentGameController;
    
    public List<UnitController> units;
    
    public PlayerInput Input;
    public PlayerInteractionController PlayerInteraction;
    public EventSystem EventSystem;

    //The height and width of the tilemap, i.e., index [EasternBorder, UpperBorder] does not exist.
    public int upperBorder = 10;
    public int easternBorder = 10;

    public bool isPaused;

    public int turnNumber;
    public string saveSlotName;
    
    public Tilemap overlayTilemap;
    public Camera mainCamera;
    
    [SerializeField] private Canvas _pauseCanvas;

    public GameObject unitBase;

    public Tilemap ground;
    public List<Sprite> immovableTiles;
    
    private void Awake()
    {
        if (CurrentGameController == null)
        {
            CurrentGameController = this;
        }
        else
        {
            Destroy(this);
        }

        Input = GetComponent<PlayerInput>();
        PlayerInteraction = GetComponent<PlayerInteractionController>();
    }

    public void FindGroundTilemap()
    {
        //Find the ground tilemap
        ground = FindObjectOfType<Tilemap>();
    }

    private void Start()
    {
        SaveController.LoadMostRecentSave();
        
        //TurnController.SortUnits(units);

        //AnimationController.InitializeState();
        
        //units[0].TakeTurn();
    }

    private void Update()
    {
        if (Input.PausePressed && !LevelLoadController.CurrentLevelLoadController.isLoadingNewScene)
        {
            isPaused = !isPaused;

            _pauseCanvas.enabled = isPaused;
        }
    }

    public void React()
    {
        foreach (var unit in units)
        {
            unit.React();
        }
    }

    public Coroutine StartAnimateState()
    {
        return StartCoroutine(AnimationController.AnimateState());
    }
}

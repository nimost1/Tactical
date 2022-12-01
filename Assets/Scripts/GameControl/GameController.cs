using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(PointerController), typeof(PlayerInput), typeof(Reactions))]
public class GameController : MonoBehaviour
{
    public static GameController CurrentGameController;
    
    public List<UnitController> units;
    
    public static PlayerInput Input;
    public static PointerController Pointer;

    //The height and width of the tilemap, i.e., index [EasternBorder, UpperBorder] does not exist.
    public int upperBorder = 10;
    public int easternBorder = 10;

    public bool isPaused;

    public static bool IsHugEnabled;
    public TMP_Text huggingText;

    [SerializeField] private Canvas _pauseCanvas;

    //public Tilemap ground;
    
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
        
        huggingText.enabled = false;
    }

    /*public void FindGroundTilemap()
    {
        //Find the ground tilemap
        ground = GameObject.Find("Ground").GetComponent<Tilemap>();
    }*/

    private void Start()
    {
        Pointer = GetComponent<PointerController>();
        
        units.Sort((a, b) => { if (a == b) return 0;
            return a.turnOrder > b.turnOrder ? 1 : -1; });

        AnimationController.InitializeState();
        
        units[0].TakeTurn();
    }

    private void Update()
    {
        if (Input.PausePressed && !LoadController.CurrentLoadController.isLoadingNewScene)
        {
            isPaused = !isPaused;

            _pauseCanvas.enabled = isPaused;
        }
        
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

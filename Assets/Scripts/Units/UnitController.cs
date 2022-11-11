using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    #region UNIQUE VARIABLES

    public bool canOccupyTile;
    public string unitName;
    public int maxHitPoints;
    public int movementRange;
    public bool isControlledByPlayer;
    public int turnOrder;//Low number means early turn.
    
    #endregion

    #region GAMEPLAY VARIABLES

    public Vector2Int position;
    public int hitPoints;
    
    #endregion

    private TMP_Text _healthText;

    protected virtual void Initialize()
    {
        print("Unimplemented Initialize-method");
    }

    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        hitPoints = maxHitPoints;
        _healthText = GetComponentInChildren<TMP_Text>();
        _healthText.text = hitPoints.ToString();
        
        position = GridController.WorldCoordinatesToGridCoordinates(transform.position);
        transform.position = GridController.GridCoordinatesToWorldCoordinates(position);

        if (canOccupyTile)
        {
            GameController.Grid.AddOccupation(position);
        }
    }

    public void TakeTurn()
    {
        print($"It is {unitName}'s turn.");
        
        if (isControlledByPlayer)
        {
            StartCoroutine(TakePlayerTurn());
        }
        else
        {
            TakeAITurn();
            FinishTurn();
        }
    }

    protected void FinishTurn()
    {
        //Returns control to TurnController.
        StartCoroutine(GameController.TurnController.ProceedToNextTurn());
    }

    protected virtual IEnumerator TakePlayerTurn()
    {
        print("Standard/unimplemented TakePlayerTurn");
        
        yield return null;
        
        FinishTurn();
    }

    protected virtual void TakeAITurn()
    {
        print("Unimplemented TakeAITurn");
        return;
    }

    public virtual void React()
    {
        print("Standard/unimplemented React-function");
        return;
    }

    protected virtual void AcceptHug(UnitController hugger)
    {
        print("Unimplemented AcceptHug-function");
    }

    public void TakeDamageHit(int damage)
    {
        Reactions.TargetedUnits.Add(this);
        
        hitPoints -= damage;
        _healthText.text = hitPoints.ToString();
        
        if (damage > 0)
        {
            Reactions.DamagedUnits.Add(this);
        }
        if (hitPoints <= 0)
        {
            Reactions.KilledUnits.Add(this);
            
            if (canOccupyTile)
            {
                GameController.Grid.RemoveOccupation(position);
            }

            GameController.TurnController.RemoveUnit(this);
        }
    }

    public void MoveTo(Vector2Int pos)
    {
        if (canOccupyTile)
        {
            GameController.Grid.RemoveOccupation(position);
            GameController.Grid.AddOccupation(pos);
        }
        position = pos;
        transform.position = GridController.GridCoordinatesToWorldCoordinates(pos);
    }

    protected void Attack(UnitController targetUnit, int damage)
    {
        print($"{unitName} attacks, {damage} damage.");
        targetUnit.TakeDamageHit(damage);
    }

    protected void Attack(Vector2Int pos, int damage)
    {
        if (GameController.Grid.IsTileOccupied(pos))
        {
            Attack(GameController.Grid.GetUnitOnSpace(pos), damage);
        }
        else
        {
            print("Tried to attack an empty space.");
        }
    }

    protected void Hug(UnitController self, UnitController target)
    {
        target.AcceptHug(self);
    }
}

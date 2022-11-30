using System;
using System.Collections;
using System.Collections.Generic;
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
        GameController.CurrentGameController.units.Add(this);
        
        hitPoints = maxHitPoints;
        _healthText = GetComponentInChildren<TMP_Text>();
        _healthText.text = hitPoints.ToString();
        
        position = GridController.WorldCoordinatesToGridCoordinates(transform.position);
        transform.position = GridController.GridCoordinatesToWorldCoordinates(position);
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
            StartCoroutine(FinishTurn());
        }
    }

    protected IEnumerator FinishTurn()
    {
        //Lets all other units react
        GameController.CurrentGameController.React();
        
        //Animate any changes
        yield return AnimationController.AnimateState();

        //Gives control to the next unit in the turn order
        TurnController.ProceedToNextTurn(this);
    }

    protected virtual IEnumerator TakePlayerTurn()
    {
        print("Standard/unimplemented TakePlayerTurn");
        
        yield return null;
        
        StartCoroutine(FinishTurn());
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
        Reactions.CurrentReactions.targetedUnits.Add(this);
        
        hitPoints -= damage;
        _healthText.text = hitPoints.ToString();
        
        if (damage > 0)
        {
            Reactions.CurrentReactions.damagedUnits.Add(this);
        }
        if (hitPoints <= 0)
        {
            Reactions.CurrentReactions.killedUnits.Add(this);

            TurnController.RemoveUnitFromUnitList(this);
        }
    }

    public void MoveTo(Vector2Int pos)
    {
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
        if (GridController.IsTileOccupied(pos))
        {
            Attack(GridController.GetUnitOnSpace(pos), damage);
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

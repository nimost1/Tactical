using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    #region UNIQUE VARIABLES

    public bool canOccupyTile;
    [SaveField] public string unitName;
    public int maxHitPoints;
    public int movementRange;
    [SaveField] public bool isControlledByPlayer;
    public int turnOrder;//Low number means early turn.
    
    #endregion

    #region GAMEPLAY VARIABLES

    [SaveField] public Vector2Int position;
    [SaveField] public int hitPoints;
    
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
            FinishTurn();
        }
    }

    protected void FinishTurn()
    {
        //Lets all other units react
        GameController.CurrentGameController.React();

        //Gives control to the next unit in the turn order
        TurnController.ProceedToNextTurn(this);
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
        if (GridController.IsTileOccupiedByUnit(pos))
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
    
    //Returns all members that should be saved
    public string GetSaveData()
    {
        var typeInfo = GetType();
        var saveData = $"UNIT:{typeInfo.Name}\n";
        
        foreach (var field in typeInfo.GetFields())
        {
            if (field.GetCustomAttributes(typeof(SaveFieldAttribute), true).Length != 0)
            {
                var value = field.GetValue(this);
                if (value is int) saveData += "INT:";
                else if (value is bool) saveData += "BOOL:";
                else if (value is string) saveData += "STRING:";
                else if (value is Vector2Int) saveData += "VECTOR2INT:";
                saveData += $"{field.Name}:{value}\n";
            }
        }

        saveData += "\n";

        return saveData;
    }

    public virtual void UpdateAfterLoad()
    {
        transform.position = GridController.GridCoordinatesToWorldCoordinates(position);
        _healthText.text = hitPoints.ToString();
    }
}

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
            StartCoroutine(TakeAITurn());
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

    protected virtual IEnumerator TakeAITurn()
    {
        print("Unimplemented TakeAITurn");
        yield return null;
        FinishTurn();
    }

    public virtual void React()
    {
        print("Standard/unimplemented React-function");
        return;
    }

    protected virtual bool AcceptHug(UnitController hugger)
    {
        print("Unimplemented AcceptHug-function");
        return false;
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

    private bool Hug(UnitController target)
    {
        //Returns true if the hug results in the hugger moving onto the target's space.
        return target.AcceptHug(this);
    }

    #region SAVE_AND_LOAD

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
    
    #endregion
    
    #region ANIMATION
    protected IEnumerator MoveAlongPath(List<Vector2Int> path)
    {
        if (path.Count == 0) yield break;
        path.Insert(0, position);
        
        var index = 1;
        var startTime = Time.time;
        var timePerTile = 0.5f;

        while (index < path.Count)
        {
            var timePassed = Time.time - startTime;
            
            var xPosition = Mathf.Lerp(path[index - 1].x, path[index].x, timePassed / timePerTile - timePerTile * (index - 1));
            var yPosition = Mathf.Lerp(path[index - 1].y, path[index].y, timePassed / timePerTile - timePerTile * (index - 1));

            transform.position =
                GridController.GridCoordinatesToWorldCoordinates(new Vector2(xPosition, yPosition));
            
            if (timePassed >= timePerTile * index)
            {
                index++;
            }
            
            yield return null;
        }

        transform.position = GridController.GridCoordinatesToWorldCoordinates(path[path.Count - 1]);
        position = path[path.Count - 1];
    }

    protected IEnumerator AnimateMeleeAttack(UnitController target, int damage)
    {
        var startTime = Time.time;
        var attackTime = 0.4f;
        var largestOffset = (Vector2)(target.position - position) / 2f;
        
        while (Time.time < startTime + attackTime * 0.5f)
        {
            var fractionOfTimePassed = (Time.time - startTime) / attackTime;
            var animatedPosition = position + fractionOfTimePassed * 2f * largestOffset;
            transform.position = GridController.GridCoordinatesToWorldCoordinates(animatedPosition);
            yield return null;
        }

        Attack(target, 1);

        while (Time.time < startTime + attackTime)
        {
            var fractionOfTimePassed = (Time.time - startTime) / attackTime;
            var animatedPosition = position + (2f - fractionOfTimePassed * 2f) * largestOffset;
            transform.position = GridController.GridCoordinatesToWorldCoordinates(animatedPosition);
            yield return null;
        }

        transform.position = GridController.GridCoordinatesToWorldCoordinates(position);
    }

    protected IEnumerator AnimateHug(UnitController target)
    {
        var startTime = Time.time;
        var attackTime = 0.4f;
        var largestOffset = (Vector2)(target.position - position);
        
        while (Time.time < startTime + attackTime * 0.5f)
        {
            var fractionOfTimePassed = (Time.time - startTime) / attackTime;
            var animatedPosition = position + fractionOfTimePassed * 2f * largestOffset;
            transform.position = GridController.GridCoordinatesToWorldCoordinates(animatedPosition);
            yield return null;
        }

        if (Hug(target))
        {
            transform.position = GridController.GridCoordinatesToWorldCoordinates(target.position);
            yield break;
        }

        while (Time.time < startTime + attackTime)
        {
            var fractionOfTimePassed = (Time.time - startTime) / attackTime;
            var animatedPosition = position + (2f - fractionOfTimePassed * 2f) * largestOffset;
            transform.position = GridController.GridCoordinatesToWorldCoordinates(animatedPosition);
            yield return null;
        }

        transform.position = GridController.GridCoordinatesToWorldCoordinates(position);

    }
    #endregion
}

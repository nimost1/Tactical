using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reactions : MonoBehaviour
{
    public static Reactions CurrentReactions = null;
    
    public List<UnitController> targetedUnits = new List<UnitController>();
    public List<UnitController> damagedUnits = new List<UnitController>();
    public List<UnitController> killedUnits = new List<UnitController>();
    
    public List<Vector2Int> spacesMovedOver = new List<Vector2Int>();

    private void Awake()
    {
        if (CurrentReactions == null)
        {
            CurrentReactions = this;
        }
        else
        {
            Destroy(this);
        }
    }


    public void ClearData()
    {
        targetedUnits.Clear();
        damagedUnits.Clear();
        killedUnits.Clear();
        spacesMovedOver.Clear();
    }

    public bool IsUnitInMovementRange(UnitController caller, UnitController other)
    {
        List<Vector2Int> rangeList = GridController.GetMovableTilesInRange(caller.position, caller.movementRange);

        return rangeList.Contains(other.position);
    }

    public bool WasUnitWithNameKilled(string unitName)
    {
        return killedUnits.Exists(unit => unit.unitName == unitName);
    }
}

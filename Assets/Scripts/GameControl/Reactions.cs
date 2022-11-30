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
        List<Vector2Int> rangeList = GridController.GetReachableFromTile(caller.position, caller.movementRange);

        foreach (var pos in rangeList)
        {
            if (other.position == pos)
            {
                return true;
            }
        }

        return false;
    }

    public bool WasUnitWithNameKilled(string unitName)
    {
        foreach (var unit in killedUnits)
        {
            if (unit.unitName == unitName)
            {
                return true;
            }
        }

        return false;
    }
}

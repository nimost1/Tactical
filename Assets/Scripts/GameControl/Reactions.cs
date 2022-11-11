using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reactions
{
    public static List<UnitController> TargetedUnits = new List<UnitController>();
    public static List<UnitController> DamagedUnits = new List<UnitController>();
    public static List<UnitController> KilledUnits = new List<UnitController>();
    
    public static List<Vector2Int> SpacesMovedOver = new List<Vector2Int>();

    public static void ClearData()
    {
        TargetedUnits.Clear();
        DamagedUnits.Clear();
        KilledUnits.Clear();
        SpacesMovedOver.Clear();
    }

    public static bool IsUnitInMovementRange(UnitController caller, UnitController other)
    {
        List<Vector2Int> rangeList = GameController.Grid.GetReachableFromTile(caller.position, caller.movementRange);

        foreach (var pos in rangeList)
        {
            if (other.position == pos)
            {
                return true;
            }
        }

        return false;
    }

    public static bool WasUnitWithNameKilled(string name)
    {
        foreach (var unit in KilledUnits)
        {
            if (unit.unitName == name)
            {
                return true;
            }
        }

        return false;
    }
}

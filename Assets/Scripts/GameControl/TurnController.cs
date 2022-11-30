using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TurnController : MonoBehaviour
{
    public static UnitController NextUnit(List<UnitController> units, UnitController currentUnit)
    {
        int index = units.IndexOf(currentUnit) + 1;
        if (index >= units.Count)
        {
            index = 0;
        }
        return units[index];
    }

    public static void ProceedToNextTurn(UnitController currentUnit)
    {
        NextUnit(GameController.CurrentGameController.units, currentUnit).TakeTurn();
    }

    public static void RemoveUnitFromUnitList(UnitController unitToRemove/*, ref UnitController currentUnit*/)
    {
        /*if (currentUnit == unitToRemove)
        {
            currentUnit = NextUnit(units, currentUnit);
        }*/

        GameController.CurrentGameController.units.Remove(unitToRemove);
        
        Destroy(unitToRemove.gameObject);
    }
}
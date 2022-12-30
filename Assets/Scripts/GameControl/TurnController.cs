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
            GameController.CurrentGameController.turnNumber++;
        }
        return units[index];
    }

    public static void ProceedToNextTurn(UnitController currentUnit)
    {
        var nextUnit = NextUnit(GameController.CurrentGameController.units, currentUnit);
        
        if (nextUnit.GetType().Name == "PlayerCharacter") 
        { 
            SaveController.Save();
        }
        
        nextUnit.TakeTurn();
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

    public static void SortUnits(List<UnitController> units)
    {
        units.Sort((a, b) => { if (a == b) return 0;
            return a.turnOrder > b.turnOrder ? 1 : -1; });
    }

    public static void StartPlaying()
    {
        var playerUnit =
            GameController.CurrentGameController.units.Find(unit => unit.GetType().Name == "PlayerCharacter");
        if (playerUnit != null)
        {
            playerUnit.TakeTurn();
        }
        else
        {
            GameController.CurrentGameController.units[0].TakeTurn();
        }
    }
}
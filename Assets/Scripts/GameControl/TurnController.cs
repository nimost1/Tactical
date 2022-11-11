using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TurnController : MonoBehaviour
{
    public static List<UnitController> Units;

    private UnitController _nextUnit;

    private bool _proceedToNextTurn;

    private void Awake()
    {
        Units = new List<UnitController>(FindObjectsOfType<UnitController>());
    }

    private void Start()
    {
        Units.Sort((a, b) => { if (a == b) return 0;
            return a.turnOrder > b.turnOrder ? 1 : -1; });
        
        _nextUnit = Units[0];
        
        _proceedToNextTurn = true;
    }

    private void Update()
    {
        if (_proceedToNextTurn)
        {
            _proceedToNextTurn = false;
            _nextUnit.TakeTurn();
            SetNextUnit();
        }
    }

    
    /*
     * Class GameController
     * TurnController GetComponent
     * Update (turnController.SetNextUnits(FeedNewList)
     */
    
    public void SetNextUnit()
    {
        int index = Units.IndexOf(_nextUnit) + 1;
        if (index >= Units.Count)
        {
            index = 0;
        }
        _nextUnit = Units[index];
    }

    //TODO: Ta en second pass på denne funksjonaliteten.
    public IEnumerator ProceedToNextTurn()
    {
        yield return new WaitForSeconds(0.3f);
        foreach (var unit in Units)
        {
            unit.React();
        }

        Reactions.ClearData();
        _proceedToNextTurn = true;
    }

    public void RemoveUnit(UnitController unitToRemove)
    {
        if (_nextUnit == unitToRemove)
        {
            SetNextUnit();
        }

        Units.Remove(unitToRemove);
        
        Destroy(unitToRemove.gameObject);
    }
}
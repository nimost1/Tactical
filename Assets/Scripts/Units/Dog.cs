using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Dog : UnitController
{
    public bool isPassive;
    
    protected override void Initialize()
    {
        canOccupyTile = true;
        unitName = "Dog";
        maxHitPoints = 1;
        movementRange = 3;
        isControlledByPlayer = false;
        turnOrder = 2;
    }
    
    protected override void TakeAITurn()
    {
        if (isPassive) return;
        
        //Find units in range and sort out the dogs and the dogowner.
        List<UnitController> unitList = GameController.CurrentGameController.units.ToList();
        unitList.RemoveAll(unit => unit.unitName is "Dog" or "DogOwner");
        
        //Do nothing if no targets are found.
        if (unitList.Count == 0) return;
        
        //Select a target
        UnitController target = UnitAIUtils.SelectClosestUnit(this, unitList);
        
        //Move towards target
        Vector2Int targetTile = UnitAIUtils.FindMovementTargetTowardsUnit(this, target);
        MoveTo(targetTile);

        //Attack if possible
        if (GridController.DistanceBetweenTiles(position, target.position) == 1)
        {
            Attack(target, 1);
        }
    }

    public override void React()
    {
        if (Reactions.CurrentReactions.killedUnits.Exists(unit => unit.unitName is "DogOwner" or "Dog"))
        {
            isPassive = false;
        }
    }

    protected override void AcceptHug(UnitController hugger)
    {
        if (isPassive) return;
        Attack(hugger, 1);
        print("Player took 1 damage while hugging.");
    }
}

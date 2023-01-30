using System.Collections;
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
    
    protected override IEnumerator TakeAITurn()
    {
        if (isPassive) yield break;
        
        //Find units in range and sort out the dogs and the dogowner.
        List<UnitController> unitList = GameController.CurrentGameController.units.ToList();
        unitList.RemoveAll(unit => unit.unitName is "Dog" or "DogOwner");
        
        //Do nothing if no targets are found.
        if (unitList.Count == 0) yield break;
        
        //Select a target
        UnitController target = UnitAIUtils.SelectClosestUnit(this, unitList);
        
        //Move towards target
        Vector2Int targetTile = UnitAIUtils.FindMovementTargetTowardsUnit(this, target);
        yield return MoveAlongPath(GridController.ShortestMovablePathBetweenTiles(position, targetTile));

        //Attack if possible
        if (GridController.DistanceBetweenTiles(position, target.position) == 1)
        {
            yield return AnimateMeleeAttack(target, 1);
        }
    }

    public override void React()
    {
        if (Reactions.CurrentReactions.killedUnits.Exists(unit => unit.unitName is "DogOwner" or "Dog"))
        {
            isPassive = false;
        }
    }

    protected override bool AcceptHug(UnitController hugger)
    {
        if (isPassive) return false;
        Attack(hugger, 1);
        print("Player took 1 damage while hugging.");
        return false;
    }
}

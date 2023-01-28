using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DogOwner : UnitController
{
    private bool _isAngry;
    [SaveField] public bool isFriendly;

    protected override void Initialize()
    {
        canOccupyTile = true;
        unitName = "DogOwner";
        maxHitPoints = 1;
        movementRange = 3;
        isControlledByPlayer = false;
        turnOrder = 1;
    }
    
    protected override IEnumerator TakeAITurn()
    {
        if (!_isAngry) yield break;

        foreach (var pos in GridController.GetAdjacentTiles(position))
        {
            if (GridController.IsTileOccupiedByUnit(pos) &&
                GridController.GetUnitOnSpace(pos).unitName == "Player")
            {
                yield return AnimateMeleeAttack(GridController.GetUnitOnSpace(pos), 3);
                break;
            }
        }
        
        FinishTurn();
    }

    public override void React()
    {
        if (!isFriendly && Reactions.CurrentReactions.WasUnitWithNameKilled("Dog"))
        {
            _isAngry = true;
        }
    }

    protected override bool AcceptHug(UnitController hugger)
    {
        if (_isAngry)
        {
            Attack(hugger, 3);
            print("Player took 3 damage while hugging.");
            return false;
        }
        else
        {
            isFriendly = true;

            var unitList = GameController.CurrentGameController.units.ToList().OfType<Dog>();
            foreach (var dog in unitList)
            {
                dog.isPassive = true;
            }

            return false;
        }
    }
}

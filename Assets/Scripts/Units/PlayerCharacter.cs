using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : UnitController
{
    protected override void Initialize()
    {
        canOccupyTile = true;
        unitName = "Player";
        maxHitPoints = 3;
        movementRange = 3;
        isControlledByPlayer = true;
        turnOrder = 0;
    }

    protected override IEnumerator TakePlayerTurn()
    {
        Vector2Int target;
        GameController.PlayerInteraction.pointerPosition = position;

        do
        {
            //Movement
            yield return GameController.PlayerInteraction.SelectPositionWithPointer(position);

            GameController.PlayerInteraction.isFinished = false;
            target = GameController.PlayerInteraction.pointerPosition;
        } while (target == position
                 || (!GameController.Grid.IsTileOccupied(target)
                     && !GameController.Grid.CanMoveTo(position, target, movementRange))
                 || (GameController.Grid.IsTileOccupied(target)
                     && !GameController.Grid.CanAttackMelee(position, target, movementRange)));
        
        //If there is a unit on the target space, move to it and attack, else move to the target.
        if (GameController.Grid.IsTileOccupied(target))
        {
            MoveTo(UnitAIUtils.FindMovementTargetTowardsUnit(this, GameController.Grid.GetUnitOnSpace(target)));
            if (GameController.IsHugEnabled)
            {
                Hug(this, GameController.Grid.GetUnitOnSpace(target));
            }
            else
            {
                Attack(GameController.Grid.GetUnitOnSpace(target), 1);
            }
        }
        else
        {
            MoveTo(target);
        }

        FinishTurn();
    }

    public override void React()
    {
        return;
    }
}

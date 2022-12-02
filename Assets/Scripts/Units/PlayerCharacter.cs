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
        GameController.Pointer.SetPointerPosition(position);

        do
        {
            //Movement
            yield return GameController.Pointer.SelectPositionWithPointer(position, movementRange, 1,
                GameController.CurrentGameController.overlayTilemap);

            target = GameController.Pointer.GetPointerPosition();
        } while (target == position
                 || (!GridController.IsTileOccupied(target)
                     && !GridController.CanMoveTo(position, target, movementRange))
                 || (GridController.IsTileOccupied(target)
                     && !GridController.CanAttackMelee(position, target, movementRange)));

        //If there is a unit on the target space, move to it and attack. If not, move to the target.
        if (GridController.IsTileOccupied(target))
        {
            MoveTo(UnitAIUtils.FindMovementTargetTowardsUnit(this,
                GridController.GetUnitOnSpace(target)));
            if (GameController.IsHugEnabled)
            {
                Hug(this, GridController.GetUnitOnSpace(target));
            }
            else
            {
                Attack(GridController.GetUnitOnSpace(target), 1);
            }
        }
        else
        {
            MoveTo(target);
        }

        StartCoroutine(FinishTurn());
    }

    public override void React()
    {
        return;
    }
}

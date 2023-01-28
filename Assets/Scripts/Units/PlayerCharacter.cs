using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

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
        yield return GameController.CurrentGameController.PlayerInteraction.GetPlayerInput(position, movementRange, 1);

        var result = GameController.CurrentGameController.PlayerInteraction.result;

        switch (result.MenuChoice)
        {
            case PlayerInteractionController.MenuOptions.Wait:
                yield return MoveAlongPath(GridController.ShortestMovablePathBetweenTiles(position, result.MovementTarget));
                break;
            case PlayerInteractionController.MenuOptions.Attack:
                yield return MoveAlongPath(GridController.ShortestMovablePathBetweenTiles(position, result.MovementTarget));
                
                yield return AnimateMeleeAttack(GridController.GetUnitOnSpace(result.ActionTarget), 1);
                break;
            case PlayerInteractionController.MenuOptions.Hug:
                yield return MoveAlongPath(GridController.ShortestMovablePathBetweenTiles(position, result.MovementTarget));

                yield return AnimateHug(GridController.GetUnitOnSpace(result.ActionTarget));
                break;
        }

        FinishTurn();
    }

    public override void React()
    {
        return;
    }

    protected override bool AcceptHug(UnitController hugger)
    {
        return true;
    }
}

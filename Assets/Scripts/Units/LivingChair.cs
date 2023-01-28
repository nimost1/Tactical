using System.Collections;
using System.Linq;
using UnityEngine;

public class LivingChair : UnitController
{
    public enum State
    {
        Aggressive,
        Friendly,
        SatOn,
        Destroyed
    }

    [SaveField] public State currentState = State.Aggressive;
    
    protected override void Initialize()
    {
        canOccupyTile = true;
        unitName = "Stol";
        maxHitPoints = 1;
        movementRange = 1;
        isControlledByPlayer = false;
        turnOrder = 5;
    }

    protected override IEnumerator TakeAITurn()
    {
        if (currentState is State.SatOn or State.Destroyed) yield break;
        
        if (currentState is State.Friendly)
        {
            //Do friendly things
            var chairs = GridController.GetUnitsInAttackRange(position, 1).OfType<LivingChair>().ToList();
            foreach (var chair in chairs)
            {
                if (chair.currentState == State.Aggressive)
                {
                    yield return AnimateMeleeAttack(chair, 1);
                }
            }
        }
        else if (currentState is State.Aggressive)
        {
            //Do aggressive things
            var units = GridController.GetUnitsInAttackRange(position, 1).OfType<PlayerCharacter>().ToList();
            if (units.Count != 0)
            {
                yield return AnimateMeleeAttack(units[0], 1);
            }
            else
            {
                foreach (var unit in GameController.CurrentGameController.units)
                {
                    if (unit.isControlledByPlayer)
                    {
                        var target = UnitAIUtils.FindMovementTargetTowardsUnit(this, unit);
                        yield return MoveAlongPath(GridController.ShortestMovablePathBetweenTiles(position, target));
                    }
                }
            }
        }
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
        if (GameController.CurrentGameController.Reactions.damagedUnits.Contains(this))
        {
            currentState = State.Destroyed;
            canOccupyTile = true;
        }
    }

    protected override bool AcceptHug(UnitController hugger)
    {
        if (hugger.isControlledByPlayer)
        {
            currentState = State.Friendly;
            canOccupyTile = false;
            return true;
        }
        
        return false;
    }
}
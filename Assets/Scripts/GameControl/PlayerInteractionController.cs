using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PointerController), typeof(MenuController))]
public class PlayerInteractionController : MonoBehaviour
{
    public PointerController Pointer;
    public MenuController Menu;

    public InteractionResult result;

    private void Awake()
    {
        Pointer = GetComponent<PointerController>();
        Menu = GetComponent<MenuController>();
    }
    
    public enum MenuOptions
    {
        Wait,
        Attack,
        Hug
    }
    
    public struct InteractionResult
    {
        public MenuOptions MenuChoice;
        public Vector2Int MovementTarget;
        public Vector2Int ActionTarget;
    }

    public IEnumerator GetPlayerInput(Vector2Int startPosition, int movementRange, params int[] attackRanges)
    {
        //Initialize the InteractionResult struct
        result = new InteractionResult();
        
        //Reset the position of the pointer
        Pointer.SetPointerPosition(startPosition);

        bool waitingForMenu;

        var chosenOption = MenuOptions.Wait;
        
        //Callback functions
        void OnWait()
        {
            
            waitingForMenu = false;
            chosenOption = MenuOptions.Wait;
        }

        void OnAttack()
        {
            waitingForMenu = false;
            chosenOption = MenuOptions.Attack;
        }

        void OnHug()
        {
            waitingForMenu = false;
            chosenOption = MenuOptions.Hug;
        }

        //The point where behaviour resumes if player tries to go back to start
        while (true)
        {
            Pointer.ShowPointer();
            //Select a position with pointer
            yield return Pointer.SelectPositionWithPointer(startPosition, movementRange,
                1, GameController.CurrentGameController.overlayTilemap);

            var pointerPosition = Pointer.GetPointerPosition();

            Menu.ResetMenu();

            //Generate action menu
            if (GridController.IsTileOccupiedByUnit(pointerPosition) && pointerPosition != startPosition)
            {
                //Has selected a unit.
                Menu.AddButton("Attack", OnAttack);
                Menu.AddButton("Hug", OnHug);
            }
            else
            {
                //Has selected an empty space.
                Menu.AddButton("Wait", OnWait);
                
                var attackTargets = GridController.GetUnitsInAttackRange(pointerPosition, attackRanges);
                attackTargets.Remove(GridController.GetUnitOnSpace(startPosition));
                if (attackTargets.Count != 0)
                {
                    Menu.AddButton("Attack", OnAttack);
                }

                var hugTargets = GridController.GetUnitsInAttackRange(pointerPosition, 1);
                hugTargets.Remove(GridController.GetUnitOnSpace(startPosition));
                if (hugTargets.Count != 0)
                {
                    Menu.AddButton("Hug", OnHug);
                }
            }

            //Show the menu and wait for input
            waitingForMenu = true;
            Menu.DisplayMenu();

            var goToStart = false;
            while (waitingForMenu)
            {
                //If the player interrupts, go back to start
                if (GameController.CurrentGameController.Input.BackPressed)
                {
                    goToStart = true;
                    waitingForMenu = false;
                    Menu.ResetMenu();
                    Menu.HideMenu();
                    break;
                }

                yield return null;
            }

            if (goToStart) continue;
            
            //Hide the menu after a successful choice of action.
            Menu.ResetMenu();
            Menu.HideMenu();

            //Waits a frame to let input-state reset.
            yield return null;

            result.MenuChoice = chosenOption;
            
            if (chosenOption == MenuOptions.Wait)
            {
                result.MovementTarget = pointerPosition;
                result.ActionTarget = pointerPosition;
            }
            else if (chosenOption == MenuOptions.Attack)
            {
                var targets = GridController.GetUnitsInAttackRange(pointerPosition, attackRanges);
                targets.Remove(GridController.GetUnitOnSpace(startPosition));
                
                if (GridController.IsTileOccupiedByUnit(pointerPosition) && pointerPosition != startPosition)
                {
                    //The player has selected a tile with another unit
                    result.MovementTarget = UnitAIUtils.FindMovementTargetTowardsUnit(
                        GridController.GetUnitOnSpace(startPosition),
                        GridController.GetUnitOnSpace(pointerPosition));
                    result.ActionTarget = pointerPosition;
                }
                else
                {
                    //The player has selected an empty space
                    result.MovementTarget = pointerPosition;
                    
                    //Choose a valid unit in range
                    yield return Pointer.SelectFromListOfTargets(targets);

                    if (GameController.CurrentGameController.Input.BackPressed)
                    {
                        continue;
                    }
                    
                    result.ActionTarget = targets[Pointer.targetIndex].position;
                }
            }
            else if (chosenOption == MenuOptions.Hug)
            {
                var targets = GridController.GetUnitsInAttackRange(pointerPosition, attackRanges);
                targets.Remove(GridController.GetUnitOnSpace(startPosition));

                if (GridController.IsTileOccupiedByUnit(pointerPosition) && pointerPosition != startPosition)
                {
                    //The player has selected a tile with a unit
                    result.MovementTarget = UnitAIUtils.FindMovementTargetTowardsUnit(
                        GridController.GetUnitOnSpace(startPosition),
                        GridController.GetUnitOnSpace(pointerPosition));
                    result.ActionTarget = pointerPosition;
                }
                else
                {
                    //The player has selected an empty space
                    result.MovementTarget = pointerPosition;

                    //Choose a valid unit in range
                    yield return Pointer.SelectFromListOfTargets(targets);

                    if (GameController.CurrentGameController.Input.BackPressed)
                    {
                        continue;
                    }

                    result.ActionTarget = targets[Pointer.targetIndex].position;
                }
            }

            Pointer.HidePointer();
            
            break;
        }
    }
}
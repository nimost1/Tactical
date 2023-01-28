using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public static void InitializeState()
    {
        
    }

    public static IEnumerator AnimateState()
    {
        while (GameController.CurrentGameController.isPaused)
        {
            yield return null;
        }
        
        yield return null;
    }

    public static IEnumerator MoveAlongPath(UnitController unit, List<Vector2Int> path)
    {
        if (path.Count == 0) yield break;
        path.Insert(0, unit.position);
        
        var index = 1;
        var startTime = Time.time;
        var timePerTile = 0.5f;

        while (index < path.Count)
        {
            var timePassed = Time.time - startTime;
            
            var xPosition = Mathf.Lerp(path[index - 1].x, path[index].x, timePassed / timePerTile - timePerTile * (index - 1));
            var yPosition = Mathf.Lerp(path[index - 1].y, path[index].y, timePassed / timePerTile - timePerTile * (index - 1));

            unit.transform.position =
                GridController.GridCoordinatesToWorldCoordinates(new Vector2(xPosition, yPosition));
            
            if (timePassed >= timePerTile * index)
            {
                index++;
            }
            
            yield return null;
        }

        unit.transform.position = GridController.GridCoordinatesToWorldCoordinates(path[path.Count - 1]);
        unit.position = path[path.Count - 1];
    }

    public static IEnumerator AnimateMeleeAttack(UnitController unit, UnitController target)
    {
        var startTime = Time.time;
        var attackTime = 0.4f;
        var largestOffset = (Vector2)(target.position - unit.position) / 2f;
        
        while (Time.time < startTime + attackTime * 0.5f)
        {
            var fractionOfTimePassed = (Time.time - startTime) / attackTime;
            var animatedPosition = unit.position + fractionOfTimePassed * 2f * largestOffset;
            unit.transform.position = GridController.GridCoordinatesToWorldCoordinates(animatedPosition);
            yield return null;
        }

        while (Time.time < startTime + attackTime)
        {
            var fractionOfTimePassed = (Time.time - startTime) / attackTime;
            var animatedPosition = unit.position + (2f - fractionOfTimePassed * 2f) * largestOffset;
            unit.transform.position = GridController.GridCoordinatesToWorldCoordinates(animatedPosition);
            yield return null;
        }

        unit.transform.position = GridController.GridCoordinatesToWorldCoordinates(unit.position);
    }

    public static IEnumerator AnimateHug(UnitController unit)
    {
        yield return null;
    }
}

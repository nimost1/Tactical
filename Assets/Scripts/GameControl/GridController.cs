using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridController : MonoBehaviour
{
    //The grid controller is supposed to contain methods that let units make decisions on how to act, based on the grid state.

    public static bool IsTileOccupiedByUnit(Vector2Int pos)
    {
        foreach (var unit in GameController.CurrentGameController.units)
        {
            if (unit.canOccupyTile && unit.position == pos) return true;
        }

        return false;
    }

    public static bool IsTileMovable(Vector2Int pos)
    {
        return !GameController.CurrentGameController.immovableTiles.Contains(
            GameController.CurrentGameController.ground.GetSprite(new Vector3Int(pos.x, pos.y, 0)));
    }

    public static UnitController GetUnitOnSpace(Vector2Int pos)
    {
        foreach (var unit in GameController.CurrentGameController.units)
        {
            if (unit.position == pos)
            {
                return unit;
            }
        }
        
        return null;
    }

    public static int DistanceBetweenTiles(Vector2Int startPos, Vector2Int endPos)
    {
        return Mathf.Abs(startPos.x - endPos.x) + Mathf.Abs(startPos.y - endPos.y);
    }

    public static List<Vector2Int> ShortestMovablePathBetweenTiles(Vector2Int startPos, Vector2Int endPos)
    {
        //Using A*-search
        var open = new List<Vector2Int>();
        var closed = new List<Vector2Int>();
        var parents = new Dictionary<Vector2Int, Vector2Int>();
        var costs = new Dictionary<Vector2Int, int>();

        open.Add(startPos);
        costs.Add(startPos, 0);
        parents.Add(startPos, startPos);

        while (open.Count > 0)
        {
            //Sort the open list to get the node with the lowest cost first
            open.Sort((a, b) =>
            { if (costs[a] + DistanceBetweenTiles(a, endPos) == costs[b] + DistanceBetweenTiles(b, endPos)) return 0;
                return costs[a] + DistanceBetweenTiles(a, endPos) > costs[b] + DistanceBetweenTiles(b, endPos) ? 1 : -1; });

            var current = open[0];
            open.RemoveAt(0);
            closed.Add(current);

            //If we have reached the goal, return the path
            if (current == endPos)
            {
                var path = new List<Vector2Int>();
                while (current != startPos)
                {
                    path.Add(current);
                    current = parents[current];
                }

                path.Reverse();
                return path;
            }

            //Add new valid tiles to the open list
            foreach (var neighbor in GetAdjacentMovableTiles(current))
            {
                if (closed.Contains(neighbor))
                {
                    continue;
                }

                if (open.Contains(neighbor))
                {
                    if (costs[neighbor] > costs[current] + 1)
                    {
                        costs[neighbor] = costs[current] + 1;
                        parents[neighbor] = current;
                    }
                }
                else
                {
                    open.Add(neighbor);
                    costs.Add(neighbor, costs[current] + 1);
                    parents.Add(neighbor, current);
                }
            }
        }

        //Return an empty list if no path is found.
        return new List<Vector2Int>();
    }

    public static int ShortestMovableLengthBetweenTile(Vector2Int startPos, Vector2Int endPos)
    {
        return ShortestMovablePathBetweenTiles(startPos, endPos).Count;
    }

    public static bool CanMoveTo(Vector2Int startPos, Vector2Int endPos, int range)
    {
        //Checks if a unit on startPos are able to reach endPos within range spaces, while using available spaces.
        if (startPos == endPos)
        {
            return true;
        }
        
        var length = ShortestMovableLengthBetweenTile(startPos, endPos);
        return length != 0 && length <= range && !IsTileOccupiedByUnit(endPos) && IsTileMovable(endPos);
    }
    
    public static List<UnitController> GetUnitsInAttackRange(Vector2Int startPos, params int[] attackRanges)
    {
        var unitsInRange = new List<UnitController>();
        
        foreach (var unit in GameController.CurrentGameController.units)
        {
            //If the unit is physically there and in attack range, add it to the list
            if (unit.canOccupyTile && attackRanges.Contains(DistanceBetweenTiles(startPos, unit.position)))
                unitsInRange.Add(unit);
        }

        return unitsInRange;
    }

    public static List<Vector2Int> GetAttackableTiles(Vector2Int startPos, int movementRange, params int[] attackRanges)
    {
        var movable = GetMovableTilesInRange(startPos, movementRange);

        var attackable = new List<Vector2Int>();
        
        foreach (var targetTile in GetTilesInRange(startPos, movementRange + attackRanges.Max()))
        {
            if (movable.Exists(movableTile => attackRanges.Contains(DistanceBetweenTiles(movableTile, targetTile))))
            {
                attackable.Add(targetTile);
            }
        }

        return attackable;
    }

    public static Vector2 GridCoordinatesToWorldCoordinates(Vector2 pos)
    {
        return new Vector2(pos.x + 0.5f, pos.y + 0.5f);
    }

    public static Vector2Int WorldCoordinatesToGridCoordinates(Vector2 pos)
    {
        return Vector2Int.FloorToInt(pos);
    }

    public static List<Vector2Int> GetAdjacentTiles(Vector2Int pos)
    {
        var list = new List<Vector2Int>();
        if (pos.x != 0) list.Add(pos + Vector2Int.left);
        if (pos.x != GameController.CurrentGameController.easternBorder - 1) list.Add(pos + Vector2Int.right);
        if (pos.y != 0) list.Add(pos + Vector2Int.down);
        if (pos.y != GameController.CurrentGameController.upperBorder - 1) list.Add(pos + Vector2Int.up);
        return list;
    }

    public static List<Vector2Int> GetAdjacentMovableTiles(Vector2Int pos)
    {
        var list = GetAdjacentTiles(pos).ToList();
        list.RemoveAll(p => !IsTileMovable(p) || IsTileOccupiedByUnit(p));
        return list;
    }
    
    public static List<Vector2Int> GetTilesInRange(Vector2Int pos, int range, bool includeSelf = true)
    {
        //Returns a list of all the tiles in range of pos, ignoring whether they can be moved to
        //Includes the starting position by default, but will not if includeSelf is false
        List<Vector2Int> reachable = new List<Vector2Int>();
        List<Vector2Int> active = new List<Vector2Int>();
        
        reachable.Add(pos);
        active.Add(pos);

        for (int i = 0; i < range; i++)
        {
            int count = active.Count;
            for (int j = 0; j < count; j++)
            {
                foreach (var tile in GetAdjacentTiles(active[0]))
                {
                    if (!reachable.Contains(tile)) reachable.Add(tile);
                    if (!active.Contains(tile)) active.Add(tile);
                }
                active.RemoveAt(0);
            }
        }

        if (!includeSelf) reachable.Remove(pos);

        return reachable;
    }

    public static List<Vector2Int> GetMovableTilesInRange(Vector2Int pos, int range, bool includeSelf = true)
    {
        //Returns a list of all the tiles in range of pos, taking whether they can be moved to into account
        //Includes the starting position by default, but will not if includeSelf is false
        List<Vector2Int> reachable = new List<Vector2Int>();
        List<Vector2Int> active = new List<Vector2Int>();
        
        reachable.Add(pos);
        active.Add(pos);

        for (int i = 0; i < range; i++)
        {
            int count = active.Count;
            for (int j = 0; j < count; j++)
            {
                foreach (var tile in GetAdjacentMovableTiles(active[0]))
                {
                    if (!reachable.Contains(tile)) reachable.Add(tile);
                    if (!active.Contains(tile)) active.Add(tile);
                }
                active.RemoveAt(0);
            }
        }

        if (!includeSelf) reachable.Remove(pos);
        
        return reachable;
    }

    public static List<Vector2Int> GetNeighbors(List<Vector2Int> originalTiles, int range)
    {
        //Finds all tiles within the given range of any of the tiles in the list, excluding the tiles in the given list
        
        var result = new List<Vector2Int>();
        
        foreach (var originalTile in originalTiles)
        {
            foreach (var tile in GetTilesInRange(originalTile, range))
            {
                if (result.Contains(tile)) continue;
                if (originalTiles.Contains(tile)) continue;
                
                result.Add(tile);
            }
        }

        return result;
    }
}

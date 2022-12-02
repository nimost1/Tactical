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
    
    public TerrainObject DefaultTerrainObject;
    public TerrainObject GrassTerrainObject;

    private void Awake()
    {
        DefaultTerrainObject = new TerrainObject(true);
        GrassTerrainObject = new TerrainObject(true);
    }
    
    public static bool IsTileOccupied(Vector2Int pos)
    {
        foreach (var unit in GameController.CurrentGameController.units)
        {
            if (unit.canOccupyTile && unit.position == pos) return true;
        }

        return false;
    }

    /*public static TerrainObject GetTerrainType(Vector2Int pos, Tilemap ground)
    {
        //En løsning er å droppe dette og heller sette opp et ScriptableObject e.l. for hver tile og linke hver tile i
        //tile paletten til det. Da kan man sikkert sjekke scriptet for data.
        
        //En annen løsning kan være å lage funksjoner som sjekker terrengegenskaper basert på posisjon.
        //Kan lage en funksjon som genererer arrays med data på compile og lagrer det i et Scriptable Object.
        
        //Jeg fant en løsning på internett. Se videoen i lenken jeg la til som bokmerke

        Sprite sprite = ground.GetSprite(new Vector3Int(pos.x, pos.y, 0));
        print(sprite.name);

        //TODO: Dette funker ikke, fordi det ser ut som "name" vil gi navnet på tilemappet.

        return DefaultTerrainObject;
        
        switch (sprite.name)
        {
            case "Grass":
                print("Grass");
                return GrassTerrainObject;
            default:
                print("Default");
                return DefaultTerrainObject;
        }
    }*/
    
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
        return length != 0 && length <= range && !IsTileOccupied(endPos) /*&& GetTerrainType(endPos).IsMovable*/;
    }

    public static bool CanAttackMelee(Vector2Int startPos, Vector2Int endPos, int range)
    {
        //Returns true if a one-range attack can hit the given tile
        foreach (var tile in GetAdjacentMovableTiles(endPos))
        {
            if (CanMoveTo(startPos, tile, range))
            {
                return true;
            }
        }

        return false;
    }

    public class TerrainObject
    {
        public bool IsMovable;

        public TerrainObject(bool movable)
        {
            IsMovable = movable;
        }
    }

    public static Vector2 GridCoordinatesToWorldCoordinates(Vector2Int pos)
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
        list.RemoveAll(p => /*!GetTerrainType(p).IsMovable ||*/ IsTileOccupied(p));
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

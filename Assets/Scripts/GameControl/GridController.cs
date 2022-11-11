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
    
    
    private Tilemap _ground;
    private static bool[,] _isOccupied = new bool[10,10];
    
    public TerrainObject DefaultTerrainObject;
    public TerrainObject GrassTerrainObject;

    private void Awake()
    {
        //Find the ground tilemap
        _ground = GameObject.Find("Ground").GetComponent<Tilemap>();

        DefaultTerrainObject = new TerrainObject(true);
        GrassTerrainObject = new TerrainObject(true);
    }
    
    public void AddOccupation(Vector2Int pos)
    {
        _isOccupied[pos.x, pos.y] = true;
    }
    
    public void RemoveOccupation(Vector2Int pos)
    {
        _isOccupied[pos.x, pos.y] = false;
    }

    public bool IsTileOccupied(Vector2Int pos)
    {
        return _isOccupied[pos.x, pos.y];
    }

    public TerrainObject GetTerrainType(Vector2Int pos)
    {
        Sprite sprite = _ground.GetSprite(new Vector3Int(pos.x, pos.y, 0));

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
    }
    
    public UnitController GetUnitOnSpace(Vector2Int pos)
    {
        foreach (var unit in TurnController.Units)
        {
            if (unit.position == pos)
            {
                return unit;
            }
        }

        return null;
    }

    public List<Vector2Int> GetReachableFromTile(Vector2Int startPos, int range)
    {
        List<Vector2Int> reachable = new List<Vector2Int>();
        List<Vector2Int> active = new List<Vector2Int>();
        
        reachable.Add(startPos);
        active.Add(startPos);

        for (int i = 0; i < range; i++)
        {
            int count = active.Count;
            for (int j = 0; j < count; j++)
            {
                foreach (var tile in GetAdjacentMovableTiles(active[0]))
                {
                    reachable.Add(tile);
                    active.Add(tile);
                }
                active.RemoveAt(0);
            }
        }

        return reachable;
    }

    public static int DistanceBetweenTiles(Vector2Int startPos, Vector2Int endPos)
    {
        return Mathf.Abs(startPos.x - endPos.x) + Mathf.Abs(startPos.y - endPos.y);
    }

    public List<Vector2Int> ShortestMovablePathBetweenTiles(Vector2Int startPos, Vector2Int endPos)
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

    public int ShortestMovableLengthBetweenTile(Vector2Int startPos, Vector2Int endPos)
    {
        return ShortestMovablePathBetweenTiles(startPos, endPos).Count;
    }

    public bool CanMoveTo(Vector2Int startPos, Vector2Int endPos, int range)
    {
        //Checks if a unit on startPos are able to reach endPos within range spaces, while using available spaces.
        if (startPos == endPos)
        {
            return true;
        }
        
        var length = GameController.Grid.ShortestMovableLengthBetweenTile(startPos, endPos);
        return length != 0 && length <= range && !IsTileOccupied(endPos) && GetTerrainType(endPos).IsMovable;
    }

    public bool CanAttackMelee(Vector2Int startPos, Vector2Int endPos, int range)
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
        if (pos.x != GameController.EasternBorder - 1) list.Add(pos + Vector2Int.right);
        if (pos.y != 0) list.Add(pos + Vector2Int.down);
        if (pos.y != GameController.UpperBorder - 1) list.Add(pos + Vector2Int.up);
        return list;
    }

    public List<Vector2Int> GetAdjacentMovableTiles(Vector2Int pos)
    {
        var list = GetAdjacentTiles(pos).ToList();
        list.RemoveAll(p => !GetTerrainType(p).IsMovable || _isOccupied[p.x, p.y]);
        return list;
    }
}

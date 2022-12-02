using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public static class UnitAIUtils
{
    public static UnitController SelectClosestUnit(UnitController self, List<UnitController> units)
    {
        //Chooses a random unit from the closest to self in the given list of units.
        int targetDistance = 1000000;
        foreach (var unit in units)
        {
            var distance = GridController.ShortestMovableLengthBetweenTile(self.position, unit.position);
            if (distance < targetDistance)
            {
                targetDistance = distance;
            }
        }

        units = units.FindAll(unit =>
            GridController.ShortestMovableLengthBetweenTile(self.position, unit.position) == targetDistance);
        
        //Pick random unit from list
        return units[Random.Range(0, units.Count)];
    }

    public static Vector2Int FindMovementTargetTowardsUnit(UnitController self, UnitController target)
    {
        //Find movement target tile
        var tileList = GridController.GetMovableTilesInRange(self.position, self.movementRange);
        Vector2Int targetTile = tileList[0];
        int maxTaxiDistance = GridController.DistanceBetweenTiles(target.position, targetTile);
        float maxEuclideanDistance = Vector2Int.Distance(target.position, targetTile);
        float maxSelfDistance = Vector2Int.Distance(self.position, targetTile);
        foreach (var tile in tileList)
        {
            int currentTaxiDistance = GridController.DistanceBetweenTiles(target.position, tile);
            float currentEuclideanDistance = Vector2Int.Distance(target.position, tile);
            float currentSelfDistance = Vector2Int.Distance(self.position, tile);
            if (currentTaxiDistance < maxTaxiDistance || 
                (currentTaxiDistance == maxTaxiDistance && currentEuclideanDistance < maxEuclideanDistance) ||
                (currentTaxiDistance == maxTaxiDistance && currentEuclideanDistance == maxEuclideanDistance &&
                 currentSelfDistance <= maxSelfDistance))
            {
                targetTile = tile;
                maxTaxiDistance = GridController.DistanceBetweenTiles(target.position, targetTile);
                maxEuclideanDistance = Vector2Int.Distance(target.position, targetTile);
                maxSelfDistance = Vector2Int.Distance(self.position, targetTile);
            }
        }

        return targetTile;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveFieldAttribute : Attribute { }

/// <summary>
/// If the SaveField attribute is put on any public field of type int, bool, string or Vector2Int on a UnitController
/// in the current GameController's list of units, this script will save and load them when asked to.
/// </summary>

public class SaveController : MonoBehaviour
{
    //Creates a .txt-file representing the data of every unit.
    public static void Save()
    {
        var save = $"SCENE:{LevelLoadController.CurrentLevelLoadController.currentLevel}\n\n";
        
        foreach (var unit in GameController.CurrentGameController.units)
        {
            save += unit.GetSaveData();
        }

        if (File.Exists(GameController.CurrentGameController.saveSlotName + GameController.CurrentGameController.turnNumber))
        {
            DeleteMoreRecentSaves(GameController.CurrentGameController.turnNumber);
        }

        //Write the string to file
        File.WriteAllText(GameController.CurrentGameController.saveSlotName + GameController.CurrentGameController.turnNumber + ".txt", save);
    }

    /// <summary>
    /// Save file format:
    /// 
    /// Global variables in the following syntax:
    /// TYPE:name:value
    ///
    /// A single line of whitespace before units.
    /// 
    /// Units are declared as follows:
    /// UNIT:filename of the script that defines the unit's behaviour
    /// Variables in the following syntax:
    /// TYPE:name:value
    ///
    /// Note that there should be no whitespace except before units and a new line after every UNIT or variable.
    /// 
    /// </summary>

    public static IEnumerator Load(string path)
    {
        //Retrieve the saved data
        IEnumerable<string> savedData = File.ReadLines(path);
        Console.WriteLine(String.Join(Environment.NewLine, savedData));

        //Reads the save file
        using (StreamReader reader = File.OpenText(path))
        {
            //Read and load the scene
            var line = reader.ReadLine();
    
            if (line == null) yield break;
    
            var splitLine = SplitLineAndRemoveNewline(line);
    
            if (splitLine.Length != 2 || splitLine[0] != "SCENE") yield break;
    
            //Load the scene with the given name.
            LevelLoadController.CurrentLevelLoadController.LoadLevel(splitLine[1]);

            yield return LevelLoadController.CurrentLevelLoadController.WaitForLevelToLoad();

            var notYetInitializedUnits = GameController.CurrentGameController.units.ToList();
            
            //Reads the whitespace line above the units
            reader.ReadLine();
    
            //Reads the units
            while (!reader.EndOfStream)
            {
                ReadUnit(reader, ref notYetInitializedUnits);
            }
    
            //If any units does not exist in the save file, they should not be in the scene and will be destroyed.
            foreach (var unit in notYetInitializedUnits)
            {
                TurnController.RemoveUnitFromUnitList(unit);
            }
            
            TurnController.StartPlaying();
        }
    }

    private static void ReadUnit(StreamReader reader, ref List<UnitController> notYetInitializedUnits)
    {
        //Set up the unit
        var line = reader.ReadLine();

        if (line == null) return;

        var splitLine = SplitLineAndRemoveNewline(line);

        if (splitLine.Length != 2 || splitLine[0] != "UNIT") return;

        var currentUnit = FindOrInstantiateUnit(splitLine[1], ref notYetInitializedUnits);
        
        while ((line = reader.ReadLine()) != null)
        {
            //Set the unit's variables
            splitLine = SplitLineAndRemoveNewline(line);

            if (splitLine.Length != 3) break;
            
            var field = currentUnit.GetType().GetField(splitLine[1]);
            
            if (field == null || field.GetCustomAttributes(typeof(SaveFieldAttribute), true).Length == 0) continue;

            switch (splitLine[0])
            {
                case "INT":
                    if (int.TryParse(splitLine[2], out var intValue))
                    {
                        field.SetValue(currentUnit, intValue);
                    }
                    break;
                case "BOOL":
                    if (bool.TryParse(splitLine[2], out var boolValue))
                    {
                        field.SetValue(currentUnit, boolValue);
                    }
                    break;
                case "STRING":
                    field.SetValue(currentUnit, splitLine[2]);
                    break;
                case "VECTOR2INT":
                    if (TryParseVector2Int(splitLine[2], out var vectorValue))
                    {
                        field.SetValue(currentUnit, vectorValue);
                    }
                    break;
            }
        }

        currentUnit.UpdateAfterLoad();
    }

    public static void LoadMostRecentSave()
    {
        if (!File.Exists(GameController.CurrentGameController.saveSlotName + "1.txt"))
        {
            //The save is clean and we start a completely new game.
            LevelLoadController.CurrentLevelLoadController.StartCoroutine(LoadNewGame());
            GameController.CurrentGameController.turnNumber = 0;
            return;
        }

        var highestTurnNumber = 1;
        while (File.Exists(GameController.CurrentGameController.saveSlotName + highestTurnNumber + ".txt"))
        {
            highestTurnNumber++;
        }

        highestTurnNumber--;
        
        LevelLoadController.CurrentLevelLoadController.StartCoroutine(Load(GameController.CurrentGameController.saveSlotName + highestTurnNumber + ".txt"));

        GameController.CurrentGameController.turnNumber = highestTurnNumber;
    }

    public static IEnumerator LoadNewGame()
    { 
        LevelLoadController.CurrentLevelLoadController.LoadLevel(LevelLoadController.CurrentLevelLoadController.firstLevel);

        yield return LevelLoadController.CurrentLevelLoadController.WaitForLevelToLoad();
        
        TurnController.SortUnits(GameController.CurrentGameController.units);

        TurnController.StartPlaying();
    }

    public static void DeleteMoreRecentSaves(int turnToDeleteFrom)
    {
        //Deletes all files relating to turns after the turnToDeleteFrom, i.e., turnToDeleteFrom is not deleted.
        //If a file is missing, the code stops and assumes that this is the end.

        var turnNumberOffset = 1;
        while (File.Exists(GameController.CurrentGameController.saveSlotName + (turnToDeleteFrom + turnNumberOffset)))
        {
            turnNumberOffset++;
            File.Delete(GameController.CurrentGameController.saveSlotName + (turnToDeleteFrom + turnNumberOffset));
        }
        
    }

    //Helper functions
    private static UnitController FindOrInstantiateUnit(string scriptToAttach, ref List<UnitController> notYetInitiatedUnits)
    {
        foreach (var unit in notYetInitiatedUnits)
        {
            if (unit.GetType().Name == scriptToAttach)
            {
                notYetInitiatedUnits.Remove(unit);
                print("Initializing existing unit: " + unit.GetType().Name);
                return unit;
            }
        }
        
        var instantiatedUnit = (UnitController)Instantiate(GameController.CurrentGameController.unitBase).AddComponent(Type.GetType(scriptToAttach));
        
        print("Initializing instantiated unit: " + instantiatedUnit.GetType().Name);
        
        GameController.CurrentGameController.units.Add(instantiatedUnit);
        
        return instantiatedUnit;
    }

    private static string[] SplitLineAndRemoveNewline(string line)
    {
        return line.Replace("\n", "").Replace("\r", "").Split(':');
    }

    private static bool TryParseVector2Int(string toParse, out Vector2Int result)
    {
        result = new Vector2Int(0, 0);
        if (toParse == "" || toParse[0] != '(' || toParse[^1] != ')')
        {
            Debug.LogError("Format of Vector2Int in save file is wrong.");
            return false;
        }

        toParse = toParse.Replace("(", "").Replace(")", "").Replace(" ", "");
        var values = toParse.Split(',');
        
        if (int.TryParse(values[0], out var x) && int.TryParse(values[1], out var y))
        {
            result = new Vector2Int(x, y);
            return true;
        }
        else
        {
            Debug.LogError("Format of Vector2Int in save file is wrong. Could not parse integer values.");
            return false;
        }
    }
}
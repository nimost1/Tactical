using System;
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
    public static void Save(string path)
    {
        var save = "";
        
        foreach (var unit in GameController.CurrentGameController.units)
        {
            save += unit.GetSaveData();
        }

        //Write the string to file
        File.WriteAllText(path, save);
    }
    
    [Serializable]
    private class SaveObject
    {
        public int[] units;
    }
    
    /// <summary>
    /// Save file format:
    /// 
    /// Global variables in the following syntax:
    /// TYPE:name:value
    ///
    /// Units are declared as follows:
    /// UNIT:filename of the script that defines the unit's behaviour
    /// Variables in the following syntax:
    /// TYPE:name:value
    ///
    /// Note that there should be no whitespace and a new line after every UNIT or variable.
    /// 
    /// </summary>
    
    public static void Load(string path)
    {
        //Retrieve the saved data
        IEnumerable<string> savedData = File.ReadLines(path);
        Console.WriteLine(String.Join(Environment.NewLine, savedData));
        
        var notYetInitiatedUnits = GameController.CurrentGameController.units.ToList();

        //Set global variables and such
        //Go to next stage when we encounter a "UNIT"
        using (StreamReader reader = File.OpenText("testSave.txt"))
        {
            string line = "";
            string[] splitLine;
            
            while ((line = reader.ReadLine()) != null)
            {
                splitLine = SplitLineAndRemoveNewline(line);

                if (splitLine.Length == 3)
                {
                    //Set global variables and such
                }
                else if (splitLine.Length == 2 && splitLine[0] == "UNIT")
                {
                    break;
                }
                else
                {
                    Debug.LogError("Loaded line does not match the rule of starting with \"UNIT\" right after variables.");
                    return;
                }
            }
            
            splitLine = SplitLineAndRemoveNewline(line);
            
            if (line == null)
            {
                Debug.LogError("Save file ended without defining any units.");
                return;
            }
            
            var currentUnit = FindOrInstantiateUnit(splitLine[1], ref notYetInitiatedUnits);
            
            //Has found a line that does not have two colons, meaning to move on to the units

            while ((line = reader.ReadLine()) != null)
            {
                //Set local variables
                splitLine = SplitLineAndRemoveNewline(line);

                if (splitLine.Length == 2)
                {
                    switch (splitLine[0])
                    {
                        case "UNIT":
                            currentUnit = FindOrInstantiateUnit(splitLine[1], ref notYetInitiatedUnits);
                            break;
                    }
                }
                else if (splitLine.Length == 3)
                {
                    var field = currentUnit.GetType().GetField(splitLine[1]);
                    
                    if (field == null || field.GetCustomAttributes(typeof(SaveFieldAttribute), true).Length == 0)
                    {
                        continue;
                    }
                    
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
            }

            //If any units does not exist in the save file, they should not be there and will be destroyed.
            foreach (var unit in notYetInitiatedUnits)
            {
                TurnController.RemoveUnitFromUnitList(unit);
            }
        }
        
        //TODO: Update the values shown to the player of any variable that has been set in code.
        //For example, as it is now, any position changes will be set in the script, but the GameObject's transform will not change.
        //Might be as simple as just "running an update", i.e., update position and everything after setting all the variables.
    }
    
    //Helper functions
    private static UnitController FindOrInstantiateUnit(string scriptToAttach, ref List<UnitController> notYetInitiatedUnits)
    {
        foreach (var unit in notYetInitiatedUnits)
        {
            if (unit.GetType().Name == scriptToAttach)
            {
                notYetInitiatedUnits.Remove(unit);
                return unit;
            }
        }
        
        var instantiatedUnit = (UnitController)Instantiate(GameController.CurrentGameController.unitBase).AddComponent(Type.GetType(scriptToAttach));

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    //Need to figure out which kinds of menu options are necessary.
    public enum MenuOptions
    {
        
    }

    public void CreateMenu(Dictionary<MenuOptions, string> buttons)
    {
        foreach (var button in buttons)
        {
            //Spawn button calling a function based on the MenuOption
            //Potential problem: No arguments to functions
            continue;
        }
    }
}

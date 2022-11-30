using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadController : MonoBehaviour
{
    public static LoadController CurrentLoadController;
    public string firstLevel;
    private string _currentLevel;
    
    private LoadSceneParameters _additive = new LoadSceneParameters(LoadSceneMode.Additive);

    private void Awake()
    {
        if (CurrentLoadController == null)
        {
            CurrentLoadController = this;
        }
        else
        {
            Destroy(this);
        }

        _currentLevel = SceneManager.LoadScene(firstLevel, _additive).name;
    }

    public void LoadLevel(string levelName)
    {
        //Loads the given level, unloads the current level and sets the new level as the active level.
        
        //Ville det vært en bedre løsning å sette opp ScriptableObjects for å gi litt initialization-info om levelen?
        //Eller skal man sette opp basert på save-state?
        if (_currentLevel != "") SceneManager.UnloadSceneAsync(_currentLevel);
        _currentLevel = levelName;
        var scene = SceneManager.LoadScene(levelName, _additive);
        SceneManager.SetActiveScene(scene);
    }
}

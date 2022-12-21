using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoadController : MonoBehaviour
{
    public static LevelLoadController CurrentLevelLoadController;
    public bool isLoadingNewScene;
    
    [SerializeField] private string _firstLevel;
    private string _currentLevel = "";
    
    
    private LoadSceneParameters _additive = new LoadSceneParameters(LoadSceneMode.Additive);

    private void Awake()
    {
        if (CurrentLevelLoadController == null)
        {
            CurrentLevelLoadController = this;
        }
        else
        {
            Destroy(this);
        }


        StartCoroutine(LoadLevel(_firstLevel));
    }

    public IEnumerator LoadLevel(string levelName)
    {
        //Loads the given level, unloads the current level and sets the new level as the active level.

        isLoadingNewScene = true;
        //Ville det vært en bedre løsning å sette opp ScriptableObjects for å gi litt initialization-info om levelen?
        //Eller skal man sette opp basert på save-state?
        var loadOperation = SceneManager.LoadSceneAsync(levelName, _additive);

        while (!loadOperation.isDone)
        {
            yield return null;
        }
        
        if (_currentLevel != "") SceneManager.UnloadSceneAsync(_currentLevel);
        
        var scene = SceneManager.GetSceneByName(levelName);

        SceneManager.SetActiveScene(scene);
        
        _currentLevel = levelName;
        isLoadingNewScene = false;
        SaveController.Save("testSave.txt");
    }
}

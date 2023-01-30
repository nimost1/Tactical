using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoadController : MonoBehaviour
{
    public static LevelLoadController CurrentLevelLoadController;
    public bool isLoadingNewScene;
    
    public string firstLevel;
    public string currentLevel = "";

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


        //StartCoroutine(LoadLevel(_firstLevel));
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadLevel(string levelName)
    {
        //Loads the given level.
        SceneManager.LoadSceneAsync(levelName, _additive);
        isLoadingNewScene = true;
    }

    public IEnumerator WaitForLevelToLoad()
    {
        while (isLoadingNewScene)
        {
            yield return null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == "Loader")
        {
            return;
        }
        //Unload the previous level and sets the new level as the active level.
        if (currentLevel != "") SceneManager.UnloadSceneAsync(currentLevel);
        
        SceneManager.SetActiveScene(scene);
        GameController.CurrentGameController.FindGroundTilemap();

        var playerTransform = GameObject.Find("Player").transform;
        GameController.CurrentGameController.virtualCamera.transform.position =
            new Vector3(playerTransform.position.x, playerTransform.position.y, -10);
        GameController.CurrentGameController.virtualCamera.Follow = playerTransform;
        
        currentLevel = scene.name;
        isLoadingNewScene = false;
    }
}

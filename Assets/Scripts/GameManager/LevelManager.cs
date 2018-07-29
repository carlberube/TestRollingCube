using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour {

    public LevelObject currentLevel;
    public int levelNumber;
    public List<LevelObject> allLevels;

    public UnityEvent onLevelLoaded;
    public UnityEvent onLevelStarted;
    private Scene currentScene;

    private static List<LevelObject> GetAllLevels()
    {
        string[] guids;
        List<LevelObject> levelObjects = new List<LevelObject>();
        guids = AssetDatabase.FindAssets("t:LevelObject");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LevelObject level = (LevelObject)AssetDatabase.LoadAssetAtPath(path, typeof(LevelObject));
            levelObjects.Add(level);
        }
        return levelObjects;
    }

    // Use this for initialization
    void OnEnable ()
    {
        allLevels = GetAllLevels();
    }

    public void StartLevel()
    {
        onLevelStarted.Invoke();
    }

    public void LoadLevel()
    {
        currentScene = SceneManager.GetActiveScene();
        if (currentLevel != null)
        {
            SceneManager.UnloadSceneAsync(currentLevel.levelPath);
        }
        currentLevel = null;
        foreach(LevelObject level in allLevels)
        {
            if(level.levelNumber == levelNumber)
            {
                currentLevel = level;
            }
        }
        if(currentLevel == null)
        {
            return;
        }
        SceneManager.LoadSceneAsync(currentLevel.levelPath, LoadSceneMode.Additive);
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Level Loaded");
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
        SceneManager.SetActiveScene(scene);
        onLevelLoaded.Invoke();
        SceneManager.SetActiveScene(currentScene);
    }

}

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelManager levelObject = (LevelManager)target;
        if (GUILayout.Button("Load Level"))
        {
            levelObject.LoadLevel();
        }
        if(levelObject.currentLevel == null)
        {
            GUI.enabled = false;
        }
        if (GUILayout.Button("Start Level"))
        {
            levelObject.StartLevel();
        }
        GUI.enabled = true;
    }
}

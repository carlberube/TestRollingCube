using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController {

    public int objective;
    public int level_number = 0;
    public Dictionary<int, string> scene_for_level_number = new Dictionary<int, string>();

    private static string current_scene_path;
    public GameGrid game_grid;

    // Use this for initialization
    public LevelController() {
        scene_for_level_number[9999] = "Assets\\Scenes\\Levels\\TestLevel\\";
    }

    private string GetLatestLevel(string level_path)
    {
        string path = Directory.GetCurrentDirectory();
        string full_path = Path.Combine(path, level_path);
        if (Directory.Exists(full_path))
        {
            string[] fileEntries = Directory.GetFiles(level_path);
            int max_version = 0;
            string latest_version_path = "";
            foreach (string fileName in fileEntries)
            {
                string base_name;
                base_name = Path.GetFileNameWithoutExtension(fileName);
                if (base_name.Contains("unity"))
                {
                    continue;
                }
                string[] splitted = base_name.Split('_');
                string str_version = splitted[splitted.Length - 1];
                str_version = str_version.Replace("v", "");
                int version = Int32.Parse(str_version);
                if(version > max_version)
                {
                    latest_version_path = fileName;
                }
            }
            latest_version_path = Path.Combine(level_path, Path.GetFileNameWithoutExtension(latest_version_path));
            latest_version_path = latest_version_path.Replace("Assets\\", "");
            latest_version_path = latest_version_path.Replace("\\", "/");
            return latest_version_path;
        }
        else
        {
            return "";
        }
    }
	
	// Update is called once per frame
    public string LoadLevelAndGetPath(int number)
    {
        level_number = number;
        Debug.Log("LEVEL NUMBER : " + level_number);
        if (!scene_for_level_number.ContainsKey(level_number))
        {
            Debug.Log("NO LEVEL " + level_number);
            return null;
        }
        string path = GetLatestLevel(scene_for_level_number[level_number]);
        if (path == "")
        {
            return null;
        }

        Debug.Log("PATH : " + path);

        // Only specifying the sceneName or sceneBuildIndex will load the Scene with the Single mode
        if (current_scene_path != null)
        {
            SceneManager.UnloadSceneAsync(current_scene_path);
        }
        SceneManager.LoadScene(path, LoadSceneMode.Additive);
        return path;
    }

    public GameGrid GetGridForScenePath(string path)
    {
        Scene current_scene = SceneManager.GetActiveScene();
        path = "Assets/" + path + ".unity";
        Scene new_scene = SceneManager.GetSceneByPath(path);
        SceneManager.SetActiveScene(new_scene);
        GameObject[] tile_anchors = GameObject.FindGameObjectsWithTag("BoxTileAnchor");
        GameGrid grid = new GameGrid(tile_anchors, tile_anchors[0]);
        SceneManager.SetActiveScene(current_scene);
        return grid;
    }
}

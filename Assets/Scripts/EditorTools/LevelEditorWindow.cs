using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Linq;



public class LevelEditorWindow : EditorWindow
{
    CubeNet[] cubeNets;
    SerializedObject[] objectives;

    bool unfoldedCubeNets = true;

    GameObject tileAnchorPrefab;

    public bool existingLevelLoaded = false;
    public LevelObject levelToEdit = null;
    public Scene activeScene;
    public Scene new_scene;
    public Scene levelScene;
    public string levelPath;
    public int selectedIndex;
    public int previousSelectedIndex;
    public Dictionary<string, GameObject> labelForCubeNetObject;
    public Dictionary<SerializedObject, bool> toggleForObjective;
    public Dictionary<CubeNet, Texture2D> textureForCubeNet;
    public Dictionary<SerializedObject, UnityEngine.Object> serializedObjectForInstance;

    public string levelName = "New Level";


    [MenuItem("Window/Level Publisher")]
    static void Init()
    {
        LevelEditorWindow window = (LevelEditorWindow)GetWindow(typeof(LevelEditorWindow));
        window.Show();
    }

    public void OnEnable()
    {
        InitVars();
        activeScene = EditorSceneManager.GetActiveScene();
        selectedIndex = 0;
        textureForCubeNet = new Dictionary<CubeNet, Texture2D>();
        foreach (CubeNet cubeNet in cubeNets)
        {
            Sprite sprite = cubeNet.thumbnailSprite;
            var croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            var pixels = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                  (int)sprite.textureRect.y,
                                                  (int)sprite.textureRect.width,
                                                  (int)sprite.textureRect.height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();
            textureForCubeNet[cubeNet] = croppedTexture;
        }
    }

    public void InitVars()
    {
        toggleForObjective = new Dictionary<SerializedObject, bool>();
        labelForCubeNetObject = new Dictionary<string, GameObject>();        
        tileAnchorPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/BoxTileAnchor.prefab", typeof(GameObject));
        cubeNets = GetAllCubeNets();
        objectives = GetAllObjectives();
    }

    public void InitFromLevel(LevelObject levelToEdit)
    {
        if (existingLevelLoaded) return;
        bool loaded = false;
        objectives = GetAllObjectives(levelToEdit);
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            Scene scene = EditorSceneManager.GetSceneAt(i);
            if(scene.path == levelToEdit.levelPath)
            {
                loaded = true;
                if(scene != new_scene)
                {
                    EditorSceneManager.CloseScene(new_scene, true);
                    new_scene = scene;
                }
            }
        }
        if (!loaded)
        {
            levelScene = EditorSceneManager.OpenScene(levelToEdit.levelPath, OpenSceneMode.Additive);
            labelForCubeNetObject = new Dictionary<string, GameObject>();
            foreach (GameObject root in levelScene.GetRootGameObjects())
            {
                labelForCubeNetObject[root.name] = root;
            }
            existingLevelLoaded = true;
        }

    }


    public void Awake()
    {
        Debug.Log("Awake");
    }

    private static List<LevelObject> GetAllLevels()
    {
        string[] guids;
        List<LevelObject> levels = new List<LevelObject>();
        guids = AssetDatabase.FindAssets("t:LevelObject");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LevelObject level = (LevelObject)AssetDatabase.LoadAssetAtPath(path, typeof(LevelObject));
            levels.Add(level);
        }
        return levels;
    }

    private static CubeNet[] GetAllCubeNets()
    {
        List<CubeNet> cubeNets = new List<CubeNet>();
        string[] guids;
        guids = AssetDatabase.FindAssets("t:CubeNet");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CubeNet cubeNet = (CubeNet)AssetDatabase.LoadAssetAtPath(path, typeof(CubeNet));
            cubeNets.Add(cubeNet);
        }
        return cubeNets.ToArray();
    }

    private SerializedObject[] GetAllObjectives(LevelObject existingLevel=null)
    {
        serializedObjectForInstance = new Dictionary<SerializedObject, UnityEngine.Object>();
        string path = "";
        TotalRollsObjective totalRollInstance = null;
        StarCollectionObjective starCollectionInstance = null;
        TotalCubesObjective totalCubeInstance = null;
        TimeLimitObjective timeLimitInstance = null;

        if (existingLevel)
        {
            foreach(ObjectiveObject objective in existingLevel.objectives)
            {
                if(objective.GetType() == typeof(TotalRollsObjective))
                {
                    totalRollInstance = objective as TotalRollsObjective;
                }
                else if(objective.GetType() == typeof(StarCollectionObjective))
                {
                    starCollectionInstance = objective as StarCollectionObjective;
                }
                else if(objective.GetType() == typeof(TotalCubesObjective))
                {
                    totalCubeInstance = objective as TotalCubesObjective;
                }
                else if(objective.GetType() == typeof(TimeLimitObjective))
                {
                    timeLimitInstance = objective as TimeLimitObjective;
                }
            }
        }
        if(totalRollInstance == null)
        {
            totalRollInstance = CreateInstance<TotalRollsObjective>();
        }
        path = "Assets/Objectives/TotalRollsObjective/TotalRollsObjectiveTrackerObject.asset";
        totalRollInstance.trackerObject = (GenericObjectiveTracker)AssetDatabase.LoadAssetAtPath(path, typeof(GenericObjectiveTracker));
        serializedObjectForInstance[new SerializedObject(totalRollInstance)] = totalRollInstance;
        if(starCollectionInstance == null)
        {
            starCollectionInstance = CreateInstance<StarCollectionObjective>();
        }
        path = "Assets/Objectives/StarCollectionObjective/StarCollectionObjectiveTrackerObject.asset";
        starCollectionInstance.trackerObject = (GenericObjectiveTracker)AssetDatabase.LoadAssetAtPath(path, typeof(GenericObjectiveTracker));
        serializedObjectForInstance[new SerializedObject(starCollectionInstance)] = starCollectionInstance;

        if (totalCubeInstance == null)
        {
            totalCubeInstance = CreateInstance<TotalCubesObjective>();
        }
        path = "Assets/Objectives/TotalCubesObjective/TotalCubesObjectiveTrackerObject.asset";
        totalCubeInstance.trackerObject = (GenericObjectiveTracker)AssetDatabase.LoadAssetAtPath(path, typeof(GenericObjectiveTracker));
        serializedObjectForInstance[new SerializedObject(totalCubeInstance)] = totalCubeInstance;
        if (timeLimitInstance == null)
        {
            timeLimitInstance = CreateInstance<TimeLimitObjective>();
        }
        path = "Assets/Objectives/TimeLimitObjective/TimeLimitObjectiveTrackerObject.asset";
        timeLimitInstance.trackerObject = (GenericObjectiveTracker)AssetDatabase.LoadAssetAtPath(path, typeof(GenericObjectiveTracker));
        serializedObjectForInstance[new SerializedObject(timeLimitInstance)] = timeLimitInstance;

        return serializedObjectForInstance.Keys.ToArray();
    }

    void CreateCubeNet(CubeNet cubeNet)
    {
        if (levelScene.IsValid())
        {
            new_scene = levelScene;
        }
        if (!new_scene.IsValid())
        {
            new_scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        }
        EditorSceneManager.SetActiveScene(new_scene);
        string label = cubeNet.name;
        while (labelForCubeNetObject.ContainsKey(label))
        {
            int index = 1;
            label = label + "_" + index.ToString();
            index++;
        }

        GameObject parentGameObject = new GameObject();
        parentGameObject.AddComponent<EditModeSnapController>();
        parentGameObject.name = label;
        foreach (Vector3 pos in cubeNet.vectors)
        {
            GameObject tileAnchor = (GameObject) PrefabUtility.InstantiatePrefab(tileAnchorPrefab);
            tileAnchor.transform.SetPositionAndRotation(pos, Quaternion.identity);
            tileAnchor.transform.SetParent(parentGameObject.transform);
        }
        labelForCubeNetObject[label] = parentGameObject;
        GameObject[] selection = new GameObject[1];
        selection[0] = parentGameObject;
        Selection.objects = selection;
        EditorSceneManager.SetActiveScene(activeScene);
    }

    void OnSceneChanged(int levelNumber)
    {

    }

    string[] GetLevelNames()
    {
        List<string> levelNames = new List<string>();
        levelNames.Add("New Level");
        foreach (LevelObject level in GetAllLevels())
        {
            levelNames.Add(level.levelName);
        }
        return levelNames.ToArray();
    }


    void OnGUI()
    {
        string[] levelNames = GetLevelNames();
        GUILayout.Label("Level Publisher", EditorStyles.boldLabel);

        selectedIndex = EditorGUILayout.Popup("Levels", selectedIndex, levelNames);
        previousSelectedIndex = selectedIndex;
        if(selectedIndex != 0)
        {
            levelName = levelNames[selectedIndex];
        }

        levelName = GUILayout.TextField(levelName);

        if (levelToEdit)
        {
            if (selectedIndex == 0 || levelToEdit.levelName != levelName)
            {
                levelToEdit = null;
                existingLevelLoaded = false;
                objectives = GetAllObjectives();
                labelForCubeNetObject = new Dictionary<string, GameObject>();
                if (levelScene.IsValid())
                {
                    EditorSceneManager.CloseScene(levelScene, true);
                }
            }
        }
        if(levelToEdit == null && selectedIndex != 0)
        {
            foreach (LevelObject level in GetAllLevels())
            {
                if(level.levelName == levelName)
                {
                    levelToEdit = level;
                    break;
                }
            }
            if (levelToEdit)
            {
                InitFromLevel(levelToEdit);
            }
        }
        GUILayout.Label("Create Cube Nets", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        foreach (CubeNet cubeNet in cubeNets)
        {
            Texture2D croppedTexture = textureForCubeNet[cubeNet];
            
            if (GUILayout.Button(croppedTexture, GUILayout.Width(40), GUILayout.Height(40)))
            {
                CreateCubeNet(cubeNet);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("Current Cube Nets", EditorStyles.boldLabel);
        unfoldedCubeNets = EditorGUILayout.Foldout(unfoldedCubeNets, "Current Cube Nets");
        string keyToDelete = "";
        if (unfoldedCubeNets)
        {
            foreach (string label in labelForCubeNetObject.Keys)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label);
                if (GUILayout.Button("Select"))
                {
                    GameObject parentObject = labelForCubeNetObject[label];
                    GameObject[] selection = new GameObject[1];
                    selection[0] = parentObject;
                    Selection.objects = selection;
                }
                if (GUILayout.Button("Remove"))
                {
                    keyToDelete = label;
                    DestroyImmediate(labelForCubeNetObject[label]);
                }
                EditorGUILayout.EndHorizontal();
            }
            if (keyToDelete != "")
            {
                labelForCubeNetObject.Remove(keyToDelete);
            }
        }

        foreach (SerializedObject objective in objectives)
        {
            bool toggled = false;
            if (toggleForObjective.ContainsKey(objective))
            {
                toggled = toggleForObjective[objective];
            }
            if (levelToEdit)
            {
                foreach(ObjectiveObject existingObjective in levelToEdit.objectives)
                {
                    if(objective.targetObject == existingObjective)
                    {
                        toggled = true;
                    }
                }
            }
            SerializedProperty props = objective.GetIterator();
            toggled = EditorGUILayout.BeginToggleGroup(objective.targetObject.ToString(), toggled);
            toggleForObjective[objective] = toggled;
            while (props.NextVisible(true))
            {
                if(objective.targetObject.GetType() == typeof(TotalCubesObjective))
                {
                    if(props.name == "totalCubes" && levelToEdit == null)
                    {
                        props.intValue = labelForCubeNetObject.Keys.Count;
                    }
                }
                if(props.name == "m_Script")
                {
                    continue;
                }
                EditorGUILayout.PropertyField(props, true);
            }
            EditorGUILayout.EndToggleGroup();
            objective.ApplyModifiedProperties();
        }
        if (GUILayout.Button("Publish Level"))
        {
            string path = "Assets/Levels";
            string guid = AssetDatabase.CreateFolder(path, String.Format("Level_{0}", levelName));
            path = AssetDatabase.GUIDToAssetPath(guid);
            string levelAssetPath = path + "/LevelObject";
            string assetPathAndName = levelAssetPath + ".asset";
            LevelObject newLevel = CreateInstance<LevelObject>();
            newLevel.levelName = levelName;

            string scenesPath = "Assets/Scenes";
            string newSceneGuid = AssetDatabase.CreateFolder(scenesPath, String.Format("Level_{0}", levelName));
            scenesPath = AssetDatabase.GUIDToAssetPath(newSceneGuid);
            EditorSceneManager.SaveScene(new_scene, scenesPath + "/LevelScene.unity");
            newLevel.levelPath = new_scene.path;
            bool inEditorSettings = false;
            foreach(EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if(scene.path == new_scene.path)
                {
                    inEditorSettings = true;
                }
            }
            if (!inEditorSettings)
            {
                List<EditorBuildSettingsScene> allScenes = EditorBuildSettings.scenes.ToList<EditorBuildSettingsScene>();
                allScenes.Add(new EditorBuildSettingsScene(new_scene.path, true));
                EditorBuildSettings.scenes = allScenes.ToArray();
            }
            foreach(KeyValuePair<string, GameObject> entry in labelForCubeNetObject)
            {
                foreach (Transform child in entry.Value.transform)
                {
                    Vector3 new_pos = child.position;
                    new_pos.x = RoundToNearestHalf(new_pos.x);
                    new_pos.y = RoundToNearestHalf(new_pos.y);
                    new_pos.z = RoundToNearestHalf(new_pos.z);
                    child.position = new_pos;
                }
            }
            newLevel.cubeNetNames = labelForCubeNetObject.Keys.ToArray();
            List<ObjectiveObject> toggledObjectives = new List<ObjectiveObject>();
            foreach (SerializedObject objective in objectives)
            {
                if (toggleForObjective.ContainsKey(objective))
                {
                    if (toggleForObjective[objective])
                    {
                        ObjectiveObject instance = serializedObjectForInstance[objective] as ObjectiveObject;
                        string objectiveName = instance.ToString().Replace(" (", "").Replace(")", "");
                        string objectiveAssetPath = path + "/" + objectiveName + ".asset";
                        AssetDatabase.CreateAsset(instance, objectiveAssetPath);
                        ObjectiveObject objectiveAsset = AssetDatabase.LoadAssetAtPath(objectiveAssetPath, typeof(ObjectiveObject)) as ObjectiveObject;
                        toggledObjectives.Add(objectiveAsset);
                    }
                }
            }
            newLevel.objectives = toggledObjectives.ToArray();
            AssetDatabase.CreateAsset(newLevel, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            EditorSceneManager.CloseScene(new_scene, true);
            InitVars();
        }
    }

    public static float RoundToNearestHalf(float a)
    {
        return a = Mathf.Round(a * 2f) * 0.5f;
    }
}

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Linq;



public class LevelEditorWindow : EditorWindow
{
    int[] levelNumbers;
    CubeNet[] cubeNets;
    SerializedObject[] objectives;

    bool unfoldedCubeNets = true;

    GameObject tileAnchorPrefab;

    public int levelNumber;

    public Scene activeScene;
    public Scene new_scene;
    public string levelPath;
    public int selectedIndex;
    public Dictionary<string, List<GameObject>> labelForCubeNetObjects;
    public Dictionary<SerializedObject, bool> toggleForObjective;
    public Dictionary<CubeNet, Texture2D> textureForCubeNet;
    public Dictionary<SerializedObject, UnityEngine.Object> serializedObjectForInstance;


    [MenuItem("Window/Level Publisher")]
    static void Init()
    {
        LevelEditorWindow window = (LevelEditorWindow)GetWindow(typeof(LevelEditorWindow));
        window.Show();
    }

    public void OnEnable()
    {
        toggleForObjective = new Dictionary<SerializedObject, bool>();
        labelForCubeNetObjects = new Dictionary<string, List<GameObject>>();
        textureForCubeNet = new Dictionary<CubeNet, Texture2D>();
        serializedObjectForInstance = new Dictionary<SerializedObject, UnityEngine.Object>();
        tileAnchorPrefab = (GameObject) AssetDatabase.LoadAssetAtPath("Assets/BoxTileAnchor.prefab", typeof(GameObject));
        levelNumbers = GetAllLevels();
        cubeNets = GetAllCubeNets();
        objectives = GetAllObjectives();
        activeScene = EditorSceneManager.GetActiveScene();

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

    public void Awake()
    {
        Debug.Log("Awake");
    }

    private static int[] GetAllLevels()
    {
        string[] guids;
        List<int> level_numbers = new List<int>();
        guids = AssetDatabase.FindAssets("t:LevelObject");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LevelObject level = (LevelObject) AssetDatabase.LoadAssetAtPath(path, typeof(LevelObject));
            level_numbers.Add(level.levelNumber);
        }
        return level_numbers.ToArray();
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

    private SerializedObject[] GetAllObjectives()
    {
        TotalRollsObjective totalRollInstance = CreateInstance<TotalRollsObjective>();
        serializedObjectForInstance[new SerializedObject(totalRollInstance)] = totalRollInstance;

        StarCollectionObjective starCollectionInstance = CreateInstance<StarCollectionObjective>();
        serializedObjectForInstance[new SerializedObject(starCollectionInstance)] = starCollectionInstance;

        TotalCubesObjective totalCubeInstance = CreateInstance<TotalCubesObjective>();
        serializedObjectForInstance[new SerializedObject(totalCubeInstance)] = totalCubeInstance;

        TimeLimitObjective timeLimitInstance = CreateInstance<TimeLimitObjective>();
        serializedObjectForInstance[new SerializedObject(timeLimitInstance)] = timeLimitInstance;

        return serializedObjectForInstance.Keys.ToArray();
    }

    void CreateCubeNet(CubeNet cubeNet)
    {
        if (!new_scene.IsValid())
        {
            new_scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        }
        EditorSceneManager.SetActiveScene(new_scene);
        string label = cubeNet.name;
        while (labelForCubeNetObjects.ContainsKey(label))
        {
            int index = 1;
            label = label + "_" + index.ToString();
            index++;
        }
        labelForCubeNetObjects[label] = new List<GameObject>();
        foreach (Vector3 pos in cubeNet.vectors)
        {
            GameObject tileAnchor = (GameObject) PrefabUtility.InstantiatePrefab(tileAnchorPrefab);
            tileAnchor.transform.SetPositionAndRotation(pos, Quaternion.identity);
            labelForCubeNetObjects[label].Add(tileAnchor);
        }
        Selection.objects = labelForCubeNetObjects[label].ToArray();
        EditorSceneManager.SetActiveScene(activeScene);
    }

    void OnGUI()
    {
        List<string> levelNames = new List<string>();
        foreach(int levelNumber in levelNumbers)
        {
            levelNames.Add(levelNumber.ToString());
        }
        levelNames.Add("New Level");
        GUILayout.Label("Level Publisher", EditorStyles.boldLabel);
        selectedIndex = EditorGUILayout.Popup("Level Number", selectedIndex, levelNames.ToArray());
        string levelName = levelNames[selectedIndex];
        if(levelName == "New Level")
        {
            levelNumber = levelNumbers.Length + 1;
        }
        else
        {
            levelNumber = Int32.Parse(levelName);
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
            foreach (string label in labelForCubeNetObjects.Keys)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label);
                if (GUILayout.Button("Select"))
                {
                    Selection.objects = labelForCubeNetObjects[label].ToArray();
                }
                if (GUILayout.Button("Remove"))
                {
                    keyToDelete = label;
                    foreach (GameObject boxTileAnchor in labelForCubeNetObjects[label])
                    {
                        DestroyImmediate(boxTileAnchor);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            if (keyToDelete != "")
            {
                labelForCubeNetObjects.Remove(keyToDelete);
            }
        }

        foreach (SerializedObject objective in objectives)
        {
            bool toggled = false;
            if (toggleForObjective.ContainsKey(objective))
            {
                toggled = toggleForObjective[objective];
            }
            SerializedProperty props = objective.GetIterator();
            toggled = EditorGUILayout.BeginToggleGroup(objective.targetObject.ToString(), toggled);
            toggleForObjective[objective] = toggled;
            while (props.NextVisible(true))
            {
                if(objective.targetObject.GetType() == typeof(TotalCubesObjective))
                {
                    if(props.name == "totalCubes")
                    {
                        props.intValue = labelForCubeNetObjects.Keys.Count;
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
            string guid = AssetDatabase.CreateFolder(path, String.Format("Level{0:000}", levelNumber));
            path = AssetDatabase.GUIDToAssetPath(guid);
            string levelAssetPath = path + "/LevelObject";
            string assetPathAndName = levelAssetPath + ".asset";
            LevelObject newLevel = CreateInstance<LevelObject>();
            newLevel.levelNumber = levelNumber;

            string scenesPath = "Assets/Scenes";
            string newSceneGuid = AssetDatabase.CreateFolder(scenesPath, String.Format("Level{0:000}", levelNumber));
            scenesPath = AssetDatabase.GUIDToAssetPath(newSceneGuid);
            EditorSceneManager.SaveScene(new_scene, scenesPath + "/LevelScene.unity");
            newLevel.levelPath = new_scene.path;
            newLevel.labelForCubeNetPositions = new Dictionary<string, List<Vector3>>();
            foreach(KeyValuePair<string, List<GameObject>> entry in labelForCubeNetObjects)
            {
                List<Vector3> tileAnchorsPositions = new List<Vector3>();
                foreach (GameObject go in entry.Value)
                {
                    tileAnchorsPositions.Add(go.transform.position);
                }
                Debug.Log(tileAnchorsPositions.Count);
                newLevel.labelForCubeNetPositions[entry.Key] = tileAnchorsPositions;
            }
            List<ScriptableObject> new_objectives = new List<ScriptableObject>();
            AssetDatabase.CreateAsset(newLevel, assetPathAndName);
            foreach (SerializedObject objective in objectives)
            {
                if (toggleForObjective.ContainsKey(objective))
                {
                    if (toggleForObjective[objective])
                    {
                        ScriptableObject instance = serializedObjectForInstance[objective] as ScriptableObject;
                        string objectiveName = instance.ToString().Replace(" (", "").Replace(")", "");
                        string objectiveAssetPath = path + "/" + objectiveName + ".asset";
                        AssetDatabase.CreateAsset(instance, objectiveAssetPath);
                        new_objectives.Add(instance);
                    }
                }
            }
            newLevel.objectives = new_objectives.ToArray();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            EditorSceneManager.CloseScene(new_scene, true);
        }
    }

}

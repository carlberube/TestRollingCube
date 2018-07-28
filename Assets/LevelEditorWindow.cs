using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;



public class LevelEditorWindow : EditorWindow
{
    Vector2 scrollPos;
    int[] levelNumbers;
    CubeNet[] cubeNets;
    ObjectiveObject[] objectives;

    GameObject tileAnchorPrefab;

    public int levelNumber;


    public string levelPath;
    public int selectedIndex;
    public Dictionary<string, List<GameObject>> labelForCubeNetObjects;
    public Dictionary<ScriptableObject, bool> toggleForObjective;
    public Dictionary<CubeNet, Texture2D> textureForCubeNet;


    [MenuItem("Window/Level Publisher")]
    static void Init()
    {
        LevelEditorWindow window = (LevelEditorWindow)GetWindow(typeof(LevelEditorWindow));
        window.Show();
    }

    public void OnEnable()
    {
        toggleForObjective = new Dictionary<ScriptableObject, bool>();
        labelForCubeNetObjects = new Dictionary<string, List<GameObject>>();
        textureForCubeNet = new Dictionary<CubeNet, Texture2D>();
        tileAnchorPrefab = (GameObject) AssetDatabase.LoadAssetAtPath("Assets/BoxTileAnchor.prefab", typeof(GameObject));

        Debug.Log("TILE ANCHOR : " + tileAnchorPrefab);

        levelNumbers = GetAllLevels();
        cubeNets = GetAllCubeNets();
        objectives = GetAllObjectives();

        foreach(CubeNet cubeNet in cubeNets)
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
            Debug.Log("LevelObject: " + path);
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
            Debug.Log("CubeNet: " + path);
            CubeNet cubeNet = (CubeNet)AssetDatabase.LoadAssetAtPath(path, typeof(CubeNet));
            cubeNets.Add(cubeNet);
        }
        return cubeNets.ToArray();
    }

    private static ObjectiveObject[] GetAllObjectives()
    {
        List<ObjectiveObject> objectiveObjects = new List<ObjectiveObject>();
        string[] guids;
        guids = AssetDatabase.FindAssets("t:ObjectiveObject");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Debug.Log("ObjectiveObject: " + path);
            ObjectiveObject objectiveObject = (ObjectiveObject)AssetDatabase.LoadAssetAtPath(path, typeof(ObjectiveObject));
            objectiveObjects.Add(objectiveObject);
        }
        return objectiveObjects.ToArray();
    }

    void CreateCubeNet(CubeNet cubeNet)
    {
        string label = cubeNet.name;
        while (labelForCubeNetObjects.ContainsKey(label))
        {
            int index = 1;
            label = label + index.ToString();
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
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
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
        EditorGUILayout.EndScrollView();

        //GUILayout.Label("Current Cube Nets", EditorStyles.boldLabel);
        //bool unfolded = true;
        //unfolded = EditorGUILayout.Foldout(unfolded, "Current Cube Nets");
        //if (unfolded)
        //{
        //    EditorGUILayout.BeginHorizontal();
        //    foreach(string label in labelForCubeNetObjects.Keys)
        //    {
        //        EditorGUILayout.LabelField(label);
        //        if (GUILayout.Button("Select"))
        //        {
        //            Selection.objects = labelForCubeNetObjects[label].ToArray();
        //        }
        //        if (GUILayout.Button("Remove"))
        //        {
        //            foreach(GameObject boxTileAnchor in labelForCubeNetObjects[label])
        //            {
        //                Destroy(boxTileAnchor);
        //            }
        //            labelForCubeNetObjects.Remove(label);
        //        }
        //    }
        //    EditorGUILayout.EndHorizontal();
        //}


        //foreach(ObjectiveObject objective in objectives)
        //{
        //    bool toggled = false;
        //    toggled = EditorGUILayout.BeginToggleGroup(objective.name, toggled);
        //    if (toggled)
        //    {
        //        Type typeName = objective.GetType();
        //        var instancedObjective = CreateInstance(typeName);
        //        foreach (ObjectiveObject current_objective in toggleForObjective.Keys)
        //        {
        //            if(current_objective.GetType() == objective.GetType())
        //            {
        //                instancedObjective = current_objective;
        //            }
        //        }
        //        toggleForObjective[instancedObjective] = toggled;
        //        Editor.CreateEditor(instancedObjective);
        //    }

        //    //foreach(var prop in objective.GetType().GetProperties())
        //    //{
        //    //    Type type = prop.GetType();
        //    //    if (type == typeof(int))
        //    //    {
        //    //        int value = EditorGUILayout.IntField(prop.Name, value);
        //    //    }
        //    //    else if (type == typeof(Vector3[]))
        //    //    {
        //    //        EditorGUILayout.Vector3Field(prop.Name, )
        //    //    }
        //    //    else if (type == typeof(float))
        //    //    {

        //    //    }
        //    //}
        // }

        if (GUILayout.Button("Publish Level"))
        {
            //string path = "Assets/Levels";
            //string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + myString + ".asset");

            //AssetDatabase.CreateAsset(cubeNet, assetPathAndName);

            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
            //EditorUtility.FocusProjectWindow();
        }
    }

}

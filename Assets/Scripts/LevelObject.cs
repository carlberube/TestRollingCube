using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelObject : ScriptableObject
{
    public int levelNumber;
    public string levelPath;
    public ObjectiveObject[] objectives;
    public GameObject[] cubeNets;

}

[CustomEditor(typeof(LevelObject))]
public class LevelObjectEditor : Editor
{
    public bool unfoldedCubeNets;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelObject levelObject = (LevelObject)target;
        if (GUILayout.Button("Load Scene"))
        {
            EditorSceneManager.OpenScene(levelObject.levelPath, OpenSceneMode.Additive);
        }
    }
}
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelObject : ScriptableObject
{
    public int levelNumber;
    public string levelPath;
    public ScriptableObject[] objectives;
    public Dictionary<string, List<Vector3>> labelForCubeNetPositions;

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
        unfoldedCubeNets = EditorGUILayout.Foldout(unfoldedCubeNets, "Level Cube Nets");
        if (unfoldedCubeNets)
        {
            foreach (string label in levelObject.labelForCubeNetPositions.Keys)
            {
                EditorGUILayout.LabelField(label);
                int i = 0;
                foreach (Vector3 pos in levelObject.labelForCubeNetPositions[label])
                {
                    EditorGUILayout.Vector3Field(string.Format("Tile {0:00}", i), pos);
                    i ++;
                }
            }
        }
    }
}
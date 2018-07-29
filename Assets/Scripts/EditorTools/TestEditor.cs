using UnityEngine;
using UnityEditor;
using System.IO;

public class TestEditor : EditorWindow
{
    string myString;
    Sprite selectedSprite;
    Vector3[] selection_vectors;

    [MenuItem("Window/Cube Net Exporter")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        TestEditor window = (TestEditor)EditorWindow.GetWindow(typeof(TestEditor));
        window.Show();
    }

    void OnGUI()
    {

        GUILayout.Label("Cube Net Exporter", EditorStyles.boldLabel);
        myString = EditorGUILayout.TextField("Net Name", myString);
        selectedSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", selectedSprite, typeof(Sprite), true);
        if (GUILayout.Button("Export Selection"))
        {
            GameObject[] selection = GameObject.FindGameObjectsWithTag("BoxTileAnchor");
            selection_vectors = new Vector3[selection.Length];
            for (int i = 0; i < selection.Length; i++)
            {
                selection_vectors[i] = selection[i].transform.position;
            }
            CubeNet cubeNet = ScriptableObject.CreateInstance<CubeNet>();
            cubeNet.netName = myString;
            cubeNet.vectors = selection_vectors;
            cubeNet.thumbnailSprite = selectedSprite;
            string path = "Assets/CubeNets";
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + myString + ".asset");

            AssetDatabase.CreateAsset(cubeNet, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = cubeNet;
        }
    }
}

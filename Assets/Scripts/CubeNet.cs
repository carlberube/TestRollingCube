using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class CubeNet : ScriptableObject
{
    public string netName = "Net Name";
    public Sprite thumbnailSprite;
    public Vector3[] vectors;
}


public abstract class CubeNets : ScriptableObject
{
    public GameObject[] nets;
}



public class GizmoIconUtility
{
    [DidReloadScripts]
    static GizmoIconUtility()
    {
        EditorApplication.projectWindowItemOnGUI = ItemOnGUI;
    }

    static void ItemOnGUI(string guid, Rect rect)
    {
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);

        CubeNet obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(CubeNet)) as CubeNet;

        if (obj != null)
        {
            Sprite sprite = obj.thumbnailSprite;
            var croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            var pixels = sprite.texture.GetPixels((int)sprite.textureRect.x,
                                                    (int)sprite.textureRect.y,
                                                    (int)sprite.textureRect.width,
                                                    (int)sprite.textureRect.height);
            croppedTexture.SetPixels(pixels);
            croppedTexture.Apply();
            rect.width = rect.height;
            GUI.DrawTexture(rect, croppedTexture);
        }
    }
}
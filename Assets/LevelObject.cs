using UnityEngine;
using System.Collections.Generic;

public class LevelObject : ScriptableObject
{
    public int levelNumber;
    public string levelPath;
    public ObjectiveObject[] objectives;
    public Dictionary<string, GameObject[]> labelForCubeNetObjects;
    public GameObject[] boxTileAnchors;

    public void Awake()
    {
        boxTileAnchors = GameObject.FindGameObjectsWithTag("BoxTileAnchor");
    }
}

using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spawner : MonoBehaviour {

    public GameObject objectToSpawn;
    public ValidPositionsRuntimeSet validPositions;
    public BoxTileAnchorsRuntimeSet tileAnchors;

    private bool cubeInPlay = false;
    private GameObject cubeInstance;
    private GameGrid instancedGrid;

    public void OnLevelLoaded()
    {
        List<GameObject> gameObjects = new List<GameObject>();
        foreach(BoxTileAnchorController controller in tileAnchors.Items)
        {
            gameObjects.Add(controller.gameObject);
        }
        instancedGrid = new GameGrid(gameObjects.ToArray(), gameObjects[0]);
        foreach(Vector3 position in instancedGrid.GetValidSpawnPositions())
        {
            validPositions.Add(position);
        }
    }

    // Update is called once per frame
    void Update () {
        if (instancedGrid == null)
        {
            return;
        }
        if (!cubeInstance)
        {
            cubeInPlay = false;
        }
        Vector2 touch_position = Vector2.zero;
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touch_position = touch.position;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            touch_position = Input.mousePosition;
        }
        if(touch_position != Vector2.zero)
        { 
            Ray ray = Camera.main.ScreenPointToRay(touch_position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane))
            {
                Vector3 current_click = hit.point;
                int grid_x = 0;
                int grid_z = 0;
                instancedGrid.GetPositionOnGrid(current_click, out grid_x, out grid_z);

                Vector3 currentPosition = hit.point;
                currentPosition.x = RoundToNearestHalf(currentPosition.x);
                currentPosition.y = 0f;
                currentPosition.z = RoundToNearestHalf(currentPosition.z);
                if (validPositions.Items.Contains(currentPosition) && !cubeInPlay)
                {
                    currentPosition.y = objectToSpawn.transform.position.y;
                    cubeInstance = Instantiate(objectToSpawn, currentPosition, Quaternion.identity);
                    Controller cubeController = cubeInstance.GetComponent<Controller>();
                    cubeController.instancedGrid = instancedGrid;

                    cubeInPlay = true;
                }
            }
        }
    }

    public static float RoundToNearestHalf(float a)
    {
        return a = Mathf.Round(a * 2f) * 0.5f;
    }


}

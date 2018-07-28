using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public GameObject objectToinstantiate;
    public GameObject particleCube;
    private HashSet<Vector3> valid_positions;
    private bool cubeInPlay = false;
    private GameObject cube_instance;
    private List<GameObject> particuleCubes = new List<GameObject>();
    private Vector3 lastPosition = Vector3.zero;

    public GameObject puff_particles;

    public GameGrid instanced_grid = null;
    public string scene_path = "";
    public LevelController level_controller;

    private int current_level = 0;
    private int current_cubes;
    private int objective_rolls = 100;

    public Text objective_text;
    private string string_to_format;

    // Use this for initialization
    void Start() {
        level_controller = new LevelController();
        string_to_format = objective_text.text;
    }


    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Change Scene"))
        {
            current_level = 9999;
            scene_path = level_controller.LoadLevelAndGetPath(current_level);
            instanced_grid = null;
            //StartCoroutine(AssignNewGrid());
        }
    }

    //private IEnumerator AssignNewGrid()
    //{
    //    yield return new WaitForFixedUpdate();
    //    instanced_grid = level_controller.GetGridForScenePath(scene_path);
    //    FocusCameraOnGameObject(Camera.current, instanced_grid.anchors);
    //    LevelChanged(current_level);
    //}

    //private void LevelChanged(int level_number)
    //{
    //    Objective objective = level_controller.objective_for_level_number[current_level];
    //    current_cubes = 0;
    //    objective_rolls = objective.rolls;
    //    SetObjectiveUIText();
    //}

    //private void SetObjectiveUIText()
    //{
    //    Objective objective = level_controller.objective_for_level_number[current_level];
    //    objective_text.text = string.Format(string_to_format, current_cubes,
    //                                                          objective.cubes,
    //                                                          objective.rolls);
    //}


    // Update is called once per frame
    void Update () {
        if(instanced_grid == null)
        {
            objective_text.enabled = false;
            return;
        }
        
        if (!cube_instance)
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
                print(hit.collider);
                Vector3 current_click = hit.point;
                print("Current Click : " + current_click);
                int grid_x = 0;
                int grid_z = 0;
                instanced_grid.GetPositionOnGrid(current_click, out grid_x, out grid_z);
                print("Position on Grid : " + grid_x + " , " + grid_z);

                Vector3 currentPosition = hit.point;
                currentPosition.x = RoundToNearestHalf(currentPosition.x);
                currentPosition.y = 0f;
                currentPosition.z = RoundToNearestHalf(currentPosition.z);
                if (valid_positions.Contains(currentPosition) && !cubeInPlay)
                {
                    currentPosition.y = objectToinstantiate.transform.position.y;
                    cube_instance = Instantiate(objectToinstantiate, currentPosition, Quaternion.identity);
                    Controller cube_controller = cube_instance.GetComponent<Controller>();
                    cube_controller.instanced_grid = instanced_grid;
                    Instantiate(puff_particles, currentPosition, puff_particles.transform.rotation);
                    cubeInPlay = true;
                    DestroyParticleCubes();
                }
            }
        }
        if (cubeInPlay)
        {
            //objective_text.enabled = true;
            //SetObjectiveUIText();
            //if (cube_instance.transform.childCount == 6)
            //{
            //    cube_instance.GetComponent<Controller>().DestroyCube();
            //    current_cubes = current_cubes + 1;
            //    SetObjectiveUIText();
            //}
        }
        else
        {
            valid_positions = instanced_grid.GetValidSpawnPositions();
            if (particuleCubes.Count < 1)
            {
                foreach (Vector3 position in valid_positions)
                {
                    particuleCubes.Add(Instantiate(particleCube, position, particleCube.transform.rotation));
                }
            }
        }
    }

    private void DestroyParticleCubes()
    {
        foreach (GameObject part_cube in particuleCubes)
        {
            Destroy(part_cube);
        }
        particuleCubes.Clear();
    }



    public static float RoundToNearestHalf(float a)
    {
        return a = Mathf.Round(a * 2f) * 0.5f;
    }

    Bounds GetBoundsForGameObjects(GameObject[] game_objects)
    {
        Bounds bounds = new Bounds(game_objects[0].transform.position, Vector3.zero);
        for (var i = 1; i < game_objects.Length; i++)
            bounds.Encapsulate(game_objects[i].transform.position); 
        return bounds;
    }

    public void FocusCameraOnGameObject(Camera c, GameObject[] game_objects)
    {
        Bounds b = GetBoundsForGameObjects(game_objects);
        //Vector3 max = b.size;
        //// Get the radius of a sphere circumscribing the bounds
        //float radius = max.magnitude / 2f;
        //// Get the horizontal FOV, since it may be the limiting of the two FOVs to properly encapsulate the objects
        //float horizontalFOV = 2f * Mathf.Atan(Mathf.Tan(c.fieldOfView * Mathf.Deg2Rad / 2f) * c.aspect) * Mathf.Rad2Deg;
        //// Use the smaller FOV as it limits what would get cut off by the frustum        
        //float fov = Mathf.Min(c.fieldOfView, horizontalFOV);
        //float dist = radius / (Mathf.Sin(fov * Mathf.Deg2Rad / 2f));
        //Debug.Log("Radius = " + radius + " dist = " + dist);
        //c.transform.localPosition = new Vector3(c.transform.localPosition.x, c.transform.localPosition.y, dist);
        //if (c.orthographic)
        //    c.orthographicSize = radius;

        // Frame the object hierarchy
        c.transform.LookAt(b.center);
    }
}

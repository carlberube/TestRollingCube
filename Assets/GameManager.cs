using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public GameObject objectToinstantiate;
    public GameObject particleCube;
    private HashSet<Vector3> valid_positions;
    private bool cubeInPlay = false;
    private GameObject cube_instance;
    private List<GameObject> particuleCubes = new List<GameObject>();
    private Vector3 lastPosition = Vector3.zero;

    public GameObject puff_particles;

    // Use this for initialization
    void Start() {

    }
	// Update is called once per frame
	void Update () {
        if (!cube_instance)
        {
            cubeInPlay = false;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane))
            {
                print(hit.collider);
                Vector3 current_click = hit.point;
                print("Current Click : " + current_click);
                int grid_x = 0;
                int grid_z = 0;
                GridController.GetPositionOnGrid(current_click, out grid_x, out grid_z);
                print("Position on Grid : " + grid_x + " , " + grid_z);

                Vector3 currentPosition = hit.point;
                currentPosition.x = RoundToNearestHalf(currentPosition.x);
                currentPosition.y = 0f;
                currentPosition.z = RoundToNearestHalf(currentPosition.z);
                if (valid_positions.Contains(currentPosition) && !cubeInPlay)
                {
                    currentPosition.y = objectToinstantiate.transform.position.y;
                    cube_instance = Instantiate(objectToinstantiate, currentPosition, Quaternion.identity);
                    Instantiate(puff_particles, currentPosition, puff_particles.transform.rotation);
                    cubeInPlay = true;
                    DestroyParticleCubes();
                }
            }
        }
        if (cubeInPlay)
        {
            if (cube_instance.transform.childCount == 6)
            {
                cube_instance.GetComponent<Controller>().DestroyCube();
            }
            //print("Moving cube!!!");
        }
        else
        {
            valid_positions = GetValidSpawnPositions();
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

    private HashSet<Vector3> GetValidSpawnPositions()
    {
        HashSet<Vector3> valid_positions = new HashSet<Vector3>();
        for (int z = 0; z < GridController.grid_width; ++z)
        {
            for (int x = 0; x < GridController.grid_height; ++x)
            {
                if(GridController.grid[x,z].childCount == 1)
                {
                    if(x + 1 < GridController.grid_height && GridController.grid[x + 1, z].childCount == 0)
                    {
                        valid_positions.Add(GridController.GetWorldPositionFromGrid(x + 1, z));
                    }
                    if (z + 1 < GridController.grid_width && GridController.grid[x, z + 1].childCount == 0)
                    {
                        valid_positions.Add(GridController.GetWorldPositionFromGrid(x, z + 1));
                    }
                    if (x > 0 && GridController.grid[x - 1, z].childCount == 0)
                    {
                        valid_positions.Add(GridController.GetWorldPositionFromGrid(x - 1, z));
                    }
                    if (z > 0 && GridController.grid[x, z - 1].childCount == 0)
                    {
                        valid_positions.Add(GridController.GetWorldPositionFromGrid(x, z - 1));
                    }
                }
            }
        }
        return valid_positions;
    }

    public static float RoundToNearestHalf(float a)
    {
        return a = Mathf.Round(a * 2f) * 0.5f;
    }

}

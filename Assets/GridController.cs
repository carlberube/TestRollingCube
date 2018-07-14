using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GridController : MonoBehaviour {

    // The Grid itself
    public static int grid_width;
    public static int grid_height;
    public static Transform[,] grid = null;
    public GameObject box_tile_anchor_object;
    public static int top_z;
    public static int top_x;
    public static int bottom_x;
    public static int bottom_z;

    public GameObject[] anchors;

    public TextMesh text_debug_prefab;

    public void Awake()
    {
        anchors = new GameObject[transform.childCount];
        for(int i = 0; i < transform.childCount; ++i)
        {
            anchors[i] = transform.GetChild(i).gameObject;
        }
        float positive_x = 0;
        float positive_z = 0;
        float negative_x = 0;
        float negative_z = 0;
        grid_width = 0;
        grid_height = 0;
        foreach (GameObject tile_anchor in anchors)
        {
            positive_x = Mathf.Max(positive_x, tile_anchor.transform.position.x);
            negative_x = Mathf.Min(negative_x, tile_anchor.transform.position.x);
            positive_z = Mathf.Max(positive_z, tile_anchor.transform.position.z);
            negative_z = Mathf.Min(negative_z, tile_anchor.transform.position.z);
        }
        grid_height = Mathf.FloorToInt(positive_x + Mathf.Abs(negative_x)) + 1;
        grid_width = Mathf.FloorToInt(positive_z + Mathf.Abs(negative_z)) + 1;
        grid = new Transform[grid_height, grid_width];
        top_z = Mathf.FloorToInt(positive_z);
        top_x = Mathf.FloorToInt(positive_x);
        bottom_x = Mathf.FloorToInt(negative_x);
        bottom_z = Mathf.FloorToInt(negative_z);
        GenerateGrid();
    }

    public static Vector3 GetWorldPositionFromGrid(int grid_x, int grid_z)
    {
        return new Vector3((float)grid_x + (bottom_x) + 0.5f, 0,
                           (float)grid_z + (bottom_z) + 0.5f);
    }

    private void GenerateGrid()
    {
        for (int z = 0; z < grid_width; ++z)
        {
            for (int x = 0; x < grid_height; ++x)
            {
                Vector3 world_position = GetWorldPositionFromGrid(x, z);
                foreach(GameObject tile_anchor in anchors)
                {
                    if (tile_anchor.transform.position == world_position)
                    {
                        grid[x, z] = tile_anchor.transform;
                    }
                }
                if(grid[x,z] == null)
                {
                    GameObject instance = Instantiate(box_tile_anchor_object,
                                             world_position,
                                             box_tile_anchor_object.transform.rotation);
                    Destroy(instance.transform.GetChild(0).gameObject);
                    grid[x, z] = instance.transform;
                }
            }
        }

        for (int z = 0; z < grid_width; ++z)
        {
            for (int x = 0; x < grid_height; ++x)
            {
                Vector3 position = GetWorldPositionFromGrid(x, z);
                position.z = position.z + 0.5f;
                TextMesh text_mesh_instance = Instantiate(text_debug_prefab, position, text_debug_prefab.transform.rotation);
                text_mesh_instance.text = string.Format("[{0},{1}]", x, z);
            }
        }
    }

    public static void GetPositionOnGrid(Vector3 current_position, out int grid_x, out int grid_z)
    {
        Vector3 round_pos = current_position;
        round_pos.x = RoundToNearestHalf(round_pos.x);
        round_pos.z = RoundToNearestHalf(round_pos.z);
        // We never go negative with the grid. 
        if(Mathf.Sign(Mathf.FloorToInt(round_pos.x) - bottom_x) == -1)
        {
            grid_x = -1;
            grid_z = -1;
            return;
        }
        if (Mathf.Sign(Mathf.FloorToInt(round_pos.z) - bottom_z) == -1)
        {
            grid_x = -1;
            grid_z = -1;
            return;
        }
        grid_x = Mathf.Max(0, Mathf.FloorToInt(round_pos.x) - bottom_x);
        grid_z = Mathf.Max(0, Mathf.FloorToInt(round_pos.z) - bottom_z);

        return;
    }

    public bool IsTileEmpty(int grid_x, int grid_z)
    {
        if (grid[grid_x, grid_z].childCount == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }


    public static float RoundToNearestHalf(float a)
    {
        return a = Mathf.Round(a * 2f) * 0.5f;
    }

    // Use this for initialization
    void Start ()
    {
 
    }
	
	// Update is called once per frame
	void Update ()
    {

    }
}

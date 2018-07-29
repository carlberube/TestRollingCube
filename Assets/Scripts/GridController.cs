using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameGrid {

    // The Grid itself
    public int grid_width;
    public int grid_height;
    public Transform[,] grid = null;
    public GameObject box_tile_anchor_object;
    public int top_z;
    public int top_x;
    public int bottom_x;
    public int bottom_z;
    public GameObject[] anchors;

    public GameGrid(GameObject[] tile_anchors, GameObject anchor_object)
    {
        anchors = tile_anchors;
        box_tile_anchor_object = anchor_object;
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
                    GameObject instance = GameObject.Instantiate(box_tile_anchor_object,
                                             world_position,
                                             box_tile_anchor_object.transform.rotation);
                    GameObject.Destroy(instance.transform.GetChild(0).gameObject);
                    grid[x, z] = instance.transform;
                }
            }
        }
    }

    public Vector3 GetWorldPositionFromGrid(int grid_x, int grid_z)
    {
        return new Vector3((float)grid_x + (bottom_x) + 0.5f, 0,
                           (float)grid_z + (bottom_z) + 0.5f);
    }

    public void GetPositionOnGrid(Vector3 current_position, out int grid_x, out int grid_z)
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

    public HashSet<Vector3> GetValidSpawnPositions()
    {
        HashSet<Vector3> valid_positions = new HashSet<Vector3>();
        for (int z = 0; z < grid_width; ++z)
        {
            for (int x = 0; x < grid_height; ++x)
            {
                if (grid[x, z].childCount == 1)
                {
                    if (x + 1 < grid_height && grid[x + 1, z].childCount == 0)
                    {
                        valid_positions.Add(GetWorldPositionFromGrid(x + 1, z));
                    }
                    if (z + 1 < grid_width && grid[x, z + 1].childCount == 0)
                    {
                        valid_positions.Add(GetWorldPositionFromGrid(x, z + 1));
                    }
                    if (x > 0 && grid[x - 1, z].childCount == 0)
                    {
                        valid_positions.Add(GetWorldPositionFromGrid(x - 1, z));
                    }
                    if (z > 0 && grid[x, z - 1].childCount == 0)
                    {
                        valid_positions.Add(GetWorldPositionFromGrid(x, z - 1));
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

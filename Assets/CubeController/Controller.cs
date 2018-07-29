
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class Controller : MonoBehaviour
{
    public CubeControllersRuntimeSet cubeControllers;
    public float speed = 150.0f;
    [HideInInspector]
    public bool rotating = false;
    [HideInInspector]
    public GameGrid instancedGrid;

    private float offset = 0.5f;  // Assues a cube of 1 unit on a side.
    private Transform tr;

    private List<GameObject> visitedTiles = new List<GameObject>();

    private Vector2 touchdirection = Vector2.zero;
    private Vector2 touchStart = Vector2.zero;

    public UnityEvent onCubeRoll;
    public UnityEvent onCubeCompleted;
    public UnityEvent onCubeCreated;


    private void OnEnable()
    {
        cubeControllers.Add(this);
        onCubeCreated.Invoke();
    }

    private void OnDisable()
    {
        cubeControllers.Remove(this);
    }

    void Start()
    {
        tr = transform;
        Vector3 startPos = transform.position;
        startPos.y = 0.012f;

    }

    void Update()
    {
        Vector3 pos = Vector3.zero;
        Vector3 axis = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            pos = new Vector3(tr.position.x, 
                              tr.position.y - offset, 
                              tr.position.z + offset);
            axis = Vector3.right;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            pos = new Vector3(tr.position.x, 
                              tr.position.y - offset, 
                              tr.position.z - offset);
            axis = -Vector3.right;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            pos = new Vector3(tr.position.x + offset, 
                              tr.position.y - offset, 
                              tr.position.z);
            axis = -Vector3.forward;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            pos = new Vector3(tr.position.x - offset, 
                              tr.position.y - offset, 
                              tr.position.z);
            axis = Vector3.forward;
        }

        if(Input.touches.Length > 0)
        {
            GetTouchPosAndAxis(out pos, out axis);
        }

        RotateCube(pos, axis, 90.0f);
        if(transform.childCount == 6)
        {
            onCubeCompleted.Invoke();
        }
    }

    public void RotateCube(Vector3 pos, Vector3 axis, float degrees)
    {
        if (rotating)
        {
            return;
        }
        RaycastHit tileDirectionHit;
        Vector3 direction = Vector3.Cross(axis, Vector3.up);
        Vector3 destination = new Vector3((pos.x + (direction.x / 2)),
                                          0,
                                          (pos.z + (direction.z / 2)));
        GameObject current_tile_for_direction = null;
        // Checking if we can move in the direction we want by : 
        if (Physics.Raycast(transform.position, direction, out tileDirectionHit))
        {
            if (tileDirectionHit.collider.CompareTag("BoxTile"))
            {
                current_tile_for_direction = tileDirectionHit.collider.gameObject;
            }
        }
        int grid_x;
        int grid_z;
        instancedGrid.GetPositionOnGrid(destination, out grid_x, out grid_z);
        if(grid_x == -1 | grid_z == -1)
        {
            return;
        }
        if(grid_x >= instancedGrid.grid_height | grid_z >= instancedGrid.grid_width)
        {
            return;
        }
        Transform tile_anchor = instancedGrid.grid[grid_x, grid_z];
        if(tile_anchor.childCount == 1)
        {
            if (current_tile_for_direction)
            {
                return;
            }
        }
        if (pos != Vector3.zero)
        {
            StartCoroutine(DoRotation(pos, axis, 90.0f));
        }
    }

    private bool myApproximation(float a, float b, float tolerance)
    {
        return (Mathf.Abs(a - b) < tolerance);
    }

    private void GetTouchPosAndAxis(out Vector3 pos, out Vector3 axis)
    {
        pos = Vector3.zero;
        axis = Vector3.zero;

        bool directionChosen = false;
        // Track a single touch as a direction control.
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Handle finger movements based on touch phase.
            switch (touch.phase)
            {
                // Record initial touch position.
                case TouchPhase.Began:
                    touchStart = touch.position;
                    directionChosen = false;
                    break;

                // Determine direction by comparing the current touch position with the initial one.
                case TouchPhase.Moved:
                    touchdirection = touch.position - touchStart;
                    break;

                // Report that a direction has been chosen when the finger is lifted.
                case TouchPhase.Ended:
                    directionChosen = true;
                    break;
            }
        }
        if (directionChosen)
        {
            if(Mathf.Abs(touchdirection.x) < 0.5f && Mathf.Abs(touchdirection.y) < 0.5f)
            {
                return;
            }
            // Something that uses the chosen direction...
            if (Mathf.Abs(touchdirection.x) > Mathf.Abs(touchdirection.y))
            {
                if (touchdirection.x < 0)
                {
                    pos = new Vector3(tr.position.x,
                                        tr.position.y - offset,
                                        tr.position.z + offset);
                    axis = Vector3.right;
                }
                else
                {
                    pos = new Vector3(tr.position.x,
                                        tr.position.y - offset,
                                        tr.position.z - offset);
                    axis = -Vector3.right;
                }
            }
            else
            {
                if (touchdirection.y < 0)
                {
                    pos = new Vector3(tr.position.x - offset,
                                        tr.position.y - offset,
                                        tr.position.z);
                    axis = Vector3.forward;
                }
                else
                {
                    pos = new Vector3(tr.position.x + offset,
                                        tr.position.y - offset,
                                        tr.position.z);
                    axis = -Vector3.forward;
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("BoxTileAnchor"))
        {
            GameObject boxTile = other.GetComponent<BoxTileAnchorController>().boxTile;
            // Parent the object to the cube
            if(boxTile == null)
            {
                return;
            }
            BoxCollider boxTileCollider = boxTile.GetComponent<BoxCollider>();

            if (boxTile.transform.parent != transform)
            {
                if (!visitedTiles.Contains(boxTile))
                {
                    if (!rotating)
                    {
                        boxTile.transform.SetParent(transform, true);
                        boxTileCollider.enabled = false;
                    }
                }
                else if (boxTileCollider.enabled)
                {
                    if (!rotating)
                    {
                        boxTile.transform.SetParent(transform, true);
                        boxTileCollider.enabled = false;
                    }
                }
            }
        }
    }

    IEnumerator DoRotation(Vector3 pos, Vector3 axis, float degrees)
    {
        rotating = true;
        float curr = 0.0f;
        while (curr != degrees)
        {
            curr = Mathf.MoveTowards(curr, degrees, Time.deltaTime * speed);
            tr.RotateAround(pos, axis, Time.deltaTime * speed);
            yield return 0;
        }
        // Reset rotation to closest angle
        Vector3 vec = transform.eulerAngles;
        vec.x = RoundToNearestAngle(vec.x);
        vec.y = RoundToNearestAngle(vec.y);
        vec.z = RoundToNearestAngle(vec.z);
        transform.eulerAngles = vec;

        // Do the same for the transform
        Vector3 new_pos = transform.position;
        new_pos.x = RoundToNearestHalf(new_pos.x);
        new_pos.y = RoundToNearestHalf(new_pos.y);
        new_pos.z = RoundToNearestHalf(new_pos.z);
        transform.position = new_pos;

        // Reset all the children the same for the angle at least
        visitedTiles.Clear();
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            vec = child.eulerAngles;
            vec.x = RoundToNearestAngle(vec.x);
            vec.y = RoundToNearestAngle(vec.y);
            vec.z = RoundToNearestAngle(vec.z);
            child.eulerAngles = vec;
            child.GetComponent<BoxCollider>().enabled = true;
            visitedTiles.Add(child.gameObject);
        }
        // We're done with rotating
        onCubeRoll.Invoke();
        rotating = false;
    }


    public static float RoundToNearestHalf(float a)
    {
        return a = Mathf.Round(a * 2f) * 0.5f;
    }


    public static float RoundToNearestAngle(float a)
    {
        return a = Mathf.Round(a / 90) * 90;
    }

}


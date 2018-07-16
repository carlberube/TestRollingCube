
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Controller : MonoBehaviour
{
    public float speed = 150.0f;
    private float original_speed = 0f;
    private float offset = 0.5f;  // Assues a cube of 1 unit on a side.
    private Transform tr;
    public bool rotating = false;
    private bool swiping = false;
    private bool exploding = false;

    private Vector2 touchdirection = Vector2.zero;
    private Vector2 touch_start = Vector2.zero;
    private List<GameObject> visitedTiles = new List<GameObject>();
    private GameObject lastAnchorVisited = null;
    private Vector3 startPos;
    private Vector3 on_click_position = Vector3.zero;

    public GameObject explosion_particles;

    public GameGrid instanced_grid;

    void Start()
    {
        tr = transform;
        original_speed = speed;
        startPos = transform.position;
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
        instanced_grid.GetPositionOnGrid(destination, out grid_x, out grid_z);
        if(grid_x == -1 | grid_z == -1)
        {
            return;
        }
        if(grid_x >= instanced_grid.grid_height | grid_z >= instanced_grid.grid_width)
        {
            return;
        }
        Transform tile_anchor = instanced_grid.grid[grid_x, grid_z];
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

    public void DestroyCube()
    {
        exploding = true;
        Instantiate(explosion_particles, transform.position, transform.rotation);
        List<Transform> parent_less = new List<Transform>();
        foreach (Transform child in transform)
        {
            parent_less.Add(child);
        }
        transform.DetachChildren();
        foreach(Transform tile in parent_less)
        { 
            Rigidbody child_rigid_body = tile.GetComponent<Rigidbody>();
            child_rigid_body.useGravity = true;
            child_rigid_body.isKinematic = false;
            child_rigid_body.AddExplosionForce(800f, transform.position, 40f);
            
        }
        Vector3 up_position = transform.position + (Vector3.up * 3);
        StartCoroutine(ExplodeCube(transform, transform.position, up_position, 6f, parent_less));
    }

    IEnumerator ExplodeCube(Transform objectToMove, Vector3 a, Vector3 b, float speed, List<Transform> tiles_to_delete)
    {
        float step = (speed / (a - b).magnitude) * Time.fixedDeltaTime;
        float t = 0;
        Quaternion original_rotation = objectToMove.rotation;
        while (t <= 1.0f)
        {
            t += step; // Goes from 0 to 1, incrementing by step each time
            objectToMove.position = Vector3.Lerp(a, b, t); // Move objectToMove closer to b
            objectToMove.rotation = Quaternion.Lerp(original_rotation, Quaternion.Euler(new Vector3(900, 900, 900)), t);
            yield return new WaitForFixedUpdate();         // Leave the routine and return here in the next frame
        }
        objectToMove.position = b;
        foreach(Transform tile_to_delete in tiles_to_delete)
        {
            Destroy(tile_to_delete.gameObject);
        }
        Destroy(objectToMove.gameObject);
    }

    private bool myApproximation(float a, float b, float tolerance)
    {
        return (Mathf.Abs(a - b) < tolerance);
    }

    private void GetMousePosAndAxis(out Vector3 pos, out Vector3 axis)
    {
        Ray camRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        pos = Vector3.zero;
        axis = Vector3.zero;
        RaycastHit floorHit;
        float camRayLength = 100f;
        if (Physics.Raycast(camRay, out floorHit, camRayLength))
        {
            if (floorHit.collider.CompareTag("BoxTileAnchor"))
            {
                Vector3 playerToMouse = floorHit.point - transform.position;
                playerToMouse.y = 0f;
                Vector3 direction = playerToMouse / playerToMouse.magnitude;
                float forward = Vector3.Dot(direction.normalized, Vector3.forward);
                float right = Vector3.Dot(direction.normalized, Vector3.right);
                float best_axis = Mathf.Max(Mathf.Abs(forward), Mathf.Abs(right));

                if (best_axis == Mathf.Abs(right))
                {
                    if (myApproximation(right, 1f, .3f))
                    {
                        pos = new Vector3(tr.position.x + offset,
                                          tr.position.y - offset,
                                          tr.position.z);
                        axis = -Vector3.forward;
                    }
                    else if (myApproximation(right, -1f, .3f))
                    {
                        pos = new Vector3(tr.position.x - offset,
                                          tr.position.y - offset,
                                          tr.position.z);
                        axis = Vector3.forward;
                    }
                }
                else
                {
                    if (myApproximation(forward, 1f, .3f))
                    {
                        pos = new Vector3(tr.position.x,
                                          tr.position.y - offset,
                                          tr.position.z + offset);
                        axis = Vector3.right;
                    }
                    else if (myApproximation(forward, -1f, .3f))
                    {
                        pos = new Vector3(tr.position.x,
                                          tr.position.y - offset,
                                          tr.position.z - offset);
                        axis = -Vector3.right;
                    }
                }
            }
        }
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
                    touch_start = touch.position;
                    directionChosen = false;
                    break;

                // Determine direction by comparing the current touch position with the initial one.
                case TouchPhase.Moved:
                    touchdirection = touch.position - touch_start;
                    break;

                // Report that a direction has been chosen when the finger is lifted.
                case TouchPhase.Ended:
                    directionChosen = true;
                    break;
            }
        }
        if (directionChosen)
        {
            print("Direction : " + touchdirection);
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


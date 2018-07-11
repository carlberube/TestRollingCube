
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
    private List<GameObject> visitedTiles = new List<GameObject>();
    private GameObject lastAnchorVisited = null;
    private Vector3 startPos;

    private Vector3 on_click_position = Vector3.zero;

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

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            pos = new Vector3(tr.position.x, 
                              tr.position.y - offset, 
                              tr.position.z + offset);
            axis = Vector3.right;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            pos = new Vector3(tr.position.x, 
                              tr.position.y - offset, 
                              tr.position.z - offset);
            axis = -Vector3.right;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            pos = new Vector3(tr.position.x + offset, 
                              tr.position.y - offset, 
                              tr.position.z);
            axis = -Vector3.forward;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            pos = new Vector3(tr.position.x - offset, 
                              tr.position.y - offset, 
                              tr.position.z);
            axis = Vector3.forward;
        }

        if (Input.GetMouseButton(0))
        {
            GetMousePosAndAxis(out pos, out axis);
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
        RaycastHit hitInside;
        // Do the rotation

        Vector3 direction = Vector3.Cross(axis, Vector3.up);
        Vector3 destination = new Vector3((pos.x + (direction.x / 2)),
                                          0.012f,
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
        if (Physics.Raycast(pos, direction, out hitInside))
        {
            if (hitInside.collider.CompareTag("BoxTileAnchor"))
            {
                GameObject tileAnchor = hitInside.collider.gameObject;
                if (tileAnchor.transform.childCount < 1)
                {
                    // Maybe it's an empty anchor, check for its tile
                    // and see if it matches ours
                    if (current_tile_for_direction != tileAnchor.GetComponent<BoxTileAnchorController>().boxTile)
                    {
                        pos = Vector3.zero;
                    }
                }
                else if (current_tile_for_direction != null)
                {
                    pos = Vector3.zero;
                }
            }
        }
        else
        {
            if (destination != startPos)
            {
                pos = Vector3.zero;
            }
        }
        if (pos != Vector3.zero)
        {
            if (rotating)
            {
                speed = speed * 2;
            }
            else
            {
                speed = original_speed;
                StartCoroutine(DoRotation(pos, axis, 90.0f));
            }
        }
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

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("BoxTileAnchor"))
        {
            GameObject boxTile = other.GetComponent<BoxTileAnchorController>().boxTile;
            // Parent the object to the cube
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


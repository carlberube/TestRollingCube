using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public GameObject objectToinstantiate;
    public GameObject particleCube;
    private GameObject[] boxTileAnchors;
    private HashSet<Vector3> valid_positions;
    private bool cubeInPlay = false;
    private GameObject cube_instance;
    private List<GameObject> particuleCubes = new List<GameObject>();
    private Vector3 lastPosition = Vector3.zero; 

    // Use this for initialization
    void Start() {

    }
	// Update is called once per frame
	void Update () {
        // Ask the user to create a cube
        Ray myRay;
        RaycastHit hit;
        myRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(myRay, out hit))
        {
            Vector3 currentPosition = hit.point;
            currentPosition.x = RoundToNearestHalf(currentPosition.x);
            currentPosition.y = 0f;
            currentPosition.z = RoundToNearestHalf(currentPosition.z);
            if (!cubeInPlay)
            {
                // Would've initialize the board, but for now let's just collect the 
                // our boxtileanchors
                boxTileAnchors = GameObject.FindGameObjectsWithTag("BoxTileAnchor");
                valid_positions = GetValidSpawnPositions();
                if (particuleCubes.Count < 1)
                {
                    foreach (Vector3 position in valid_positions)
                    {
                        particuleCubes.Add(Instantiate(particleCube, position, particleCube.transform.rotation));
                        //GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        //capsule.transform.SetPositionAndRotation(position, Quaternion.identity);
                    }
                }
            }
            if (Input.GetMouseButton(0))
            {

                if (valid_positions.Contains(currentPosition) && !cubeInPlay)
                {
                    currentPosition.y = objectToinstantiate.transform.position.y;
                    cube_instance = Instantiate(objectToinstantiate, currentPosition, Quaternion.identity);
                    cubeInPlay = true;
                    DestroyParticleCubes();
                }
                if (cubeInPlay)
                {

                    //print("Moving cube!!!");
                }
            }
            lastPosition = currentPosition;
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
        foreach(GameObject boxTileAnchor in boxTileAnchors)
        {
            // Add all the positions next to the anchor, we will assume they are good
            Vector3 position = boxTileAnchor.transform.position;
            position.y = 0;
            valid_positions.Add(position + Vector3.right);
            valid_positions.Add(position - Vector3.right);
            valid_positions.Add(position + Vector3.forward);
            valid_positions.Add(position - Vector3.forward);
            if (boxTileAnchor.transform.childCount < 1)
            {
                // No children? That's a good position
                valid_positions.Add(position);
            }
            foreach(RaycastHit hit in Physics.SphereCastAll(boxTileAnchor.transform.position, 2f, Vector3.up))
            {    
                if (hit.collider.gameObject.CompareTag("BoxTileAnchor"))
                {
                    if(hit.collider.transform.childCount >= 1)
                    {
                        Vector3 hit_position = hit.collider.transform.position;
                        hit_position.y = 0f;
                        valid_positions.Remove(hit_position);
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

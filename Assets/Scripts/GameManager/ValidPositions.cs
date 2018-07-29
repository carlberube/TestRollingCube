
using System.Collections.Generic;
using UnityEngine;

public class ValidPositions : MonoBehaviour {

    public GameObject particleCube;
    public ValidPositionsRuntimeSet validPositions;
    private List<GameObject> particuleCubes = new List<GameObject>();

    public void CreateParticles ()
    {
        foreach (Vector3 position in validPositions.Items)
        {
            particuleCubes.Add(Instantiate(particleCube, position, particleCube.transform.rotation));
        }
    }

    public void DestroyParticles()
    {
        foreach (GameObject partCube in particuleCubes)
        {
            Destroy(partCube);
        }
        particuleCubes.Clear();
    }


}

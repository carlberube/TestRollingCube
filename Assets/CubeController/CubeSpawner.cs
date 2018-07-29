using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeSpawner : MonoBehaviour {

    public GameObject puff_particles;

    // Use this for initialization
    public void SpawnParticles ()
    {
        Instantiate(puff_particles, transform.position, puff_particles.transform.rotation);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeDestruction : MonoBehaviour
{
    public GameObject explosion_particles;

    public void DestroyCube()
    {
        Instantiate(explosion_particles, transform.position, transform.rotation);
        List<Transform> parent_less = new List<Transform>();
        foreach (Transform child in transform)
        {
            parent_less.Add(child);
        }
        transform.DetachChildren();
        foreach (Transform tile in parent_less)
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
        foreach (Transform tile_to_delete in tiles_to_delete)
        {
            Destroy(tile_to_delete.gameObject);
        }
        Destroy(objectToMove.gameObject);
    }
}

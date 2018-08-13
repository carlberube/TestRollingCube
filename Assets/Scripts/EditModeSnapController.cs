using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EditModeSnapController : MonoBehaviour {

    void Update()
    {
        float x, y, z;
        x = RoundToNearestHalf(transform.position.x);
        y = RoundToNearestHalf(transform.position.y);
        z = RoundToNearestHalf(transform.position.z);

        transform.position = new Vector3(x, y, z);
    }

    public static float RoundToNearestHalf(float a)
    {
        return a = Mathf.Round(a * 2f) * 0.5f;
    }

}


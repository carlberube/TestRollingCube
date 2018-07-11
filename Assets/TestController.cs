using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour {
    public float horizontalSpeed = 2.0F;
    public float verticalSpeed = 2.0F;

    // Use this for initialization
    void Start () {
		
	}

    void Update()
    {
        Vector2 touch_direction = Vector2.zero;
        float speed = 0f;
        if (Input.GetMouseButton(0))
        {

        }
        if (Input.touches.Length > 1)
        {
            if (Input.touches[0].phase == TouchPhase.Moved)//Check if Touch has moved.
            {
                touch_direction = Input.touches[0].deltaPosition.normalized;  //Unit Vector of change in position
                speed = Input.touches[0].deltaPosition.magnitude / Input.touches[0].deltaTime; //distance traveled divided by time elapsed
            }
        }

    }

}

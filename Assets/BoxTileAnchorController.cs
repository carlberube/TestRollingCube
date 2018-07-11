using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxTileAnchorController : MonoBehaviour {

    public GameObject boxTile;

	// Use this for initialization
	void Start () {
        foreach (Transform child in transform)
        {
            if (child.tag == "BoxTile")
                boxTile = child.gameObject;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerStay(Collider other)
    {
        if(other.transform.parent && other.transform.parent != transform)
        {
            if (other.transform.parent.GetComponent<Controller>())
            {
                bool rotating = other.transform.parent.GetComponent<Controller>().rotating;
                if (other.gameObject.CompareTag("BoxTile"))
                {
                    if (other.gameObject == boxTile) // That's our own Box Tile! 
                    {
                        if (!rotating)
                        {
                            //Reparent it
                            other.transform.SetParent(transform);
                            other.GetComponent<BoxCollider>().enabled = false;
                        }
                    }
                }
            }
        }
    }
}

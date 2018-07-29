using UnityEngine;


public class CameraController : MonoBehaviour {

    public BoxTileAnchorsRuntimeSet boxTileAnchors;

    Bounds GetBoundsForGameObjects(GameObject[] gameObjects)
    {
        Bounds bounds = new Bounds(gameObjects[0].transform.position, Vector3.zero);
        for (var i = 1; i < gameObjects.Length; i++)
            bounds.Encapsulate(gameObjects[i].transform.position);
        return bounds;
    }

    public void FocusCameraOnGameObject()
    {
        GameObject[] gameObjects = boxTileAnchors.Items.ToArray();
        Bounds b = GetBoundsForGameObjects(gameObjects);
        //Vector3 max = b.size;
        //// Get the radius of a sphere circumscribing the bounds
        //float radius = max.magnitude / 2f;
        //// Get the horizontal FOV, since it may be the limiting of the two FOVs to properly encapsulate the objects
        //float horizontalFOV = 2f * Mathf.Atan(Mathf.Tan(c.fieldOfView * Mathf.Deg2Rad / 2f) * c.aspect) * Mathf.Rad2Deg;
        //// Use the smaller FOV as it limits what would get cut off by the frustum        
        //float fov = Mathf.Min(c.fieldOfView, horizontalFOV);
        //float dist = radius / (Mathf.Sin(fov * Mathf.Deg2Rad / 2f));
        //Debug.Log("Radius = " + radius + " dist = " + dist);
        //c.transform.localPosition = new Vector3(c.transform.localPosition.x, c.transform.localPosition.y, dist);
        //if (c.orthographic)
        //    c.orthographicSize = radius;

        // Frame the object hierarchy
        transform.LookAt(b.center);
    }
}

using UnityEngine;
using System.Collections;

public class MovingSpaceObject : MonoBehaviour
{
    public Transform meshObject;

    public virtual void DisableCollider()
    {
        meshObject.GetComponent<BoxCollider>().enabled = false;
    }
}

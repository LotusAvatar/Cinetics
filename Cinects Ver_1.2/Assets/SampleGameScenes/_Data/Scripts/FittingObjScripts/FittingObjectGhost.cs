using UnityEngine;
using System.Collections;

public class FittingObjectGhost : MonoBehaviour
{
    FittingObject myParent;

	// Use this for initialization
	void Start ()
    {
        myParent = transform.parent.GetComponent<FittingObject>();
    }

    void OnTriggerEnter(Collider hit)
    {
        if (hit.tag == "Socket")
        {
            myParent.TryToMove(hit.transform.position);
        }
    }
}

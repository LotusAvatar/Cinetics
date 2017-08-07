using UnityEngine;
using System.Collections;

public class FittingObjectCollider : MonoBehaviour
{
    FittingObject fittingParent;

    void Start()
    {
        fittingParent = transform.parent.GetComponent<FittingObject>();
    }

    public GameObject HasBeenSelectet()
    {
       return fittingParent.ShowGhost();
    }

    public void HasBeenReleased()
    {
        fittingParent.HideGhost();
    }

    public float MyHeight()
    {
        return fittingParent.GridNormalizedPosition.y;
    }
}

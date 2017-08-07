using UnityEngine;
using System.Collections;

public class RadarObjects : MonoBehaviour
{
    protected Vector3 initialPos;
    protected Vector3 finalPos;
    protected Vector3 currentPosition;
    protected bool canMove;

    // Update is called once per frame
    protected virtual void Update ()
    {
        if (canMove)
            OnRadarObjectMove();
	}

    public virtual void OnDestroyObjectReference()
    {
        gameObject.GetComponent<ParticleSystem>().Stop();
        Destroy(gameObject, 1f);
    }

    protected virtual void OnRadarObjectMove()
    {
        
    }
}

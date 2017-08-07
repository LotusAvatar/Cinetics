using UnityEngine;
using System.Collections;

public class RadarMeteor : RadarObjects
{
    public GameObject radarBlast;
    GameObject newRadarBlast;
    Meteor meteorRef;
   
    public void Initialize(Vector3 _finalPos, Meteor _meteorRef)
    {
        initialPos = transform.position;
        finalPos = _finalPos;
        meteorRef = _meteorRef;
        meteorRef.RadarMeteor = this;
        canMove = true;
    }

    public void DestroyOnSpaceshipCollision()
    {
        canMove = false;
        newRadarBlast = Instantiate(radarBlast, transform.position, Quaternion.identity) as GameObject;
        OnDestroyObjectReference();
    }

    protected override void OnRadarObjectMove()
    {
        currentPosition = Vector3.Lerp(initialPos, finalPos, meteorRef.MovingProgress);
        transform.position = currentPosition;
    }
}

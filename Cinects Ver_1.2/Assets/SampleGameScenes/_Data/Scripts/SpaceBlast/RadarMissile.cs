using UnityEngine;
using System.Collections;

public class RadarMissile : RadarObjects {

    public GameObject radarBlast;
    GameObject newRadarBlast;
    Missile missileRef;

    public void Initialize( Missile _missileRef)
    {
        initialPos = transform.position;
        missileRef = _missileRef;
        missileRef.RadarMissile = this;
        canMove = true;
    }

    protected override void OnRadarObjectMove()
    {
        finalPos = missileRef.TargetRadarPos;
        float totalDistance = Vector3.Distance(finalPos, initialPos);
        float currentDistance = Vector3.Distance(finalPos, transform.position);

        currentPosition = Vector3.Lerp(initialPos, finalPos, missileRef.GetMissileProgress());
        transform.position = currentPosition;
    }

    public override void OnDestroyObjectReference()
    {
        canMove = false;
        newRadarBlast = Instantiate(radarBlast, transform.position, Quaternion.identity) as GameObject;
        base.OnDestroyObjectReference();
    }
}

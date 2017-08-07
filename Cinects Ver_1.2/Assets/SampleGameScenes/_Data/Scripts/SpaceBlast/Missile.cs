using UnityEngine;
using System.Collections;

public enum MissileType
{
    Type_1 = 0,
    Type_2,
    Type_3
}

public class Missile : MonoBehaviour {

    public MissileType missileType;
    public GameObject[] ignite;
    public Transform rotateObj;
    Meteor target;
    bool canMove;
    float scaleProgress;
    float movingProgress;
    float acceleration = 0.5f;
    Vector3 currentPosition;
    Vector3 initialRef;
    Vector3 finalRef;
    Vector3 initialScale;
    Vector3 finalScale;
    Vector3 currentScale;
    RadarMissile radarMissile;

    public RadarMissile RadarMissile
    {
        set { radarMissile = value; }
    }

    public float Progress
    {
        get { return scaleProgress; }
    }

    public Vector3 TargetRadarPos
    {
        get
        {
            if (target == null)
                return Vector3.zero;

            return target.RadarMeteor.transform.position; }
    }

    public void Initilalize(Meteor _target)
    {
        target = _target;

        for (int i = 0; i < ignite.Length; i++)
            ignite[i].SetActive(true);

        initialScale = Vector3.one;
        initialRef = transform.position;
        finalScale = Vector3.one * 0.1f;
        RadarController.Instance.CreateMissileOnRadar(this);
        canMove = true;
    }

    public float GetMissileProgress()
    {
        float totalDistace = Vector3.Distance(initialRef, target.transform.position);
        float currentDistance = Vector3.Distance(initialRef, transform.position);
        movingProgress = currentDistance / totalDistace;
        return movingProgress;
    }
    
	void Update ()
    {
        if (canMove)
        {
            if (target == null)
            {
                Destroy(this.gameObject);
                return;
            }
            finalRef = target.transform.position;
            OnMove();
        }
	}

    void OnMove()
    {
        scaleProgress = (transform.position.z - initialRef.z) / (200f - initialRef.z);
        float distance = Vector3.Distance(transform.position, finalRef);
        if (distance > 3f)
        {
            TurnTowards(finalRef);
            transform.Translate(Vector3.forward * acceleration);

            currentScale = Vector3.Lerp(initialScale, finalScale, scaleProgress);
            transform.localScale = currentScale;
            rotateObj.Rotate(Vector3.forward);
        }
        else
        {
            radarMissile.OnDestroyObjectReference();
            target.OnDestroyedByMissile((int)missileType);
            Destroy(this.gameObject);
        }
    }

    void TurnTowards(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        if (direction.magnitude <= 0.002f)
        {
            return;
        }

        Quaternion newRot = Quaternion.LookRotation(direction);

        if (newRot != transform.rotation)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, newRot, 0.05f);
        }
    }
}

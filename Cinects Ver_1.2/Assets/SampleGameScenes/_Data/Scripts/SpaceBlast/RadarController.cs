using UnityEngine;
using System.Collections;

public class RadarController : MonoBehaviour {

    public Transform[] initialRadarPositions;
    public Transform[] finalRadarPositions;
    public GameObject bigMeteor;
    public GameObject smallMeteor;
    public GameObject radarMissile;
    GameObject newRadarObject;

    private static RadarController instance;
    /// <summary>
    /// Gets the single JointOverlayerCalibration instance.
    /// </summary>
    /// <value>The JointOverlayerCalibration instance.</value>
    public static RadarController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (RadarController)GameObject.FindObjectOfType(typeof(RadarController));
            }
            return instance;
        }
    }

    public void CreateMeteorOnRadar(int initialIndex, int finalIndex, float progress, Meteor meteorRef)
    {
        if (meteorRef.IsFisrt)
        {
            newRadarObject = Instantiate(bigMeteor, initialRadarPositions[initialIndex].position,
                Quaternion.identity) as GameObject;
        }
        else
        {
            int meteorFinalIndex = meteorRef.FinalIndex % 3;
            Vector3 middlePos = Vector3.Lerp(initialRadarPositions[meteorRef.InitialIndex].position,
                finalRadarPositions[meteorFinalIndex].position, progress);
            newRadarObject = Instantiate(smallMeteor, middlePos, Quaternion.identity) as GameObject;
        }

        finalIndex %= 3;
        newRadarObject.transform.SetParent(this.transform);
        newRadarObject.GetComponent<RadarMeteor>().Initialize(finalRadarPositions[finalIndex].position, meteorRef);
    }

    public void CreateMissileOnRadar(Missile missile)
    {
        newRadarObject = Instantiate(radarMissile, finalRadarPositions[0].position,
                Quaternion.identity) as GameObject;

        newRadarObject.transform.SetParent(this.transform);
        newRadarObject.GetComponent<RadarMissile>().Initialize(missile);
    }
}

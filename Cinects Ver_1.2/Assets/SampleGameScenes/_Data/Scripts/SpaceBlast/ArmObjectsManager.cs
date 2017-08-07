using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArmObjectsManager : MonoBehaviour
{
    ArmAnimationController armAnimationController;
    public ParticleSystem magneticPulse;
    public GameObject[] objectsInArm;
    public Transform powerUpParent;
    GameObject currentObject;
    GameObject nextObject;
    GameObject powerUp;

    // Use this for initialization
    void Start()
    {
        armAnimationController = this.GetComponent<ArmAnimationController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnChangeObjectInArm()
    {
        for (int i = 0; i < objectsInArm.Length; i++)
            objectsInArm[i].SetActive(false);

        if (SpaceBlastGameController.Instance.objectIndex != 3)
            SpaceBlastGameController.Instance.objectIndex = SpaceBlastGameController.Instance.missileIndex;

        currentObject = objectsInArm[SpaceBlastGameController.Instance.objectIndex];
        SpaceBlastGameController.Instance.objectIndex = SpaceBlastGameController.Instance.missileIndex;
        currentObject.SetActive(true);
    }

    public void ClearPowerUp()
    {
        powerUp.SetActive(false);
        SpaceBlastGameController.Instance.UpdatePowerUp();
    }

    public void GetPowerUp(GameObject _powerUp)
    {
        powerUp = _powerUp;
        powerUp.transform.SetParent(powerUpParent);
    }

    public void LaunchMissile()
    {
        nextObject = Instantiate(currentObject, currentObject.transform.position, currentObject.transform.rotation) as GameObject;
        currentObject.SetActive(false);

        //***********JOintOverlayer***************
        SpaceBlastGameController.Instance.SetMissileTarget().DisableCollider();
        nextObject.GetComponent<Missile>().Initilalize(SpaceBlastGameController.Instance.SetMissileTarget());
    }

    public void StartMagneticPulse()
    {
        magneticPulse.Play(true);
        SpaceBlastGameController.Instance.OnInializeMagneticInteraction();
    }

    public void StopMagneticPulse()
    {
        magneticPulse.Stop(true);
    }
}

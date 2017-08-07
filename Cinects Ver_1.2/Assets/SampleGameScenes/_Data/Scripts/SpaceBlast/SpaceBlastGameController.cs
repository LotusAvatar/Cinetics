using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SpaceAimState
{
    Nope,
    Moving,
    LockingMeteor,
    LockingPowerUp
}

[System.Serializable]
public struct ButtonsMaterial
{
    public Material buttonMat;
    public ParticleSystem buttonParticle;
}

public class SpaceBlastGameController : MonoBehaviour
{
    public PowerUpController powerUpController;
    public Transform mainCamera;
    public GameObject armObject;
    public ButtonsMaterial[] buttonsParticle;
    public Transform meteorInitialRef;
    public Transform[] initialMeteorPositions;
    public Transform[] finalMeteorPositions;
    public GameObject[] meteors;
    public int missileIndex;
    public int objectIndex;
    public bool pull;
    public SpaceAimState spaceAimState = SpaceAimState.Moving;
    List<Meteor> meteorsInScene = new List<Meteor>();
    GameObject newObject;
    GameObject powerUp;
    Vector3 cameraInitialPos;
    Meteor newMeteor;
    Meteor targetMeteor;
    float shakeScaler = 1f;
    float shakeDelay = 0.02f;
    float shakeTimer;
    float damageAmount = 0f;
    float waveDelay = 40f;
    float waveDelayCounter = 40f;
    float initialTimer = 2f;
    float aimLockedDelay = 2f;
    float aimLockedCounter = 0f;
    bool shakeSwitch;
    bool onDamageTaken;
    int meteorsInWave = 2;
    System.Random rand = new System.Random();

    private static SpaceBlastGameController _instance;

    public static SpaceBlastGameController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (SpaceBlastGameController)GameObject.FindObjectOfType(typeof(SpaceBlastGameController));
            }
            return _instance;
        }
    }


    public float AimLockedDelay
    {
        get { return aimLockedDelay; }
    }

    void Start()
    {
        UpdatePowerUp();
    }

    void Update ()
    {
        MeteorWave();
        CheckAimState();

        if (onDamageTaken)
            ShakeSpaceship();

        if (pull)
        {
            pull = false;
            OnStartPull();
        }
    }

    void Recharge()
    {
        armObject.GetComponent<ArmAnimationController>().SetArmAnimatorState(ArmState.Recharge);
    }

    void CheckAimState()
    {
        if (spaceAimState == SpaceAimState.LockingMeteor ||
            spaceAimState == SpaceAimState.LockingPowerUp)
        {
            aimLockedCounter -= Time.deltaTime * 2f;
            if (aimLockedCounter <= 0f)
            {
                spaceAimState = SpaceAimState.Moving;
                JointOverlayerSpaceBlast.Instance.OnTargetRelese();
            }
        }
    }

    void MeteorWave()
    {
        if (initialTimer > 0f)
        {
            initialTimer -= Time.deltaTime;
            return;
        }

        if (meteorsInScene.Count == 0)
            waveDelayCounter = 0f;

        waveDelayCounter -= Time.deltaTime;

        if (waveDelayCounter <= 0)
        {
            waveDelayCounter = 40f;
            meteorsInWave++;

            if (meteorsInWave % 5 == 0)
                powerUpController.InitializeMovement();

            initialTimer = 1f;
            for (int i = 0; i < meteorsInWave; i++)
            {
                float nextDelay = (float)(rand.Next(10) / 10f);
                Invoke("InstantiateMeteor", nextDelay);
            }
        }
    }

    void InstantiateMeteor()
    {
        int meteorIndex = rand.Next(meteors.Length);
        int initialPosIndex = 0;
        meteorInitialRef.position = SortMeteorInitialPos(ref initialPosIndex);
        int finalPosIndex = rand.Next(finalMeteorPositions.Length);
        newObject = Instantiate(meteors[meteorIndex], meteorInitialRef.position, Quaternion.identity) as GameObject;
        newMeteor = newObject.GetComponent<Meteor>();
        newMeteor.Initialize(initialPosIndex, finalPosIndex, finalMeteorPositions[finalPosIndex].position, 10f, 0f, 0.02f, true);
        RadarController.Instance.CreateMeteorOnRadar(initialPosIndex, finalPosIndex, 0f, newMeteor);
        meteorsInScene.Add(newMeteor);
    }

    Vector3 SortMeteorInitialPos(ref int nearestIndex)
    {
        Vector3 nextPos = Vector3.zero;
        int index_1 = rand.Next(0, initialMeteorPositions.Length);
        int index_2 = rand.Next(1, initialMeteorPositions.Length) + index_1;
        index_2 %= initialMeteorPositions.Length;
        float variation = ((float)rand.Next(100)) / 100f;

        nextPos = Vector3.Lerp(initialMeteorPositions[index_1].position, 
            initialMeteorPositions[index_2].position, variation);

        float distance = 200f;
        float nextDistance = distance;
        for (int i = 0; i < initialMeteorPositions.Length; i++)
        {
            nextDistance = Vector3.Distance(nextPos, initialMeteorPositions[i].position);
            if (distance > nextDistance)
            {
                distance = nextDistance;
                nearestIndex = i;
            }
        }

        return nextPos;
    }

    void ShakeSpaceship()
    {
        shakeTimer -= Time.deltaTime;
        if (shakeTimer <= 0f)
        {
            shakeSwitch = !shakeSwitch;
            shakeTimer = shakeDelay;
            if (shakeSwitch)
            {
                Vector3 shakePos = cameraInitialPos + Random.onUnitSphere * 0.2f * shakeScaler;
                mainCamera.position = shakePos;
            }
            else
                mainCamera.position = cameraInitialPos;
        }

        if (shakeScaler > 0f)
            shakeScaler -= Time.deltaTime / 2f;

        if (shakeScaler < 0f)
        {
            onDamageTaken = false;
            shakeScaler = 0f;
            mainCamera.position = cameraInitialPos;
        }

    }

    public void OnStartPull()
    {
        armObject.GetComponent<ArmAnimationController>().SetArmAnimatorState(ArmState.Pull);
    }

    public void StopPulling()
    {
        armObject.GetComponent<ArmAnimationController>().StopPulling();
    }

    public void OnInializeMagneticInteraction()
    {
        powerUpController.OnStartMagnetic();
    }

    public void OnGetPowerUp()
    {
        armObject.GetComponent<ArmObjectsManager>().GetPowerUp(powerUp);
        armObject.GetComponent<ArmAnimationController>().StopPulling();
        if (missileIndex < 2)
            missileIndex++;
        //Invoke("Recharge", 2f);
    }

    public void UpdatePowerUp()
    {
        for (int i = 0; i < buttonsParticle.Length; i++)
        {
            buttonsParticle[i].buttonParticle.Stop();
            buttonsParticle[i].buttonMat.color = Color.black;
        }
        buttonsParticle[missileIndex].buttonParticle.Play();
        buttonsParticle[missileIndex].buttonMat.color = Color.green;
    }

    public void OnDamageChange(float amount, bool heal = false)
    {
        if (heal)
            damageAmount -= amount;
        else
            damageAmount += amount;
        
        onDamageTaken = damageAmount > 0f;
        damageAmount = Mathf.Clamp(damageAmount, 0f, 1f);
        shakeScaler = 1f;
    }

    public void RemoveMeteorFromList(Meteor meteor)
    {
        meteorsInScene.Remove(meteor);
    }

    public void DivideMeteorInHalf(Meteor meteor)
    {
        int half_1_Index = (int)meteor.meteorType + 1;
        half_1_Index %= 3;
        int half_2_Index = (half_1_Index + 1) % 3;
        int finalPosIndexHalf_1 = rand.Next(finalMeteorPositions.Length);
        int finalPosIndexHalf_2 = rand.Next(1, finalMeteorPositions.Length);
        finalPosIndexHalf_2 = (finalPosIndexHalf_1 + finalPosIndexHalf_2) % finalMeteorPositions.Length;

        newObject = Instantiate(meteors[half_1_Index], meteor.GetInitialHalfPosition(finalPosIndexHalf_1), Quaternion.identity) as GameObject;
        newMeteor = newObject.GetComponent<Meteor>();
        newMeteor.Initialize(meteor.InitialIndex, finalPosIndexHalf_1, finalMeteorPositions[finalPosIndexHalf_1].position, 6f, meteor.MovingProgress, meteor.Speed);
        RadarController.Instance.CreateMeteorOnRadar(meteor.InitialIndex, finalPosIndexHalf_1, meteor.MovingProgress, newMeteor);
        meteorsInScene.Add(newMeteor);

        newObject = Instantiate(meteors[half_2_Index], meteor.GetInitialHalfPosition(finalPosIndexHalf_2), Quaternion.identity) as GameObject;
        newMeteor = newObject.GetComponent<Meteor>();
        newMeteor.Initialize(meteor.InitialIndex, finalPosIndexHalf_2, finalMeteorPositions[finalPosIndexHalf_2].position, 4f, meteor.MovingProgress, meteor.Speed);
        RadarController.Instance.CreateMeteorOnRadar(meteor.InitialIndex, finalPosIndexHalf_2, meteor.MovingProgress, newMeteor);
        meteorsInScene.Add(newMeteor);

        RemoveMeteorFromList(meteor);
    }

    public void ReduceMeteorByHalf(Meteor meteor)
    {
        int halfIndex = (int)meteor.meteorType + 1;
        halfIndex %= 3;

        newObject = Instantiate(meteors[halfIndex], meteor.transform.position, Quaternion.identity) as GameObject;
        newMeteor = newObject.GetComponent<Meteor>();
        newMeteor.Initialize(meteor.InitialIndex, meteor.FinalIndex, meteor.FinalRef, 6f, meteor.MovingProgress, meteor.Speed);
        RadarController.Instance.CreateMeteorOnRadar(newMeteor.InitialIndex, newMeteor.FinalIndex, meteor.MovingProgress, newMeteor);
        meteorsInScene.Add(newMeteor);

        RemoveMeteorFromList(meteor);
    }

    public void OnPowerUpArive()
    {
        armObject.GetComponent<ArmAnimationController>().StopPulling();
    }

    public Meteor SetMissileTarget()
    {
        return targetMeteor;
    }

    public void LaunchMissile()
    {
        armObject.GetComponent<ArmAnimationController>().SetArmAnimatorState(ArmState.Attack);
        aimLockedCounter = -1f;
    }

    public void OnMeteorTargetLocked(Meteor target)
    {
        targetMeteor = target;
        spaceAimState = SpaceAimState.LockingMeteor;
        aimLockedCounter = aimLockedDelay;
        JointOverlayerSpaceBlast.Instance.OnTargetLocked();
    }

    public void OnPowerUpTargetLocked(GameObject _powerUp)
    {
        powerUp = _powerUp;
        spaceAimState = SpaceAimState.LockingPowerUp;
        aimLockedCounter = aimLockedDelay;
        JointOverlayerSpaceBlast.Instance.OnTargetLocked();
    }
    
}

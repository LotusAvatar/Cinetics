using UnityEngine;
using System.Collections;

public enum MeteorType
{
    Type_1 = 0,
    Type_2,
    Type_3
}

public class Meteor : MovingSpaceObject
{
    public bool hitShip;
    public bool hitMissile;
    public int missileIndex;
    public MeteorType meteorType;
    public GameObject enableParticle;
    public GameObject[] explosionParticles;
    public Transform[] halfPositions;
    public Transform halfPositionsParent;
    GameObject explode;
    Vector3 initialRef = new Vector3();
    Vector3 finalRef = new Vector3();
    Vector3 currentPosition = new Vector3();
    Vector3 currentScale = new Vector3();
    Vector3 initialScale = new Vector3();
    Vector3 finalScale = new Vector3();
    Vector3 randomRotation = new Vector3();
    float movingProgress;
    float speed;
    float finalScaleMultiplier;
    bool canMove = false;
    bool isFirst = false;
    int initialIndex;
    int finalIndex;
    Space space = Space.Self;

    public float Speed
    {
        get { return speed; }
    }

    public float MovingProgress
    {
        get { return movingProgress; }
    }

    public Vector3 FinalRef
    {
        get { return finalRef; }
    }

    private RadarMeteor radarMeteor;

    public RadarMeteor RadarMeteor
    {
        get { return radarMeteor; }
        set { radarMeteor = value; }
    }

    public bool IsFisrt
    {
        get { return isFirst; }
    }

    public int InitialIndex
    {
        get { return initialIndex; }
    }

    public int FinalIndex
    {
        get { return finalIndex; }
    }

    // Update is called once per frame
    void Update ()
    {
        if (canMove)
            OnMove();

        if (hitShip)
        {
            hitShip = false;
            ExplodeOnHitSpaceShip();
        }

        if (hitMissile)
        {
            hitMissile = false;
            OnDestroyedByMissile(missileIndex);
        }
    }
	
    public void Initialize(int _initialIndex, int _finalIndex, Vector3 _finalRef, float _finalScaleMultiplier, float _movingProgress, float _speed, bool _isFirst = false)
    {
        initialIndex = _initialIndex;
        finalIndex = _finalIndex;
        isFirst = _isFirst;
        enableParticle.SetActive(isFirst);
        initialScale = Vector3.one;
        if (!isFirst)
            initialScale *= 0.5f;

        finalScale = Vector3.one;
        randomRotation = Random.onUnitSphere * 50f;
        initialRef = transform.position;
        finalRef = _finalRef;
        finalScaleMultiplier = _finalScaleMultiplier;
        finalScale *= finalScaleMultiplier;
        initialScale = Vector3.Lerp(initialScale, finalScale , _movingProgress);
        speed = _speed * (1f + _movingProgress);
        canMove = true;
    }

    public void OnDestroyedByMissile(int missileIndex)
    {
        canMove = false;
        meshObject.gameObject.SetActive(false);
        explode = Instantiate(explosionParticles[missileIndex], transform.position, Quaternion.identity) as GameObject;
        explode.transform.SetParent(transform);
        explode.transform.localPosition = Vector3.zero;

        if (isFirst)
        {
            switch (missileIndex)
            {
                case 0: SpaceBlastGameController.Instance.DivideMeteorInHalf(this); break;
                case 1: SpaceBlastGameController.Instance.ReduceMeteorByHalf(this); break;
                case 2: SpaceBlastGameController.Instance.RemoveMeteorFromList(this); break;
                default: break;
            }
        }
        else
            SpaceBlastGameController.Instance.RemoveMeteorFromList(this);

        radarMeteor.OnDestroyObjectReference();

        Destroy(this.gameObject, 2f);
    }

    public Vector3 GetInitialHalfPosition(int index)
    {
        return halfPositions[index].position;
    }

    void OnMove()
    {
        movingProgress += Time.deltaTime * speed;
        if (movingProgress < 1f)
        {
            currentPosition = Vector3.Lerp(initialRef, finalRef, movingProgress);
            transform.position = currentPosition;
            currentScale = Vector3.Lerp(initialScale, finalScale, movingProgress);
            halfPositionsParent.localScale = currentScale;
            meshObject.localScale = currentScale;
            meshObject.Rotate(randomRotation * Time.deltaTime, Space.Self);
        }
        else
            ExplodeOnHitSpaceShip();
    }

    void ExplodeOnHitSpaceShip()
    {
        canMove = false;
        meshObject.gameObject.SetActive(false);
        explosionParticles[0].SetActive(true);
        float damageAmount = isFirst ? 0.1f : 0.2f;
        SpaceBlastGameController.Instance.OnDamageChange(damageAmount);
        SpaceBlastGameController.Instance.RemoveMeteorFromList(this);
        radarMeteor.DestroyOnSpaceshipCollision();
        Destroy(this.gameObject, 2f);
        //TODO Off The Radar
        //TODO: OnSpaceShipHit (isFirst)
    }
}

using UnityEngine;
using System.Collections;

public enum PowerUpState
{
    Nope,
    Moving,
    ToInitialPos,
    ToFinalPos
}

public class PowerUpController : MonoBehaviour
{
    public Transform powerUp;
    public Transform initialMagneticRef;
    public Transform finalMagneticRef;
    public Transform rightMagneticRef;
    public Transform leftMagneticRef;
    PowerUpState powerUpState = PowerUpState.Nope;
    Vector3 initialPos;
    Vector3 finalPos;
    System.Random rand = new System.Random();
    float progress = 0f;
    float speed = 0.01f;
    bool move;
    public bool start;

	// Update is called once per frame
	void Update ()
    {
        if (move)
            OnMove();
	}

    public void InitializeMovement()
    {
        progress = 0f;
        int sort = rand.Next(2);
        bool towardsRight = sort == 0;

        if (towardsRight)
        {
            initialPos = leftMagneticRef.position;
            finalPos = rightMagneticRef.position;
        }
        else
        {
            initialPos = rightMagneticRef.position;
            finalPos = leftMagneticRef.position;
        }
        powerUp.gameObject.SetActive(true);
        powerUp.SetParent(this.transform);
        powerUp.position = initialPos;
        powerUpState = PowerUpState.Moving;
        move = true;
    }

    public void OnStartMagnetic()
    {
        progress = 0f;
        initialPos = powerUp.position;
        finalPos = initialMagneticRef.position;
        powerUpState = PowerUpState.ToInitialPos;
    }

    void OnMove()
    {
        switch (powerUpState)
        {
            case PowerUpState.Moving: Moving(); break;
            case PowerUpState.ToInitialPos: ToInitialPosition(); break;
            case PowerUpState.ToFinalPos: ToFinalPosition(); break;
            default: break;
        }
    }

    void Moving()
    {
        progress += Time.deltaTime * speed;
        if (progress <= 1f)
            powerUp.position = Vector3.Lerp(initialPos, finalPos, progress);
        else
        {
            powerUp.position = initialPos;
            powerUpState = PowerUpState.Nope;
        }
    }

    void ToInitialPosition()
    {
        progress += Time.deltaTime;
        if (progress <= 1f)
            powerUp.position = Vector3.Lerp(initialPos, finalPos, progress);
        else
        {
            progress = 0f;
            powerUp.position = finalPos;
            initialPos = finalPos;
            finalPos = finalMagneticRef.position;
            SpaceBlastGameController.Instance.StopPulling();
            powerUpState = PowerUpState.ToFinalPos;
        }
    }

    void ToFinalPosition()
    {
        progress += Time.deltaTime;
        if (progress <= 1f)
            powerUp.position = Vector3.Lerp(initialPos, finalPos, progress);
        else
        {
            progress = 0f;
            powerUp.position = finalPos;
            SpaceBlastGameController.Instance.OnGetPowerUp();
            powerUpState = PowerUpState.Nope;
        }
    }
}

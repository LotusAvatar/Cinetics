using UnityEngine;
using System.Collections;

public enum ClockState
{
    Nope = 0,
    Ticking,
    Ringing
}

public class ClockBehavior : MonoBehaviour
{
    public Transform clock;
    public Transform hourPointer;
    public Transform minutePointer;
    Quaternion hourRotReference;
    Quaternion minuteRotReference;
    Vector3 clockInitialPosition;
    Vector3 clockUpperPos;
    Vector3 pointerSpeed;
    float progress = 0f;
    float tickingTime;
    float ringingTime = 5f;
    float ringDelay = 0f;
    float fallingSpeed;
    bool swithPos;
    ClockState currentState = ClockState.Nope;
    System.Random rand = new System.Random();

    // Use this for initialization
    void Start ()
    {
        pointerSpeed.z = 1f;
        hourRotReference = hourPointer.localRotation;
        minuteRotReference = minutePointer.localRotation;
        clockInitialPosition = clock.localPosition;
    }
	
	// Update is called once per frame
	void Update ()
    {
        OnStateChecker();
    }

    void OnStateChecker()
    {
        switch (currentState)
        {
            case ClockState.Nope:
                OnNope();
                break;
            case ClockState.Ticking:
                OnTicking();
                break;
            case ClockState.Ringing:
                OnRinging();
                break;
            default:
                break;
        }
    }

    public void StateTrigger(ClockState nextState)
    {
        switch (nextState)
        {
            case ClockState.Nope:
                OnNopeTrigger();
                break;
            case ClockState.Ticking:
                OnTickingTrigger();
                break;
            case ClockState.Ringing:
                OnRingingTrigger();
                break;
            default:
                break;
        }
    }

    void OnNopeTrigger()
    {
        currentState = ClockState.Nope;
        //TODO: Stop all SFX
    }

    void OnNope()
    {
        //TODO: Get device hour
    }

    void OnTickingTrigger()
    {
        currentState = ClockState.Ticking;
        //TODO: Play SFX Ticking
    }

    void OnTicking()
    {
        hourPointer.Rotate(Vector3.forward * 3f);
        minutePointer.Rotate(Vector3.forward * 60f);
    }

    void OnRingingTrigger()
    {
        currentState = ClockState.Ringing;
        progress = 0f;
        //TODO: Stop SFX Ticking
        //TODO: Play SFX Ringing
    }

    void OnRinging()
    {
        progress += Time.deltaTime;

        if (progress > ringingTime)
        {
            progress = 0;
            clock.localPosition = clockInitialPosition;
            StateTrigger(ClockState.Nope);
        }
        else
        {
            ringDelay += Time.deltaTime;
            if (ringDelay > 0.02f)
            {
                ringDelay = 0f;
                swithPos = !swithPos;

                if (swithPos)
                {
                    Vector3 sortedPos = new Vector3();
                    bool onLimitDistance = false;
                    do
                    {
                        sortedPos = Random.onUnitSphere;
                        onLimitDistance = (Vector3.Distance(sortedPos, clockInitialPosition) < 1f && sortedPos.y > 0f);

                    } while (!onLimitDistance);

                    clock.localPosition += sortedPos;
                    clockUpperPos = clock.localPosition;
                }
                else
                    clock.localPosition = clockInitialPosition;
            }
        }
    }
}

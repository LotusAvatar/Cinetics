using UnityEngine;
using System.Collections;

public class FanUnity : MonoBehaviour 
{
	public enum FanState
	{
		Idle_01 = 0,
		Idle_02,
		Clapping,
		Thrilled,
		Ola,
		MissDefence
	}

	FanState fanState = FanState.Clapping;
	Animator fanAnimator;
	float startDelay;

	void Start()
	{
		fanAnimator = gameObject.GetComponent<Animator> ();
		startDelay = Random.Range (0f, 3f);
		Invoke ("StartStateMachine", startDelay); 
	}

	public void SetCurrentState(FanState nextState)
	{
		switch (nextState) 
		{
		case FanState.Idle_01: OnIdleState(0); break;
		case FanState.Idle_02: OnIdleState(1); break;
		case FanState.Clapping: OnCelebrateState(0); break;
		case FanState.Thrilled: OnCelebrateState(1); break;
		case FanState.Ola: OnOlaState(); break;
		case FanState.MissDefence: OnMissDefenceState(); break;
		default:
			break;
		}
		fanState = nextState;
	}

	void OnIdleState(int idleIndex)
	{

		fanAnimator.SetBool ("EnterIdle", true);
		fanAnimator.SetInteger ("CurrentIdle", idleIndex);

	}

	void OnCelebrateState(int celebrationIndex)
	{
		fanAnimator.SetBool ("EnterCelebrate", true);
		fanAnimator.SetInteger ("CurrentCelebration", celebrationIndex);
	}

	void OnOlaState()
	{
		fanAnimator.SetTrigger ("StartOla");
	}

	void OnMissDefenceState()
	{
		fanAnimator.SetTrigger ("StartMissBall");
	}

	void SortNextAnimation()
	{
		AnimatorStateInfo info = fanAnimator.GetCurrentAnimatorStateInfo (0);
		FanState nextState;
		fanAnimator.SetBool ("EnterCelebrate", false);
		fanAnimator.SetBool ("EnterIdle", false);


		if (info.IsName("MissDefence")) 
		{
			nextState =(FanState) Random.Range (0, 2);
		}
		else
		{
			do 
			{
				nextState =(FanState) Random.Range (0, 4);
			} while (nextState == fanState);
		}
		SetCurrentState (nextState);
	}

	void StartStateMachine()
	{
		fanAnimator.SetBool ("CanStart", true);
		FanState nextState =(FanState) Random.Range (0, 4);
		SetCurrentState (nextState);
	}
}

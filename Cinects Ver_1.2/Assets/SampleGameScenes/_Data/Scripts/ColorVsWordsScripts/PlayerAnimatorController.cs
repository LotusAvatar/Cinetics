using UnityEngine;
using System;
using System.Collections;

public enum PlayerState
{
	SittingIdle = 0,
	SittingCelebrating,
	SittingAngry,
	Sitting,
	StandingIdle,
	StandingCelebrating,
	StandingAngry,
	Standing,
	WalkingForward,
	WalkingBackwards,
	Running,
	PositioningBall,
	Kicking
}

public class PlayerAnimatorController : MonoBehaviour 
{
	enum RunningState
	{
		ToBench,
		FromBench,
		ToChangingRoom
	}

	public static event Action<PlayerState> AnimatorCallback;
	public bool changeStateTo;
	public PlayerState _nextState;
	public int testIndex;
	public GameObject ballInHand;
	RunningState runningState;
	PlayerState currentState = PlayerState.SittingIdle;
	Animator playerAnimator;
	int sittingIdleIndex = 0;
	bool onRunning = false;

	public PlayerState CurrentState 
	{
		get {return this.currentState;}
		set {currentState = value;}
	}

	void Awake()
	{
		playerAnimator = gameObject.GetComponent<Animator>();
	}

	// Use this for initialization
	void Start () 
	{
		currentState = PlayerState.SittingIdle;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (changeStateTo) 
		{
			changeStateTo = false;
			SetPlayerNextState(_nextState.ToString());
		}
	}

	public void SetPlayerNextState(string nextStateString)
	{
		switch (nextStateString) 
		{
		case "SittingIdle": currentState = PlayerState.SittingIdle; OnSittingIdle(); break;
		case "SittingCelebrating": currentState = PlayerState.SittingCelebrating; OnSittingCelebrating(); break;
		case "SittingAngry": currentState = PlayerState.SittingAngry; OnSittingAngry(); break;
		case "Sitting": currentState = PlayerState.Sitting; OnSitting(); break;
		case "StandingIdle": currentState = PlayerState.StandingIdle; break;
		case "StandingCelebrating": currentState = PlayerState.StandingCelebrating; OnStandingCelebrating(); break;
		case "StandingAngry": currentState = PlayerState.StandingAngry; OnStandingAngry(); break;
		case "Standing": currentState = PlayerState.Standing; OnStanding(); break;
		case "WalkingBackwards": currentState = PlayerState.WalkingBackwards; OnWalkingBackwards(); break;
		case "WalkingForward": OnWalkingForward(currentState); break;
		case "Running": OnRunning(currentState); break;
		case "PositioningBall": currentState = PlayerState.PositioningBall; OnPositioningBall(); break;
		case "Kicking": currentState = PlayerState.Kicking; OnKicking(); break;
		default: break;
		}
	}

	void OnSittingIdle()
	{
		if (PlayersBehaviorsController.Instance.SittingReactionIndex != -1) 
			PlayersBehaviorsController.Instance.SittingReactionIndex = -1;

		int rand = 0;
		do rand = UnityEngine.Random.Range(0,3);
		while (rand == sittingIdleIndex);
		sittingIdleIndex = rand;
		playerAnimator.SetInteger("SittingIdleIndex", sittingIdleIndex);
		if (AnimatorCallback != null) 
			AnimatorCallback (currentState);
	}

	void OnSittingCelebrating()
	{
		bool checkToCelerebrate = false;
		int rand = 0;
		do 
		{
			if (PlayersBehaviorsController.Instance.SittingReactionIndex != -1) 
			{
				rand = UnityEngine.Random.Range(0,3);
				if (rand != PlayersBehaviorsController.Instance.SittingReactionIndex) 
				{
					checkToCelerebrate = true; 
					break;
				}
			}
			else
			{
				checkToCelerebrate = true; 
				rand = UnityEngine.Random.Range(0,3);
			}

		}while (!checkToCelerebrate);
		PlayersBehaviorsController.Instance.SittingReactionIndex = rand;
		playerAnimator.SetInteger("HappyIndex", rand);
		playerAnimator.SetTrigger("OnReactHappy");
	}

	void OnSittingAngry()
	{
		bool checkToBeSad = false;
		int rand = 0;
		do 
		{
			if (PlayersBehaviorsController.Instance.SittingReactionIndex != -1) 
			{
				rand = UnityEngine.Random.Range(0,3);
				if (rand != PlayersBehaviorsController.Instance.SittingReactionIndex) 
				{
					checkToBeSad = true; 
					break;
				}
			}
			else
			{
				checkToBeSad = true; 
				rand = UnityEngine.Random.Range(0,3);
			}
			
		}while (!checkToBeSad);
		PlayersBehaviorsController.Instance.SittingReactionIndex = rand;
		playerAnimator.SetInteger("SadIndex", rand);
		playerAnimator.SetTrigger("OnReactSad");
	}

	void OnSitting()
	{
		playerAnimator.SetTrigger("OnSit");
	}

	void OnStandingCelebrating()
	{
		int rand = UnityEngine.Random.Range(0,2);
		playerAnimator.SetInteger("HappyIndex", rand);
		playerAnimator.SetTrigger("OnReactHappy");
	}

	void OnStandingAngry()
	{
		int rand = UnityEngine.Random.Range(0,3);
		playerAnimator.SetInteger("SadIndex", rand);
		playerAnimator.SetTrigger("OnReactSad");
	}

	void OnStanding()
	{
		playerAnimator.SetTrigger("OnStand");
		runningState = RunningState.FromBench;
		onRunning = false;
	}

	void OnWalkingBackwards()
	{
		playerAnimator.SetInteger("MovingIndex", 0);
		playerAnimator.SetTrigger("OnMoving");
	}
	
	void OnWalkingForward(PlayerState _currentState)
	{
		playerAnimator.SetInteger("MovingIndex", 1);
		if (_currentState != PlayerState.Running) 
			playerAnimator.SetTrigger("OnMoving");
		currentState = PlayerState.WalkingForward;
	}

	void OnRunning(PlayerState _currentState)
	{
		playerAnimator.SetInteger("MovingIndex", 2);
		if (_currentState != PlayerState.WalkingForward)
			playerAnimator.SetTrigger("OnMoving");
		currentState = PlayerState.Running;
	}

	void OnPositioningBall()
	{
		int kickStrength = UnityEngine.Random.Range(0,3);
		if (kickStrength > 0) 
			playerAnimator.SetInteger("PositioningBallIndex", 1);
		else 
			playerAnimator.SetInteger("PositioningBallIndex", kickStrength);

		playerAnimator.SetInteger("KickIndex", kickStrength );
		playerAnimator.SetTrigger("OnPositioningBall");
	}

	void OnKicking()
	{
		playerAnimator.SetTrigger("OnKicking");
	}

	void EnableBallInHand()
	{
		ballInHand.SetActive (true);
	}

	void DisableBallInHand()
	{
		ballInHand.SetActive (false);
		ColorManager.Instance.SetWhiteBallPosition(ballInHand.transform);
	}

	void KickBall()
	{
		ColorManager.Instance.PlayerKick ();
	}

	void OnRunningStart()
	{
		if (!onRunning) 
		{
			onRunning = true;
			if (runningState == RunningState.FromBench) 
				PlayersBehaviorsController.Instance.SetRunToPenaltyFromBench();
		}
	}

	void OnEndStanndingReaction()
	{
		if (ColorManager.Instance.NotFinishKicking()) 
		{
			SetPlayerNextState(PlayerState.PositioningBall.ToString());
		} 
		else 
		{
			ColorManager.Instance.ResetKicks();
			PlayersBehaviorsController.Instance.ToExit();
		}
	}

	void OnMoveBackwardWhileSitting()
	{
		PlayersBehaviorsController.Instance.StartMovingWhileSitting ();
	}
}

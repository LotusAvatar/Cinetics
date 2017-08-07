using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct Uniform
{
	public Material uniform;
	public bool isUsing;
}

public class PlayersBehaviorsController : MonoBehaviour 
{
	enum PlayersState
	{ 
		ChangeShooter,
		ToPenaltyFromBench,
		ToPenaltyWalking,
		LeavingFieldWalking,
		LeavingFieldRunning,
		EnterInTheField,
		WalkToBench,
		SittingInBench,
		PlacingBall,
		None
	}

	public Transform toPenaltyPosition;
	public Transform penaltyMark;
	public Transform goal;
	public Transform exitMark;
	public Transform enterMark;
	public Transform benchMark;
	public Material[] uniformMaterials;
	public float curretShooterRunningSpeed = 1f;
	public float curretShooterWalkingSpeed = 1f;
	public List<PlayerBehavior> players;

	PlayersState currentPlayersState = PlayersState.None;
	PlayersState secondaryPlayersState = PlayersState.None;
	PlayerBehavior curretShooter;
	PlayerBehavior secondaryShooter;
	int sittingReactionIndex = -1;
	int curretShooterIndex = 0;
	int lastShooterIndex = 0;
	float blendToPenaltyFromBenchDelay = 0.25f;
	float blendToPenaltyFromBenchTimer = 0f;
	float secondaryRotationSpeed;
	float sittingMovemenTimer;
	Uniform[] uniforms = new Uniform[4]; 
	bool toPenaltyFromBench;
	bool secondaryPlayer;
	bool moveWhileSitting;
	Vector3 nextPosition;
	Vector3 nextRotationPosition;
	Vector3 nextSecondaryPosition;
	Vector3 nextSecondaryRotationPosition;

	public int SittingReactionIndex 
	{
		get {return this.sittingReactionIndex;}
		set {sittingReactionIndex = value;}
	}

	#region Sigleton
	
	private static PlayersBehaviorsController _instance = null;
	
	public static PlayersBehaviorsController Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindObjectOfType<PlayersBehaviorsController>();
			}
			
			return _instance;
		}
	}
	
	#endregion


	void OnEnable()
	{
		ColorManager.FeedbackEvent += OnFeedbackEvent;
	}
	
	void OnDestroy()
	{
		ColorManager.FeedbackEvent -= OnFeedbackEvent;
	}

	// Use this for initialization
	void Start () 
	{
		DistributeUniforms ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (currentPlayersState == PlayersState.ToPenaltyFromBench) 
		{
			ToPenaltyFromBench ();
		}
		else if (currentPlayersState == PlayersState.ToPenaltyWalking) 
		{
			ToPenaltyWalking();
		}
		if (secondaryPlayersState == PlayersState.LeavingFieldWalking) 
		{
			LeavingFieldWalking();
		}
		else if (secondaryPlayersState == PlayersState.LeavingFieldRunning) 
		{
			LeavingFieldRunning();
		}
		else if (secondaryPlayersState == PlayersState.EnterInTheField) 
		{
			EnterInTheField();
		}
		else if (secondaryPlayersState == PlayersState.WalkToBench) 
		{
			WalkToBench();
		}
		else if (secondaryPlayersState == PlayersState.SittingInBench) 
		{
			SittingInBench();
		}
		if (moveWhileSitting) 
		{
			MoveWhileSitting();
		}
	}

	public void CallPlayerToShoot(bool hasExitingPlayer = false)
	{
		lastShooterIndex = curretShooterIndex;
		if (hasExitingPlayer) 
			secondaryShooter = curretShooter;
		do 
		{
			curretShooterIndex = Random.Range (0, players.Count);
		} while (lastShooterIndex == curretShooterIndex);

		curretShooter = players[curretShooterIndex];
		curretShooter.GetUp ();
	}
	
	void CurrentPlayerRotation()
	{
		Quaternion newRot = Quaternion.LookRotation (nextRotationPosition - curretShooter.transform.position);
		if (newRot != curretShooter.transform.rotation) 
		{
			curretShooter.transform.rotation = Quaternion.Lerp(curretShooter.transform.rotation, newRot, 0.1f);
		}
	}
	
	void ToPenaltyFromBench ()
	{
		float distance = Vector3.Distance (curretShooter.transform.position, nextPosition);
		if (distance >= 5f) 
		{
			blendToPenaltyFromBenchTimer += Time.deltaTime;
			if (blendToPenaltyFromBenchTimer <= blendToPenaltyFromBenchDelay) 
			{
				curretShooter.transform.Translate (Vector3.forward * Time.deltaTime * curretShooterRunningSpeed * 2f);
			} 
			else 
			{
				curretShooter.transform.Translate (Vector3.forward * Time.deltaTime * curretShooterRunningSpeed);
			}
		} 
		else 
		{
			currentPlayersState = PlayersState.ToPenaltyWalking;
			nextRotationPosition = goal.position;
			nextPosition = goal.position;
			curretShooter.playerAnimatorController.SetPlayerNextState(PlayerState.WalkingForward.ToString());
		}
		CurrentPlayerRotation ();
	}

	void ToPenaltyWalking ()
	{
		float distance = Vector3.Distance (curretShooter.transform.position, nextPosition);
		if (distance >= 50f) 
		{
			curretShooter.transform.Translate (Vector3.forward * Time.deltaTime * curretShooterWalkingSpeed);
		} 
		else 
		{
			currentPlayersState = PlayersState.PlacingBall;
			curretShooter.playerAnimatorController.SetPlayerNextState(PlayerState.PositioningBall.ToString());
			penaltyMark.position = curretShooter.transform.position;
		}
		CurrentPlayerRotation ();
	}

	void SecondaryPlayerRotation()
	{
		Quaternion newRot = Quaternion.LookRotation (nextSecondaryRotationPosition - secondaryShooter.transform.position);
		if (newRot != secondaryShooter.transform.rotation) 
		{
			secondaryShooter.transform.rotation = Quaternion.Lerp(secondaryShooter.transform.rotation, newRot, secondaryRotationSpeed);
		}
	}

	void LeavingFieldWalking()
	{
		float distance = Vector3.Distance (secondaryShooter.transform.position, penaltyMark.position);
		if (distance <= 5f) 
		{
			secondaryShooter.transform.Translate (Vector3.forward * Time.deltaTime * curretShooterWalkingSpeed);
		} 
		else 
		{
			secondaryShooter.playerAnimatorController.SetPlayerNextState(PlayerState.Running.ToString());
			secondaryPlayersState = PlayersState.LeavingFieldRunning;
		}
		SecondaryPlayerRotation ();
	}

	void LeavingFieldRunning()
	{
		float distance = Vector3.Distance (secondaryShooter.transform.position, nextSecondaryPosition);
		if (distance >= 0.5f) 
		{
			secondaryShooter.transform.Translate (Vector3.forward * Time.deltaTime * curretShooterRunningSpeed);
		} 
		else 
		{
			secondaryShooter.transform.position = enterMark.position;
			nextSecondaryPosition = benchMark.position;
			secondaryShooter.transform.LookAt(nextSecondaryPosition);
			int freeUniformIndex = 0;
			for (int i = 0; i < uniforms.Length; i++) 
			{
				if (!uniforms[i].isUsing) 
				{
					freeUniformIndex = i;
					break;
				}
			}
			for (int i = 0; i < uniforms.Length; i++) {
				if (uniforms[i].uniform == secondaryShooter.uniform.material) 
				{
					uniforms[i].isUsing = false;
					secondaryShooter.uniform.material = uniforms[freeUniformIndex].uniform;
					uniforms[freeUniformIndex].isUsing = true;
				}
			}
			secondaryPlayersState = PlayersState.EnterInTheField;
		}
		SecondaryPlayerRotation ();
	}

	void EnterInTheField()
	{
		float distance = Vector3.Distance (secondaryShooter.transform.position, nextSecondaryPosition);
		if (distance >= 5f) 
		{
			secondaryShooter.transform.Translate (Vector3.forward * Time.deltaTime * curretShooterRunningSpeed);
		} 
		else 
		{
			nextSecondaryPosition = secondaryShooter.benchInitialPosition;
			nextSecondaryRotationPosition = secondaryShooter.benchInitialPosition;
			secondaryShooter.playerAnimatorController.SetPlayerNextState(PlayerState.WalkingForward.ToString());
			secondaryRotationSpeed = 0.3f;
			secondaryPlayersState = PlayersState.WalkToBench;
		}
	}

	void WalkToBench()
	{
		float distance = Vector3.Distance (secondaryShooter.transform.position, nextSecondaryPosition);
		if (distance >= 0.2f) 
		{
			secondaryShooter.transform.Translate (Vector3.forward * Time.deltaTime * curretShooterWalkingSpeed);
		} 
		else 
		{
			secondaryShooter.playerAnimatorController.SetPlayerNextState(PlayerState.Sitting.ToString());
			nextSecondaryRotationPosition = penaltyMark.position;
			secondaryPlayersState = PlayersState.SittingInBench;
		}
		SecondaryPlayerRotation ();
	}

	void SittingInBench()
	{
		SecondaryPlayerRotation ();
	}

	void MoveWhileSitting()
	{
		sittingMovemenTimer += Time.deltaTime * 7.5f;
		if (sittingMovemenTimer <= 1f) 
		{
			secondaryShooter.transform.position = Vector3.Lerp(secondaryShooter.benchInitialPosition, secondaryShooter.benchFinialPosition, sittingMovemenTimer);
		} 
		else 
		{
			moveWhileSitting = false;
			secondaryPlayersState = PlayersState.None;
		}
	}

	void DistributeUniforms()
	{
		for (int i = 0; i < players.Count; i++) 
		{
			players[i].SetBenchPosition(players[i].transform.position);
		}
		for (int i = 0; i < uniformMaterials.Length; i++) 
		{
			uniforms[i].uniform = uniformMaterials[i];
			uniforms[i].isUsing = false;
		}
		int counter = 0;
		do 
		{
			int rand = Random.Range(0, uniforms.Length);
			if (uniforms[rand].isUsing == false) 
			{
				uniforms[rand].isUsing = true;
				players[counter].uniform.material = uniforms[rand].uniform;
				counter++;
			}

		} while (counter < players.Count);
	}


	void OnFeedbackEvent(FeedbackTypes feedback)
	{
		if (feedback == FeedbackTypes.Gain) 
		{
			foreach (PlayerBehavior player in players) 
				if (player.playerAnimatorController.CurrentState == PlayerState.SittingIdle) 
					player.playerAnimatorController.SetPlayerNextState(PlayerState.SittingCelebrating.ToString());
			
			curretShooter.playerAnimatorController.SetPlayerNextState(PlayerState.StandingAngry.ToString());
		}
		else if (feedback == FeedbackTypes.Loss || feedback == FeedbackTypes.Miss) 
		{
			foreach (PlayerBehavior player in players) 
				if (player.playerAnimatorController.CurrentState == PlayerState.SittingIdle) 
					player.playerAnimatorController.SetPlayerNextState(PlayerState.SittingAngry.ToString());

			curretShooter.playerAnimatorController.SetPlayerNextState(PlayerState.StandingCelebrating.ToString());
		}
	}
	public void SetRunToPenaltyFromBench()
	{
		currentPlayersState = PlayersState.ToPenaltyFromBench;
		blendToPenaltyFromBenchTimer = 0f;
		nextPosition = toPenaltyPosition.position;
		nextRotationPosition = nextPosition;
	}

	public void StartMovingWhileSitting()
	{
		sittingMovemenTimer = 0f;
		moveWhileSitting = true;
	}

	public void ToExit()
	{
		CallPlayerToShoot (true);
		nextSecondaryPosition = exitMark.position;
		nextSecondaryRotationPosition = nextSecondaryPosition;
		secondaryShooter.playerAnimatorController.SetPlayerNextState (PlayerState.WalkingForward.ToString());
		secondaryPlayersState = PlayersState.LeavingFieldWalking;
		secondaryRotationSpeed = 0.1f;
	}
}

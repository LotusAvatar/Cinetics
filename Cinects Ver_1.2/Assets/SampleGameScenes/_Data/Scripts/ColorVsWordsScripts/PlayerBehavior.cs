using UnityEngine;
using System.Collections;

public class PlayerBehavior : MonoBehaviour 
{
	public PlayerAnimatorController playerAnimatorController;
	public SkinnedMeshRenderer uniform;
	public Vector3 benchFinialPosition;
	public Vector3 benchInitialPosition;

	public void GetUp()
	{
		playerAnimatorController.SetPlayerNextState (PlayerState.Standing.ToString ());
	}

	public void SetBenchPosition(Vector3 benchPosition)
	{
		benchFinialPosition = benchPosition;
		benchInitialPosition = benchPosition;
		benchInitialPosition.x -= 1.5f;
	}
}

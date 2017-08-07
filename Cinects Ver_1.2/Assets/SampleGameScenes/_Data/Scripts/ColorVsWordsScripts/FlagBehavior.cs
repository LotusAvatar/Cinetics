using UnityEngine;
using System.Collections;

public class FlagBehavior : MonoBehaviour 
{
	enum FlagState
	{
		Hide,
		Show,
		Swing,
		Nope
	}

	public Cloth cloth;
	public Material[] materials; 
	public GameObject flagSupport;
	public SkinnedMeshRenderer flagMesh;
	FlagState flagState = FlagState.Nope;
	Vector3 flagHidePosition = new Vector3(-2.62f, -5.5f, -5.4f);
	Vector3 flagShowPosition = new Vector3(-2.62f, -2.35f, -5.4f);
	Vector3 swingAngle = new Vector3(0f, 0f, 1f);
	float flagRelativePosition = 0f;
	float externalAcceleration = 120f;
	float flagRotationLimit = 80f;
	bool switchFlagRotation = false;
	bool isInFinalPosition = false;
	int swingCounter = 0;

	void Update () 
	{
		if (flagState != FlagState.Nope) 
		{
			OnFlagStateBehavior();
		}
	}

	void OnFlagStateBehavior()
	{
		if (flagState == FlagState.Show) 
		{
			if (VertialTranslation(flagHidePosition, flagShowPosition, 5f)) 
			{
				flagState = FlagState.Swing; 
				flagRelativePosition = 0f;
				swingCounter = 0;
			}
		}
		else if (flagState == FlagState.Swing) 
		{
			OnSwingFlag();
		}
		else if (flagState == FlagState.Hide) 
		{
			if (VertialTranslation(flagShowPosition, flagHidePosition, 2f)) 
			{
				flagState = FlagState.Nope;
				flagRelativePosition = 0f;
				flagSupport.SetActive(false);
			}
		}
	}

	bool VertialTranslation(Vector3 initialPosition, Vector3 finalPosition, float flagTranslationSpeed)
	{
		bool isInFinalPosition = false;
		flagRelativePosition += Time.deltaTime * flagTranslationSpeed;

		if (flagRelativePosition <= 1f) 
			transform.localPosition = Vector3.Lerp (initialPosition, finalPosition, flagRelativePosition);
		else 
		{
			transform.localPosition = finalPosition;
			isInFinalPosition = true;
		}
		return isInFinalPosition;
	}

	void OnSwingFlag()
	{
		float currentRotation = transform.eulerAngles.z;
		if (currentRotation < 60f && !switchFlagRotation) 
		{
			switchFlagRotation = true;
			cloth.externalAcceleration = new Vector3(120f, 0f, 0f);
			swingCounter++;
		} 
		else if (currentRotation > 160f && switchFlagRotation) 
		{
			switchFlagRotation = false;
			cloth.externalAcceleration = new Vector3(-120f, 0f, 0f);
		}

		if (switchFlagRotation) 
		{
			if (swingCounter < 2) 
				transform.Rotate (swingAngle, 10f);
			else if (currentRotation < 100f) 
				transform.Rotate (swingAngle, 10f);
		}
		else 
		{
			transform.Rotate (swingAngle, -10f);
		}

	}

	public void HideFlag()
	{
		flagState = FlagState.Hide;
	}

	public void StartFlagSwing(int ruleIndex)
	{
		flagMesh.material = materials [ruleIndex];
		switchFlagRotation = false;
		flagSupport.SetActive(true);
		transform.eulerAngles = new Vector3(0f, 0f, 59f);
		flagState = FlagState.Show;
	}
}

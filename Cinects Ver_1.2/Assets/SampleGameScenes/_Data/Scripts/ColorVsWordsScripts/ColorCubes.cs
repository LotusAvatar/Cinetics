using UnityEngine;
using System;
using System.Collections;

public enum BallState
{
	DefendedRight = 0, 
	DefendedWrong,
	ShowRight,
	Disapear,
	OnMove
}

public class ColorCubes : MonoBehaviour {

    public Color cubeColor;
    public int colorID;
	public Transform meshBall;
	public Material[] materials;
    public static event Action<int> OnHitCube;
	bool onScale;
	bool onRotation;
	bool onTwinkle;
	bool switchTwinkle;
	bool canCollideWithPlayer;
	float scaleSpeed = 0.5f;
	float relativeScale;
	float rotationAngle;
	float alphaFactor;
	float twinkleTimer;
	float twinkleLimit = 1f;
	Vector3 rotationAxis;
	Vector3 initialBallScale;
	Vector3 finalBallScale;
	BallState ballState = BallState.OnMove;
	Color[] defaultColors = new Color[2];
	Color twinkleColor;

	public bool CanCollideWithPlayer 
	{
		get {return this.canCollideWithPlayer;}
		set {canCollideWithPlayer = value;}
	}

	public float AlphaFactor 
	{
		set {alphaFactor = value;}
	}

	public BallState BallState 
	{
		get {return this.ballState;}
	}

	public void Start()
	{
		initialBallScale = Vector3.one * 0.4f;
		finalBallScale = Vector3.one * 0.8f;
		for (int i = 0; i < materials.Length; i++) 
		{
			defaultColors[i] = materials[i].color;
			defaultColors[i].a = 1f;
			materials[i].color = defaultColors[i];
		}
	}

	void Update()
	{
		if (onRotation) 
		{
			OnBallRotation();
		}
		if (onScale) 
		{
			OnBallScale();
		}
		if (onTwinkle) 
		{
			OnTwinkle();
		}
		if (ballState == BallState.Disapear) 
		{
			OnDisapear();
		}
	}

	void TwinkleBall(Color setTwinkleColor)
	{
		twinkleColor = setTwinkleColor;
		twinkleTimer = 0f;
		onTwinkle = true;
	}

	public void ResetBallState()
	{
		onRotation = false;
		meshBall.localScale = initialBallScale;
		ballState = BallState.OnMove;
		for (int i = 0; i < materials.Length; i++) 
		{
			materials[i].color = defaultColors[i];
		}
	}

	public void OnMovement(bool followBezier = false)
	{
		rotationAngle = UnityEngine.Random.Range (-5f, 5f);
		rotationAxis = UnityEngine.Random.onUnitSphere;

		onScale = !followBezier;
		onRotation = true;
	}

	public void SetBallState(BallState nextState)
	{
		if (nextState == BallState.OnMove) 
		{
			ballState = nextState;
			return;
		}
		switch (nextState) 
		{
		case BallState.DefendedRight: OnDefendRight(); break;
		case BallState.DefendedWrong: OnDefendWrong(); break;
		case BallState.Disapear: canCollideWithPlayer = false; OnDisapear(); break;
		default: break;
		}
		if (ballState == BallState.OnMove) 
			ballState = nextState;
	}

	void OnDefendRight()
	{
		onRotation = false;
		TwinkleBall (Color.white);
	}

	void OnDefendWrong()
	{
		onRotation = false;
		TwinkleBall (Color.black);
	}

	void OnTwinkle()
	{
		twinkleTimer += Time.deltaTime;
		if (twinkleTimer >= twinkleLimit) 
		{
			onTwinkle = false;
			for (int i = 0; i < materials.Length; i++) 
			{
				materials[i].color = defaultColors[i];
			}
		} 
		else 
		{
			for (int i = 0; i < materials.Length; i++) 
			{
				if (switchTwinkle) 
				{
					materials[i].color = defaultColors[i];
				}
				else
				{
					materials[i].color = twinkleColor;
				}
			}
			switchTwinkle = !switchTwinkle;
		}
	}

	void OnDisapear()
	{
		float alpha = materials[0].color.a;
		alpha -= Time.deltaTime * alphaFactor;
		if (alpha <= 0f) 
		{
			for (int i = 0; i < materials.Length; i++) 
			{
				Color myColor = materials [i].color;
				myColor.a = 0f;
				materials [i].color = myColor;
			}
		} 
		else 
		{
			for (int i = 0; i < materials.Length; i++) 
			{
				Color myColor = materials[i].color;
				myColor.a = alpha;
				materials[i].color = myColor;
			}
		}
	}

    void OnBallRotation()
	{
		meshBall.Rotate (rotationAxis, rotationAngle);
	}

	void OnBallScale()
	{
		relativeScale += Time.deltaTime * scaleSpeed;
		if (relativeScale <= 1f) 
		{
			meshBall.transform.localScale = Vector3.Lerp(initialBallScale, finalBallScale, relativeScale);
		} 
		else 
		{
			onScale = false;
			relativeScale = 0f;
			meshBall.transform.localScale = finalBallScale;
		}
	}

	void OnTriggerEnter(Collider hit)
    {
        if (hit.tag == "Player" && canCollideWithPlayer)
        {
            if (OnHitCube != null)
            {
                OnHitCube(colorID);
            }
        }
		else if (hit.tag == "Ground") 
		{
			ColorManager.Instance.BounceDirection(colorID);
		}
    }
}

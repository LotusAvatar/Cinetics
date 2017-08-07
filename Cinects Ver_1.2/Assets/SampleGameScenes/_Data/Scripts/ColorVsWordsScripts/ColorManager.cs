using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum FeedbackTypes
{ 
	Gain = 0,
	Loss, 
	Miss,
	None
}

public enum Difficulty
{
	Easy,
	Normal,
	Hard
}

public class ColorManager : MonoBehaviour 
{
	enum ColorBallsState
	{
		HasExplode,
		GotDirection,
		afterBallLimitPosition,
		Finish
	}

    enum ValidDraw
    { 
        Color = 0,
        Text
    }

    ValidDraw validDraw = ValidDraw.Color;
	FeedbackTypes currentFeedback = FeedbackTypes.Gain;

	// Use this for initialization
	public Material[] colorMaterials;
	public BezierManager bezierManager;
	public WordsInLed wordsInLed;
	public FlagBehavior flag;
    public TextMesh score;
    public TextMesh series;
    public TextMesh difficultyFeedback;
	public GoalController goalController;
	public Transform cameraPos;
	public Transform whiteBall;
	public Transform feedbackParticlePosition;
	public Transform[] ballFinalPositions;
	public Transform[] balls;
	public GameObject[] ballsFeedback;
    public Color[] ballsFeedbackColors;
    public float ballsSpeed = 1f;
	public ParticleSystem[] fireworkParticles;
	public ParticleSystem kickParticle;
	public ParticleSystem divideBallParticle;
	public ParticleSystem poofParticle;
	public ParticleSystem explodeParticle;
	public string easy;
	public string normal; 
	public string hard;

	public static event Action<FeedbackTypes> FeedbackEvent;

	public Difficulty difficulty = Difficulty.Easy;
	Difficulty lastDifficulty = Difficulty.Easy;

	ColorBallsState colorBallState;
	Vector3 initialBallPos = new Vector3 ();
	Vector3 hideBallPos;
	Vector3[] currentBallsFinalPos = new Vector3[4];
	bool[] occupiedPositions = new bool[4];
    int[] fireworkAddress = new int[5];

    float timerFeedback;
	float alpha;
	float relativeBallsPosition = 0f;
	float delayAfterDefence = 2f;
	float delayToDisapear = 1f;
	float timeAfterDefence;
    float fireworkDelay = 0f;
    int addressCounter = 0;
    int kicksPerPlayer = 5;
	int totalKicks = 15;
	int currentKicks = 0;
	int countKicks;
	int countSeries;
    int scoreAmount;
    int textIndex = 0;
    int colorIndex = 0;
    int validIndex = 0;
    bool hasHit = false;
	bool playTime = false;
	bool ballsOnMove = false;
	bool gotDirections;
	bool afterBallLimitPosition;
	bool dismissAlfaBalls = false;
	bool canCallToShoot = true;
	bool gamePaused = false;
    bool shootFirework = false;
	Color feedbackColor = new Color();
	Vector3[] ballDirections = new Vector3[4];
    System.Random rand = new System.Random();

    public bool GamePaused 
	{
		get {return this.gamePaused;}
	}
    
	public FeedbackTypes CurrentFeedback 
	{
		get {
			return this.currentFeedback;
		}
		set {
			currentFeedback = value;
		}
	}

	public Vector3 HideBallPos 
	{
		get {return this.hideBallPos;}
	}

    #region Sigleton

    private static ColorManager _instance = null;

    public static ColorManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ColorManager>();
            }

            return _instance;
        }
    }

    #endregion

    void Awake()
    {
        ColorCubes.OnHitCube += PrintColor;
    }

    void OnDestroy()
    {
        ColorCubes.OnHitCube -= PrintColor;
    }

	void Start () 
    {
		for (int i = 0; i < occupiedPositions.Length; i++) 
		{
			occupiedPositions[i] = false;
		}
		for (int i = 0; i < balls.Length; i++) 
		{
			balls [i].GetComponent<ColorCubes>().AlphaFactor = 1f / delayToDisapear;
		}
		hideBallPos = whiteBall.position;

		lastDifficulty = difficulty;

		switch (difficulty) 
		{
		case Difficulty.Easy: difficultyFeedback.text = easy; difficultyFeedback.color = Color.green; break;
		case Difficulty.Normal: difficultyFeedback.text = normal; difficultyFeedback.color = Color.yellow; break;
		case Difficulty.Hard: difficultyFeedback.text = hard; difficultyFeedback.color = Color.red; break;
		default: break;
		}
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        CheckForUserDetection();
		if (ballsOnMove) 
		{
			OnMoveBall();
		}
		
		if (canCallToShoot) 
		{
			canCallToShoot = false;
			PlayersBehaviorsController.Instance.CallPlayerToShoot();
		}
		if (lastDifficulty != difficulty) 
		{
			lastDifficulty = difficulty;
			switch (difficulty) 
			{
			case Difficulty.Easy: difficultyFeedback.text = easy; difficultyFeedback.color = Color.green; break;
			case Difficulty.Normal: difficultyFeedback.text = normal; difficultyFeedback.color = Color.yellow; break;
			case Difficulty.Hard: difficultyFeedback.text = hard; difficultyFeedback.color = Color.red; break;
			default: break;
			}
		}
        if (shootFirework)
        {
            if (fireworkDelay > 0f)
                fireworkDelay -= Time.fixedDeltaTime;
            else
            {
                if (addressCounter == fireworkParticles.Length)
                {
                    shootFirework = false;
                    return;
                }
                fireworkParticles[fireworkAddress[addressCounter]].gameObject.SetActive(true);
                addressCounter++;
                fireworkDelay = ((float)rand.Next(2, 5)) / 10f;
            }
        }
	}

	void Feedback()
	{
		switch (currentFeedback) 
		{
		case FeedbackTypes.Gain: FeedbackGain(); break;
		case FeedbackTypes.Loss: FeedbackLoss(); break;
		case FeedbackTypes.Miss: FeedbackMiss(); break;
		default: break;
		}
		UpdateInterface();
		if (FeedbackEvent != null)
			FeedbackEvent(currentFeedback);

		goalController.UpdateGoal (totalKicks, currentKicks);
		currentFeedback = FeedbackTypes.None;
	}

	void FeedbackGain()
	{
//		scoreAmount++;
		scoreAmount += 5;
		Renderer rend = ballsFeedback [countKicks - 1].GetComponent<Renderer> ();
		rend.material.shader = Shader.Find("Particles/VertexLit Blended");
		rend.material.SetColor("_EmisColor", ballsFeedbackColors[0]);
	}

	void FeedbackLoss()
	{
		if (scoreAmount >= 3) 
			scoreAmount -= 3;
		else
			scoreAmount = 0;

		Renderer rend = ballsFeedback [countKicks - 1].GetComponent<Renderer> ();
		rend.material.shader = Shader.Find("Particles/VertexLit Blended");
		rend.material.SetColor("_EmisColor", ballsFeedbackColors[1]);
	}

	void FeedbackMiss()
	{
        if (scoreAmount >= 3)
            scoreAmount -= 3;
        else
            scoreAmount = 0;

        Renderer rend = ballsFeedback [countKicks - 1].GetComponent<Renderer> ();
		rend.material.shader = Shader.Find("Particles/VertexLit Blended");
		rend.material.SetColor("_EmisColor", Color.red);
	}

    void CheckForUserDetection()
    {
        if (playTime != KinectManager.Instance.IsUserDetected())
            OnSessionBehavior(KinectManager.Instance.IsUserDetected());
    }

	void OnSessionBehavior(bool canPlay)
	{
        if (canPlay)
        {
            timerFeedback = 1f;
//            feedback.text = "Start!";
//            feedback.color = Color.blue;
//			Time.timeScale = 1f;
        }
        else
        {
//		    feedback.text = "Pause";
//		    feedback.color = Color.blue;
//			Time.timeScale = 0f;
        }
        playTime = canPlay;
	}

	void SortNextRule()
    {
		for (int i = 0; i < balls.Length; i++) 
		{

			balls[i].transform.position = hideBallPos;
			balls[i].GetComponent<ColorCubes>().ResetBallState();
		}
        int randText = UnityEngine.Random.Range(1, 4);
        int randColor = UnityEngine.Random.Range(1, 4);

        colorIndex += randColor;
        colorIndex %= 4;
        textIndex += randText;
        textIndex %= 4;

		if (difficulty == Difficulty.Easy) 
		{
			colorIndex = textIndex;
		}

        validDraw = (ValidDraw)UnityEngine.Random.Range(0, 2);

        if (validDraw == ValidDraw.Color)
            validIndex = colorIndex;
        else
            validIndex = textIndex;
        hasHit = false;
		flag.StartFlagSwing ((int)validDraw);
		wordsInLed.UpdateNextRule (colorMaterials[colorIndex], textIndex);
    }

    void UpdateInterface()
    {
		if (difficulty == Difficulty.Hard) 
			ballsSpeed = 0.2f + (float)scoreAmount * 0.01f;

		string points = "";
		if (scoreAmount < 100) 
		{
			points += "0";
		}
		if (scoreAmount < 10) 
		{
			points += "0";
		}
		points += scoreAmount.ToString ();
		score.text = points;
    }

	void OnMoveBall()
	{

		relativeBallsPosition += Time.deltaTime * ballsSpeed;

		if (relativeBallsPosition <= 1f) 
		{
			for (int i = 0; i < balls.Length; i++) 
			{
				int indexBallState = (int) balls [i].GetComponent<ColorCubes>().BallState;
				if (indexBallState > 1) 
				{
					balls [i].localPosition = Vector3.Lerp (initialBallPos, currentBallsFinalPos [i], relativeBallsPosition);
					if (colorBallState == ColorBallsState.HasExplode) 
						ballDirections[i] = balls [i].localPosition - initialBallPos;
				}
			}
			if (colorBallState == ColorBallsState.HasExplode) 
				colorBallState = ColorBallsState.GotDirection;
		} 
		else 
		{
			if (colorBallState == ColorBallsState.afterBallLimitPosition) 
			{
				timeAfterDefence += Time.fixedDeltaTime;

				if (!dismissAlfaBalls && timeAfterDefence >= delayToDisapear) 
				{
					dismissAlfaBalls = true;
					for (int i = 0; i < balls.Length; i++) 
					{
						if (balls [i].GetComponent<ColorCubes>().BallState == BallState.Disapear) 
						{
							balls [i].GetComponent<ColorCubes>().ResetBallState();
							balls[i].position = hideBallPos;
						}
					}

				}
				if (timeAfterDefence >= delayAfterDefence) 
				{
					ballsOnMove = false;
					timeAfterDefence = 0f;
					relativeBallsPosition = 0f;
					wordsInLed.RefreshLedPainel();
					flag.HideFlag();
					for (int i = 0; i < balls.Length; i++) 
					{
						if (balls [i].GetComponent<ColorCubes>().BallState != BallState.OnMove) 
						{
							if (balls [i].GetComponent<ColorCubes>().BallState == BallState.DefendedRight ||
							    balls [i].GetComponent<ColorCubes>().BallState == BallState.ShowRight) 
							{
								poofParticle.transform.position = balls [i].position;
								poofParticle.gameObject.SetActive(true);
							}
							else if (balls [i].GetComponent<ColorCubes>().BallState == BallState.DefendedWrong) 
							{
								explodeParticle.transform.position = balls [i].position;
								explodeParticle.gameObject.SetActive(true);
							}
							balls [i].GetComponent<ColorCubes>().ResetBallState();
							balls[i].position = hideBallPos;
						}
					}
				}
				else
				{
					for (int i = 0; i < balls.Length; i++) 
					{
						int indexBallState = (int) balls [i].GetComponent<ColorCubes>().BallState;
						if (indexBallState > 1) 
						{
							balls [i].localPosition += ballDirections[i]*0.5f;
						}
					}
				}
			}
			else if (colorBallState == ColorBallsState.GotDirection)
			{
				PrintColor();
				Feedback();

				for (int i = 0; i < balls.Length; i++)
					balls[i].GetComponent<ColorCubes>().CanCollideWithPlayer = false;

				colorBallState = ColorBallsState.afterBallLimitPosition;
			}
		}
	}

	public bool NotFinishKicking()
	{
		return countKicks < kicksPerPlayer;
	}

	public void ResetKicks()
	{
		countKicks = 0;
		countSeries++;

		if (countSeries < 100) 
		{
			if (countSeries < 10) 
				series.text = "0"+ countSeries.ToString();
			else
				series.text = countSeries.ToString();
		}

		for (int i = 0; i < ballsFeedback.Length; i++) 
		{
			Renderer rend = ballsFeedback [i].GetComponent<Renderer> ();
			rend.material.shader = Shader.Find("Particles/VertexLit Blended");
			rend.material.SetColor("_EmisColor", Color.white);
		}
	}

	public void BounceDirection(int index)
	{
		Vector3 bounceDirection = ballDirections[index];
		bounceDirection.y *= -1f;
		ballDirections [index] = bounceDirection;
	}

	public void SetWhiteBallPosition(Transform ballTransform)
	{
		whiteBall.position = ballTransform.position;
		whiteBall.rotation = ballTransform.rotation;
		countKicks++;
		currentKicks++;
	}

	public void PlayerKick()
	{
		Bezier[] randBezier = bezierManager.GetRandomBezier (whiteBall.position);
		kickParticle.transform.position = whiteBall.position;
		kickParticle.gameObject.SetActive (true);
		whiteBall.GetComponent<BallBezierTravaler> ().Play(randBezier);
		hasHit = false;
		dismissAlfaBalls = false;
		SortNextRule ();
	}

	public void ExplodeBall()
	{
		initialBallPos = whiteBall.localPosition;
		Vector3 divideBallParticlePos = cameraPos.position - whiteBall.position;
		divideBallParticlePos = Vector3.Normalize (divideBallParticlePos)*2f;
		divideBallParticlePos += whiteBall.position;
		divideBallParticle.transform.position = divideBallParticlePos;
		divideBallParticle.gameObject.SetActive (true);
		for (int i = 0; i < balls.Length; i++) 
		{
			balls[i].GetComponent<ColorCubes>().CanCollideWithPlayer = true;
			balls[i].GetComponent<SphereCollider>().enabled = true;
			balls[i].position = whiteBall.position;
			balls[i].rotation = whiteBall.rotation;
			occupiedPositions[i] = false;
		}
		whiteBall.position = hideBallPos;
		bool allPositionsOk = false;
		int count = 0;
		
		do 
		{
			int randPos = UnityEngine.Random.Range(0, 4);
			
			if (occupiedPositions[randPos] == false) 
			{
				occupiedPositions[randPos] = true;
				currentBallsFinalPos[count] = ballFinalPositions[randPos].localPosition;
				count++;
				allPositionsOk = true;
				
				for (int i = 0; i < occupiedPositions.Length; i++) 
				{
					if (occupiedPositions[i] == false) 
					{
						allPositionsOk = false;
						break;
					}
				}
			}
			
		} while (!allPositionsOk);
		for (int i = 0; i < balls.Length; i++) 
		{
			balls[i].GetComponent<ColorCubes>().OnMovement();
		}
		colorBallState = ColorBallsState.HasExplode;
		ballsOnMove = true;
	}

    public void PrintColor(int colorID = -1)
    {
        if (!hasHit)
        {
			bool hasDefended = false;
			for (int i = 0; i < balls.Length; i++) 
			{
				if (balls[i].GetComponent<ColorCubes>().colorID == colorID) 
				{
					if (colorID == validIndex) 
					{
						currentFeedback = FeedbackTypes.Gain;
						balls[i].GetComponent<ColorCubes>().SetBallState(BallState.DefendedRight);
                        EnableFirework();
                        hasDefended = true;
					}
					else
					{
						currentFeedback = FeedbackTypes.Loss;
						balls[i].GetComponent<ColorCubes>().SetBallState(BallState.DefendedWrong);
						hasDefended = true;
					}
				}
				else
				{
					if (balls[i].GetComponent<ColorCubes>().colorID == validIndex) 
						balls[i].GetComponent<ColorCubes>().SetBallState(BallState.ShowRight);
					else
						balls[i].GetComponent<ColorCubes>().SetBallState(BallState.Disapear);
				}
			}

			if (!hasDefended) 
				currentFeedback = FeedbackTypes.Miss;
			
            hasHit = true;
        }
    }
    void EnableFirework()
    {
        for (int i = 0; i < fireworkAddress.Length; i++)
            fireworkAddress[i] = -1;

        addressCounter = 0;
        int nextAddress = 0;

        do
        {
            nextAddress = rand.Next(0, 5);
            bool inArray = false;
            for (int i = 0; i < fireworkAddress.Length; i++)
            {
                if (fireworkAddress[i] != -1 && fireworkAddress[i] == nextAddress)
                {
                    inArray = true;
                    break;
                }
            }
            if (!inArray)
            {
                fireworkAddress[addressCounter] = nextAddress;
                addressCounter++;
            }

        } while (addressCounter < fireworkAddress.Length);
        shootFirework = true;
        addressCounter = 0;
    }
}

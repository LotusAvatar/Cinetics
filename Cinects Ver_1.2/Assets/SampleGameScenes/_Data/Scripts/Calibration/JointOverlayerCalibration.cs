using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BendingDirection
{
    Straight,
    Right,
    Left
}

public enum CalibrationState
{
    Nope = 0,
    GetPlayerSkeleton,
    Recognizing,
    HandChoice,
    OtherHandLeaving,
    HandToMid,
    HandIndoDeep,
    MovingCollider,
    AmplifyingReach
}

public class JointOverlayerCalibration : MonoBehaviour
{
    [Tooltip("Text for debugging")]
    public UILabel debugText;

    [Tooltip("Filled circle sprite reference.")]
    public UISprite cursor;

    [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
    public int playerIndex = 0;

	[Tooltip("Dynamicly expands the hand reach")]
	public bool dynamic = true;

    public Transform invisibleBall;
    public Transform head;
    public Transform rightHandInitialRef;
    public Transform leftHandInitialRef;
    public Transform rightHandScreenRef;
    public Transform leftHandScreenRef;
    public Transform handMiddleRef;
    public Transform headDeepRef;

    public GameObject leftHandBorder;
    public GameObject rightHandBorder;
    public GameObject leftHandModel;
    public GameObject rightHandModel;
    public MeshRenderer rightBall;
    public MeshRenderer leftBall;

    public ParticleSystem turnOnRight;
    public ParticleSystem turnOnLeft;
    public ParticleSystem rightEletric;
    public ParticleSystem leftEletric;
    public ParticleSystem miscParticle;
    public ParticleSystem explodeParticle;
    public ParticleSystem[] rightSpark;
    public ParticleSystem[] leftSpark;

    public InterfaceController interfaceController;

    public Color middleColor;
    public Color finalColor;

    GUIText decoyText;
    float ringDelay = 0.2f;
    float rightShake = 0.2f;
    float leftShake = 0.2f;
    float handParticlesProgress = 0f;
    float headBallProgress = 0f;
    float movementProgress = 0f;
    float blinkProgress = 0f;
    bool rightSwithPos;
    bool leftSwithPos;
    bool isIboxValid;
    bool circleScreenPosInited;
    bool userDetected;
    Vector3 rightModelHandPos = new Vector3(3f, 0f, -5.26f);
    Vector3 leftModelHandPos = new Vector3(-3f, 0f, -5.26f);
    Vector3 handPos = Vector3.zero;
    Vector3 spineMidPos = Vector3.zero;
    Vector3 dynamicIboxLeftBotBack = Vector3.zero;
    Vector3 dynamicIboxRightTopFront = Vector3.zero;
    Vector3 handSpineMidDistance = Vector3.zero;
    Vector3 IboxLeftBotBack = Vector3.zero;
    Vector3 handScreenPos = Vector3.zero;
    Vector3 circleScreenPos = Vector3.zero;
    Vector3 handMaxLimitPos = new Vector3(30f, 16f, 30f);
	Vector3 handMinLimitPos = new Vector3(-30f, -16f, 30f);
    Vector3 currentHandPos = new Vector3();
    Vector3 IboxRightTopFront = Vector3.zero;
    Vector3 referenceInitialPos = new Vector3();

	Transform initialRef;
	Transform finalRef;
    UISprite spriteProgress;

    Quaternion headInitRot = new Quaternion();

    KinectInterop.JointType nextJoint;

    BendingDirection currentDirection;
    BendingDirection lastDirection;

    CalibrationState currentState;
    CalibrationState lastState;

    private bool handPositionInited;

    public bool HandPositionInited
    {
        get { return handPositionInited; }
        set { handPositionInited = value; }
    }

    private bool useFixedIBox;

    public bool UseFixedIBox
    {
        get { return useFixedIBox; }
        set { useFixedIBox = value; }
    }


    private static JointOverlayerCalibration instance;
    /// <summary>
    /// Gets the single JointOverlayerCalibration instance.
    /// </summary>
    /// <value>The JointOverlayerCalibration instance.</value>
    public static JointOverlayerCalibration Instance
	{ 
		get
		{
			if (instance == null)
			{
				instance = (JointOverlayerCalibration)GameObject.FindObjectOfType(typeof(JointOverlayerCalibration));
			}
			return instance;
		}
	}

    void Start()
    {
        currentState = CalibrationState.GetPlayerSkeleton;
        headInitRot = head.transform.rotation;
    }

    void LateUpdate()
    {
        StateChecker();
        if (handPositionInited && !useFixedIBox)
        {
            OnHandMovement();
        }
        if (!userDetected)
        {
            if (PlayerData.Instance.IsUserDetected && currentState == CalibrationState.GetPlayerSkeleton)
            {
                userDetected = PlayerData.Instance.IsUserDetected;
                currentState = CalibrationState.Recognizing;
            }
        }
    }

    void StateChecker()
    {
        if (lastState != currentState)
        {
            switch (currentState)
            {
                
                case CalibrationState.GetPlayerSkeleton:
                    interfaceController.EnablePopUp(PopUps.HeadMovement);
                    break;
                case CalibrationState.HandIndoDeep:
                    interfaceController.EnablePopUp(PopUps.HandMovement);
                    break;
                case CalibrationState.MovingCollider:
                    break;
                case CalibrationState.AmplifyingReach:
                    break;
                default:
                    break;
            }

        }
        lastState = currentState;
    }

    Vector3 CalculatePosition(Vector3 screenNormalPos, ref Vector3 currentPos)
    {
        currentPos.x = Mathf.Lerp(handMinLimitPos.x, handMaxLimitPos.x, screenNormalPos.x);
        currentPos.y = Mathf.Lerp(handMinLimitPos.y, handMaxLimitPos.y, screenNormalPos.y);
        currentPos.z = Mathf.Lerp(handMinLimitPos.z, handMaxLimitPos.z, screenNormalPos.z);
        return currentPos;
    }

    void OnHandMovement()
    {
        try
        {
            invisibleBall.transform.localPosition = CalculatePosition(handScreenPos, ref currentHandPos);
        }
        catch (System.Exception)
        {
            Debug.Log("Position not defined");
            throw;
        }
    }

    void CursorMovement()
    {
        //if (circleScreenPos.y >= 0.5666f)
        //    circleScreenPos.y = 0.5666f;
        //else if (circleScreenPos.y <= 0.3087f)
        //    circleScreenPos.y = 0.3087f;

        Debug.Log(circleScreenPos);

        Vector3 objectFilledSpritePosition = new Vector3(circleScreenPos.x * Screen.width - Screen.width / 2,
        circleScreenPos.y * Screen.height - Screen.height / 2, 0f);

        if (circleScreenPosInited)
        {
            cursor.transform.localPosition = objectFilledSpritePosition;
            MouseControl.MouseMove(circleScreenPos, decoyText);
            
        }
        else
            circleScreenPosInited = true;

    }

    public void UpdateScreenPos(Vector3 _handScreenPos, Vector3 _circleScreenPos)
    {
        handScreenPos = _handScreenPos;
        circleScreenPos = _circleScreenPos;
    }

    public void ContinueHandAnimation(CalibrationState nextState)
    {
        movementProgress = 0f;
        currentState = nextState;
    }

    public void UpdateHeadAngle(Quaternion headOrientation)
    {
        switch (currentState)
        {
            case CalibrationState.Nope:
                break;
            case CalibrationState.Recognizing:
                OnRecognizing();
                break;
            case CalibrationState.HandChoice:
                OnHandChoice(headOrientation);
                break;
            case CalibrationState.OtherHandLeaving:
                OnOtherHandLeaving();
                break;
            case CalibrationState.HandToMid:
                OnHandToMid();
                break;
            case CalibrationState.HandIndoDeep:
                OnHandIndoDeep();
                break;
            default:
                break;
        }
    }

    public void FindSkeleton()
    {
        ContinueHandAnimation(CalibrationState.GetPlayerSkeleton);
    }

    public void EnableCineticPopUp()
    {
        useFixedIBox = true;
        circleScreenPosInited = true;
        interfaceController.EnablePopUp(PopUps.Cinetic);
    }

    void OnRecognizing()
    {
        movementProgress += Time.deltaTime * 2f;
        if (movementProgress < 1f)
        {
            OnMovement(leftHandModel, leftHandInitialRef, leftHandScreenRef, movementProgress);
            OnMovement(rightHandModel, rightHandInitialRef, rightHandScreenRef, movementProgress);
        }
        else
        {
            OnMovement(leftHandModel, leftHandInitialRef, leftHandScreenRef, 1f);
            OnMovement(rightHandModel, rightHandInitialRef, rightHandScreenRef, 1f);
            for (int i = 0; i < 3; i++)
            {
                rightSpark[i].gameObject.SetActive(true);
                leftSpark[i].gameObject.SetActive(true);
            }
            currentState = CalibrationState.HandChoice;
        }
    }

    void OnHandChoice(Quaternion headOrientation)
    {
        Quaternion rot = headOrientation;
        rot *= headInitRot;
        head.rotation = rot;

        //string text = "";
        //text += "Z: " + head.localEulerAngles.z;
        //debugText.text = text;

        if (head.localEulerAngles.z > 10f && head.localEulerAngles.z < 180f)
            currentDirection = BendingDirection.Left;
        else if (head.localEulerAngles.z < 350f && head.localEulerAngles.z > 180f)
            currentDirection = BendingDirection.Right;
        else
            currentDirection = BendingDirection.Straight;

        if (lastDirection != currentDirection)
        {
            Vector3 circlePos = cursor.transform.localPosition;
            switch (currentDirection)
            {
                case BendingDirection.Straight:
                    leftHandBorder.SetActive(false);
                    rightHandBorder.SetActive(false);
                    SetMeshRendererCollor(rightBall, Color.red, middleColor, 0f);
                    SetMeshRendererCollor(leftBall, Color.red, middleColor, 0f);
                    handParticlesProgress = 0f;
                    leftEletric.gameObject.SetActive(false);
                    rightEletric.gameObject.SetActive(false);
                    for (int i = 0; i < 3; i++)
                    {
                        rightSpark[i].gameObject.SetActive(true);
                        leftSpark[i].gameObject.SetActive(true);
                    }
                    break;
                case BendingDirection.Right:
                    rightHandBorder.SetActive(true);
                    circlePos.x = 485f;
                    cursor.transform.localPosition = circlePos;
                    break;
                case BendingDirection.Left:
                    leftHandBorder.SetActive(true);
                    circlePos.x = -485f;
                    cursor.transform.localPosition = circlePos;
                    break;
                default:
                    break;
            }
        }

        if (currentDirection != BendingDirection.Straight)
            headBallProgress += Time.deltaTime * 0.4f;
        else
            headBallProgress = 0f;

        if (headBallProgress >= 1f)
        {
            //cursor.fillAmount = 0f;
            if (currentDirection == BendingDirection.Right)
            {
                nextJoint = KinectInterop.JointType.HandRight;
                turnOnRight.gameObject.SetActive(true);
            }
            else if (currentDirection == BendingDirection.Left)
            {
                nextJoint = KinectInterop.JointType.HandLeft;
                turnOnLeft.gameObject.SetActive(true);
            }

            SetParticlesOnHandProgress(currentDirection, 1f);

            CalibrationHoneycombMatrix.Instance.OnHandChosen(currentDirection);
            currentState = CalibrationState.OtherHandLeaving;
            movementProgress = 0f;
            //leftHandModel.SetActive(false);
            //rightHandModel.SetActive(false);
            return;
        }
        if (currentDirection == BendingDirection.Right)
        {
            ShakingHand(rightHandModel, rightHandScreenRef, 1f - headBallProgress, rightSwithPos, rightShake);
            ShakingHand(leftHandModel, leftHandScreenRef, 1f, leftSwithPos, leftShake);
            SetMeshRendererCollor(rightBall, Color.red, middleColor, headBallProgress);
            SetParticlesOnHandProgress(currentDirection, headBallProgress);
        }
        else if (currentDirection == BendingDirection.Left)
        {
            ShakingHand(leftHandModel, leftHandScreenRef, 1f - headBallProgress, leftSwithPos, leftShake);
            ShakingHand(rightHandModel, rightHandScreenRef, 1f, rightSwithPos, rightShake);
            SetMeshRendererCollor(leftBall, Color.red, middleColor, headBallProgress);
            SetParticlesOnHandProgress(currentDirection, headBallProgress);
        }
        else
        {
            ShakingHand(leftHandModel, leftHandScreenRef, 1f, leftSwithPos, leftShake);
            ShakingHand(rightHandModel, rightHandScreenRef, 1f, rightSwithPos, rightShake);
        }

        lastDirection = currentDirection;
    }

    void OnOtherHandLeaving()
    {
        movementProgress += Time.deltaTime * 2f;
        if (currentDirection == BendingDirection.Right)
        {
            if (movementProgress < 1f)
                OnMovement(leftHandModel, leftHandScreenRef, leftHandInitialRef, movementProgress);
            else
            {
                OnMovement(leftHandModel, leftHandScreenRef, leftHandInitialRef, 1f);
                leftHandModel.SetActive(false);
                currentState = CalibrationState.Nope;
            }
        }
        else if (currentDirection == BendingDirection.Left)
        {
            if (movementProgress < 1f)
                OnMovement(rightHandModel, rightHandScreenRef, rightHandInitialRef, movementProgress);
            else
            {
                OnMovement(rightHandModel, rightHandScreenRef, rightHandInitialRef, 1f);
                rightHandModel.SetActive(false);
                currentState = CalibrationState.Nope;
            }
        }
    }

    void OnGetPlayerSkeleton()
    {
        interfaceController.SetInterfaceState(InterfaceState.Clear);
        interfaceController.EnablePopUp(PopUps.HeadMovement);
    }

    void OnHandToMid()
    {
        movementProgress += Time.deltaTime * 0.3f;
        blinkProgress += Time.deltaTime;
        if (currentDirection == BendingDirection.Left)
        {
            if (movementProgress < 1f)
            {
                SetMeshRendererCollor(leftBall, middleColor, finalColor, movementProgress);
                OnMovement(leftHandModel, leftHandModel.transform, handMiddleRef, movementProgress);
                if (blinkProgress > 0.2f)
                {
                    blinkProgress = 0f;
                    leftSwithPos = !leftSwithPos;
                    leftHandBorder.SetActive(leftSwithPos);
                }
            }
            else
            {
                SetMeshRendererCollor(leftBall, middleColor, finalColor, 1f);
                leftHandBorder.SetActive(false);
                OnMovement(leftHandModel, leftHandScreenRef, handMiddleRef, 1f);
                movementProgress = 0f;
                currentState = CalibrationState.HandIndoDeep;
                interfaceController.EnablePopUp(PopUps.HandMovement);
            }
        }
        else if (currentDirection == BendingDirection.Right)
        {
            if (movementProgress < 1f)
            {
                SetMeshRendererCollor(rightBall, middleColor, finalColor, movementProgress);
                OnMovement(rightHandModel, rightHandModel.transform, handMiddleRef, movementProgress);
                if (blinkProgress > 0.2f)
                {
                    blinkProgress = 0f;
                    rightSwithPos = !rightSwithPos;
                    rightHandBorder.SetActive(rightSwithPos);
                }
            }
            else
            {
                SetMeshRendererCollor(rightBall, middleColor, finalColor, 1f);
                rightHandBorder.SetActive(false);
                OnMovement(rightHandModel, rightHandScreenRef, handMiddleRef, 1f);
                movementProgress = 0f;
                currentState = CalibrationState.HandIndoDeep;
                interfaceController.EnablePopUp(PopUps.HandMovement);
            }
        }
    }

    void OnHandIndoDeep()
    {
        movementProgress += Time.deltaTime * 0.2f;
        if (currentDirection == BendingDirection.Left)
        {
            if (movementProgress < 0.2f)
                OnMovement(leftHandModel, leftHandModel.transform, headDeepRef, movementProgress);
            else
            {
                OnMovement(leftHandModel, handMiddleRef, headDeepRef, 1f);
                movementProgress = 0f;
                leftHandModel.SetActive(false);
                invisibleBall.gameObject.SetActive(true);
                miscParticle.gameObject.SetActive(true);
                Invoke("ToMovingCollider", 0.5f);
                currentState = CalibrationState.Nope;
            }
        }
        else if (currentDirection == BendingDirection.Right)
        {
            if (movementProgress < 0.2f)
                OnMovement(rightHandModel, rightHandModel.transform, headDeepRef, movementProgress);
            else
            {
                OnMovement(rightHandModel, handMiddleRef, headDeepRef, 1f);
                movementProgress = 0f;
                rightHandModel.SetActive(false);
                invisibleBall.gameObject.SetActive(true);
                miscParticle.gameObject.SetActive(true);
                Invoke("ToMovingCollider", 1f);
                currentState = CalibrationState.Nope;
            }
        }
    }

    void ToMovingCollider()
    {
        //ContinueHandAnimation(CalibrationState.MovingCollider);
        explodeParticle.gameObject.SetActive(true);
        PlayerData.Instance.trackedJoint = nextJoint;
    }

    void SetMeshRendererCollor(MeshRenderer currentMesh, Color lastColor, Color nextColor, float progress)
    {
        currentMesh.material.color = Color.Lerp(lastColor, nextColor, progress);
    }

    void SetParticlesOnHandProgress(BendingDirection direction, float progress)
    {
        if (progress > handParticlesProgress)
        {
            ParticleSystem[] sparks = new ParticleSystem[3];
            ParticleSystem eletric = new ParticleSystem();
            if (direction == BendingDirection.Right)
            {
                sparks = rightSpark;
                eletric = rightEletric;
            }
            else if (direction == BendingDirection.Left)
            {
                sparks = leftSpark;
                eletric = leftEletric;
            }
            if (progress == 1f)
            {
                eletric.gameObject.SetActive(false);
            }
            else if (progress > 0.48f)
            {
                eletric.gameObject.SetActive(true);
                sparks[0].gameObject.SetActive(false);
            }
            else if (progress > 0.32f)
            {
                sparks[1].gameObject.SetActive(false);
                handParticlesProgress += 0.16f;
            }
            else if (progress > 0.16f)
            {
                sparks[2].gameObject.SetActive(false);
                handParticlesProgress += 0.16f;
            }
        }
    }

    void ShakingHand(GameObject hand, Transform handScreenRef, float sizer, bool swithPos, float shakeProgress)
    {
        shakeProgress += Time.deltaTime;
        if (shakeProgress > ringDelay)
        {
            shakeProgress = 0f;
            swithPos = !swithPos;

            if (swithPos)
            {
                Vector3 sortedPos = new Vector3();
                sortedPos = Random.onUnitSphere;
                sortedPos *= Mathf.Pow(sizer, 2f) * 0.025f;
                sortedPos += handScreenRef.localPosition;
                hand.transform.localPosition = sortedPos;
            }
            else
                hand.transform.localPosition = handScreenRef.localPosition;
        }
    }

    protected virtual void OnMovement(GameObject movingObject, Transform initial, Transform final, float progress)
    {
        Vector3 onProgressPos = new Vector3();
        Vector3 onProgressScale = new Vector3();
        Quaternion onProgressRot = new Quaternion();
        if (progress < 1f)
        {
            onProgressPos = Vector3.Lerp(initial.localPosition, final.localPosition, progress);
            onProgressScale = Vector3.Lerp(initial.localScale, final.localScale, progress);
            onProgressRot = Quaternion.Lerp(initial.localRotation, final.localRotation, progress);
            movingObject.transform.localPosition = onProgressPos;
            movingObject.transform.localScale = onProgressScale;
            movingObject.transform.localRotation = onProgressRot;
        }
        else
        {
            movingObject.transform.localPosition = final.localPosition;
            movingObject.transform.localScale = final.localScale;
            movingObject.transform.localRotation = final.localRotation;
        }
    }

    Transform TransformLerp(Transform initialReference, Transform finalReference, float _progress)
    {
        Transform result = initialReference;
        Vector3 initialPosReference = initialReference.localPosition;
        Vector3 finalPosReference = finalReference.localPosition;
        Quaternion initialRotationReference = initialReference.localRotation;
        Quaternion finalRotationReference = finalReference.localRotation;
        Vector3 stepLocalPosition = Vector3.Lerp(initialPosReference, finalPosReference, _progress);
        Quaternion stepLocalRotation = Quaternion.Lerp(initialRotationReference, finalRotationReference, _progress);
        result.localPosition = stepLocalPosition;
        result.localRotation = stepLocalRotation;

        return result;
    }
}
using UnityEngine;
using System.Collections;

public class JointOverlayerSpaceBlast : MonoBehaviour
{
    [Tooltip("GUI-texture used to display the color camera feed on the scene background.")]
    public GUITexture backgroundImage;

    [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
    public int playerIndex = 0;

    [Tooltip("Game object used to overlay the joint.")]
    public Transform overlayObject;

    [Tooltip("Filled circle sprite reference.")]
    public UISprite filledCircleSprite;

    private Quaternion initialRotation = Quaternion.identity;
    public TargetController targetController;
	bool isIboxValid;
    bool circleScreenPosInited;
    Vector3 handPos = Vector3.zero;
	Vector3 spineMidPos = Vector3.zero;
	Vector3 dynamicIboxLeftBotBack = Vector3.zero;
	Vector3 dynamicIboxRightTopFront = Vector3.zero;
	Vector3 handSpineMidDistance= Vector3.zero;
	Vector3 IboxLeftBotBack = Vector3.zero;
	Vector3 handScreenPos = Vector3.zero;
	Vector3 handMaxLimitPos = new Vector3(7f, 2.7f, 10.35f);
	Vector3 handMinLimitPos = new Vector3(-7f, -1.6f, 10.35f);
	Vector3 currentHandPos = new Vector3();	Vector3 IboxRightTopFront = Vector3.zero;
    Vector3 circleScreenPos = Vector3.zero;

    private static JointOverlayerSpaceBlast instance;
    /// <summary>
    /// Gets the single JointOverlayerColorVsWords instance.
    /// </summary>
    /// <value>The JointOverlayerColorVsWords instance.</value>
    public static JointOverlayerSpaceBlast Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (JointOverlayerSpaceBlast)GameObject.FindObjectOfType(typeof(JointOverlayerSpaceBlast));
            }
            return instance;
        }
    }

    private bool handPositionInited;

    public bool HandPositionInited
    {
        get { return handPositionInited; }
        set { handPositionInited = value; }
    }

    void LateUpdate()
    {
        if (handPositionInited)
            UpdateTargetController();
    }

    void UpdateTargetController()
    {
        if (SpaceBlastGameController.Instance.spaceAimState == SpaceAimState.Moving)
        {
            targetController.UpdateTargetPos(handScreenPos);
        }
        else if (SpaceBlastGameController.Instance.spaceAimState == SpaceAimState.LockingMeteor)
        {
            targetController.UpdateTargetPos(handScreenPos, true);
            if (handScreenPos.z >= 0.95f)
                SpaceBlastGameController.Instance.LaunchMissile();
        }
        else if (SpaceBlastGameController.Instance.spaceAimState == SpaceAimState.LockingPowerUp)
        {
            targetController.UpdateTargetPos(handScreenPos, true);
            if (handScreenPos.z >= 0.95f)
                SpaceBlastGameController.Instance.OnStartPull();
        }

    }

    void OnHandMovement()
    {
        try
        {
            overlayObject.localPosition = CalculatePosition(handScreenPos, ref currentHandPos);
        }
        catch (System.Exception)
        {
            Debug.Log("Position not defined");
            throw;
        }

        try
        {
            FilledCircleInObjectMovement();
        }
        catch (System.Exception)
        {
            Debug.Log("Position not defined");
            throw;
        }

    }

    void FilledCircleInObjectMovement()
    {
        //if (circleScreenPos.y >= 0.5666f)
        //    circleScreenPos.y = 0.5666f;
        //else if (circleScreenPos.y <= 0.3087f)
        //    circleScreenPos.y = 0.3087f;

        Vector3 objectFilledSpritePosition = new Vector3(circleScreenPos.x * Screen.width - Screen.width / 2,
        circleScreenPos.y * Screen.height - Screen.height / 2, 0f);

        if (circleScreenPosInited)
            filledCircleSprite.transform.localPosition = objectFilledSpritePosition;
        else
            circleScreenPosInited = true;

    }

    public void UpdateScreenPos(Vector3 _handScreenPos, Vector3 _circleScreenPos)
    {
        handScreenPos = _handScreenPos;
        circleScreenPos = _circleScreenPos;
    }

    public void OnTargetLocked()
    {
        targetController.OnTargetLocked();
    }

    public void OnTargetRelese()
    {
        targetController.OnTargetRelese();
    }

    public void OnTargetFound()
    {
        targetController.OnTargetFound();
    }

    public void OnTargetMiss()
    {
        targetController.OnTargetMiss();
    }

    Vector3 CalculatePosition(Vector3 screenNormalPos, ref Vector3 currentPos)
	{
		currentPos.x = Mathf.Lerp(handMinLimitPos.x, handMaxLimitPos.x, screenNormalPos.x);
		currentPos.y = Mathf.Lerp(handMinLimitPos.y, handMaxLimitPos.y, screenNormalPos.y);
		currentPos.z = Mathf.Lerp(handMinLimitPos.z, handMaxLimitPos.z, screenNormalPos.z);
		
		return currentPos;
	}
}


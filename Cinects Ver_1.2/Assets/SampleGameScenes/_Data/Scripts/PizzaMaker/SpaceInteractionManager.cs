using UnityEngine;
//using Windows.Kinect;

using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.IO;

/// <summary>
/// Interaction manager is the component that deals with hand interactions.
/// </summary>
public class SpaceInteractionManager : MonoBehaviour 
{

	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;
	
	[Tooltip("Whether to use the GUI hand-cursor as on-screen cursor.")]
	public bool useHandCursor = true;

	[Tooltip("Hand-cursor texture for the hand-grip state.")]
	public Texture gripHandTexture;
	[Tooltip("Hand-cursor texture for the hand-release state.")]
	public Texture releaseHandTexture;
	[Tooltip("Hand-cursor texture for the non-tracked state.")]
	public Texture normalHandTexture;

    [Tooltip("Hand-cursor sprite parent reference.")]
    public UISprite handCursorSprite;

   [Tooltip("Smooth factor for cursor movement.")]
	public float smoothFactor = 3f;

    [Tooltip("Sensibility for cursor movement.")]
    public float sensibilityFactor = 1f;

    [Tooltip("Whether hand clicks (hand not moving for ~2 seconds) are enabled or not.")]
	public bool allowHandClicks = true;
	
	[Tooltip("Whether hand cursor and interactions control the mouse cursor or not.")]
	public bool controlMouseCursor = false;

	[Tooltip("Whether hand grips and releases control mouse dragging or not.")]
	public bool controlMouseDrag = false;

	[Tooltip("Dynamicly expands the hand reach")]
	public bool dynamic = true;

	// Bool to specify whether to convert Unity screen coordinates to full screen mouse coordinates
	//public bool convertMouseToFullScreen = false;
	
	[Tooltip("GUI-Text to display the interaction-manager debug messages.")]
	public GUIText debugText;

    private long primaryUserID = 0;

    private Camera mainCam;

	private bool isHandPress = false;
	private bool isSameColliderBox = false;

    private Vector3 cursorScreenPos = Vector3.zero;
	private bool dragInProgress = false;

    private MovingSpaceObject lastRaycastCollider;
    private MovingSpaceObject currentRaycastCollider;

    private HandEventType handEvent = HandEventType.None;
	private HandEventType lastHandEvent = HandEventType.None;

	private Vector3 handScreenPos = Vector3.zero;
	private Vector3 lastHandScreenPos = Vector3.zero;
	private Vector3 IboxLeftBotBack = Vector3.zero;
	private Vector3 IboxRightTopFront = Vector3.zero;
	private Vector3 dynamicIboxLeftBotBack = Vector3.zero;
	private Vector3 dynamicIboxRightTopFront = Vector3.zero;
	private Vector3 handSpineMidDistance = Vector3.zero;

	private bool isIboxValid = false;
    private bool cursorScreenPosInited;
    private bool objectFilledScreenPosInited;
    private bool isHandInteracting = false;
	private bool isHandClick = false;
	
	private float handInteractingSince = 0f;
	private float lastHandTime = 0f;
	private float handClickProgress = 0f;
    private float raycastDelay = 0f;
    private float identifyDelay = 0f;

    #region Constants

    private const float minTimeBetweenSameGestures = 0.0f;
	private const float clickMaxDistance = 0.05f;
	private const float poseCompleteDuration = 1.0f;
	private const float clickStayDuration = 1.0f;

	#endregion

	// Bool to keep track whether Kinect and Interaction library have been initialized
	private bool interactionInited = false;
	
	// The single instance of FacetrackingManager
	private static SpaceInteractionManager instance;

	
	/// <summary>
	/// Gets the single InteractionManager instance.
	/// </summary>
	/// <value>The InteractionManager instance.</value>
	public static SpaceInteractionManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = (SpaceInteractionManager)GameObject.FindObjectOfType(typeof(SpaceInteractionManager));
			}
			return instance;
		}
	}

    public float IdentifingProgress
    {
        get { return identifyDelay; }
    }

	public Vector3 HandScreenPos 
	{
		get {return this.handScreenPos;}
		set {handScreenPos = value;}
	}

	public bool IsHandInteracting 
	{
		get {return this.isHandInteracting;}
		set {isHandInteracting = value;}
	}

	/// <summary>
	/// Determines whether the InteractionManager was successfully initialized.
	/// </summary>
	/// <returns><c>true</c> if InteractionManager was successfully initialized; otherwise, <c>false</c>.</returns>
	public bool IsInteractionInited {
		get {
			return this.interactionInited;
		}
		set {
			interactionInited = value;
		}
	}
	/// <summary>
	/// Gets the current user ID, or 0 if no user is currently tracked.
	/// </summary>
	/// <returns>The user ID</returns>
	public long GetUserID()
	{
		return primaryUserID;
	}
	
	/// <summary>
	/// Gets the current left hand event (none, grip or release).
	/// </summary>
	/// <returns>The current left hand event.</returns>
	public HandEventType GetHandEvent()
	{
		return handEvent;
	}
	
	/// <summary>
	/// Gets the last detected hand event (grip or release).
	/// </summary>
	/// <returns>The last hand event.</returns>
	public HandEventType GetLastHandEvent()
	{
		return lastHandEvent;
	}
	
	/// <summary>
	/// Gets the current normalized viewport position of the hand, in range [0, 1].
	/// </summary>
	/// <returns>The hand viewport position.</returns>
	public Vector3 GetHandScreenPos()
	{
		return handScreenPos;
	}
	
	/// <summary>
	/// Determines whether the hand is pressing.
	/// </summary>
	/// <returns><c>true</c> if the hand is pressing; otherwise, <c>false</c>.</returns>
	public bool IsHandPress()
	{
		return isHandPress;
	}
	
	/// <summary>
	/// Determines whether a hand click is detected, false otherwise.
	/// </summary>
	/// <returns><c>true</c> if a hand click is detected; otherwise, <c>false</c>.</returns>
	public bool IsHandClickDetected()
	{
		if(isHandClick)
		{
			isHandClick = false;
			handClickProgress = 0f;
			lastHandScreenPos = Vector3.zero;
			lastHandTime = 0f;
			
			return true;
		}
		
		return false;
	}

	/// <summary>
	/// Gets the hand click progress, in range [0, 1].
	/// </summary>
	/// <returns>The hand click progress.</returns>
	public float GetHandClickProgress()
	{
		return handClickProgress;
	}

	//----------------------------------- end of public functions --------------------------------------//


	void Start() 
	{
		instance = this;
		interactionInited = true;
        mainCam = JointOverlayerSpaceBlast.Instance.targetController.cameraRef.GetComponent<Camera>();
        if (!isHandInteracting)
             isHandInteracting = true;
    }

    void OnDestroy()
	{
		if(interactionInited)
		{
			interactionInited = false;
			instance = null;
		}
	}
	
	/// <summary>
    /// 
    /// </summary>
    void LateUpdate ()
	{
		useHandCursor = (SpaceBlastGameController.Instance.spaceAimState != SpaceAimState.Nope);

        if (useHandCursor)
        {
            if (SpaceBlastGameController.Instance.spaceAimState != SpaceAimState.LockingMeteor ||
                SpaceBlastGameController.Instance.spaceAimState != SpaceAimState.LockingPowerUp)
            {
                if (IsSameColliderBox())
                {
                    identifyDelay += Time.deltaTime;

                    if (identifyDelay > 0.5f)
                    {
                        identifyDelay = 0f;
                        lastHandTime = 0f;
                        if (currentRaycastCollider.tag == "Meteor")
                        {
                            SpaceBlastGameController.Instance.OnMeteorTargetLocked(
                                currentRaycastCollider.GetComponent<Meteor>());
                        }
                        else if (currentRaycastCollider.tag == "PowerUp")
                        {
                            SpaceBlastGameController.Instance.OnPowerUpTargetLocked(
                                currentRaycastCollider.gameObject);
                        }
                    }
                }
                else
                {
                    identifyDelay = 0f;
                    isHandClick = false;
                    handClickProgress = 0f;
                    lastHandScreenPos = handScreenPos;
                    lastHandTime = 0f;
                }
            }
        }
        else
        {
            identifyDelay = 0f;
        }
	}

    Vector3 SetCursorPositionBySensibility(Vector3 currentCursorPos)
    {
        currentCursorPos *= sensibilityFactor;
        if (currentCursorPos.x > 1f)
            currentCursorPos.x = 1f;
        if (currentCursorPos.y > 1f)
            currentCursorPos.y = 1f;
        if (currentCursorPos.z > 1f)
            currentCursorPos.z = 1f;

        return currentCursorPos;
    }
	
    void CursorMovement()
    {
        if (useHandCursor)
        {
            if (isHandInteracting)
            {
                Vector3 cursorSpritePosition = new Vector3(cursorScreenPos.x * Screen.width - Screen.width / 2,
                    cursorScreenPos.y * Screen.height - Screen.height / 2, 0f);

                try
                {
                    if (cursorScreenPosInited)
                        handCursorSprite.transform.localPosition = cursorSpritePosition;
                    else
                        cursorScreenPosInited = true;
                }
                catch (Exception)
                {
                    cursorScreenPosInited = false;
                    throw;
                }

                if (controlMouseCursor)
                    MouseControl.MouseMove(cursorScreenPos, debugText);
            }
        }
    }

    public Vector3 Point3DToScreen2D(Vector3 aimPos)
    {
        float screenX = 0f;
        float screenY = 0f;

        // Translate input point using camera position
        float inputX = aimPos.x - mainCam.transform.position.x;
        float inputY = aimPos.y - mainCam.transform.position.y;
        float inputZ = aimPos.z - mainCam.transform.position.z;

        // Apply projection to X and Y
        screenX = (inputX / 2.15f) / (-inputZ * (float) Math.Tan((double)mainCam.fieldOfView / 2D));

        screenY = (inputY * 0.8f) / (-inputZ * (float)Math.Tan((double)mainCam.fieldOfView / 2D));

        // Convert to screen coordinates
        screenX = (float)Screen.width * (1f - ((screenX + 1.125f) / 2f));
        screenY = (float)Screen.height * (1f - ((screenY + 1f) / 2f));

        Vector3 screenPos = new Vector3(screenX, screenY, 0f);

        return screenPos;
    }

    public bool IsSameColliderBox()
    {
        Vector3 aimPos = JointOverlayerSpaceBlast.Instance.targetController.AimPos;

        // check if there is an underlying object to be selected
        if (aimPos != Vector3.zero)
        {
            // convert the normalized screen pos to pixel pos

            Ray ray = Camera.main.ScreenPointToRay(Point3DToScreen2D(aimPos));

            //Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
           
            // check if there is an underlying objects
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                bool onBoxColliderHit = false;
                if (hit.collider.gameObject.tag == "Meteor")
                {
                    onBoxColliderHit = true;
                    lastRaycastCollider = currentRaycastCollider;
                    currentRaycastCollider = hit.collider.transform.parent.GetComponent<MovingSpaceObject>();
                    JointOverlayerSpaceBlast.Instance.OnTargetFound();
                }
                else if (hit.collider.gameObject.tag == "PowerUp")
                {
                    onBoxColliderHit = true;
                    lastRaycastCollider = currentRaycastCollider;
                    currentRaycastCollider = hit.collider.gameObject.GetComponent<MovingSpaceObject>();
                    JointOverlayerSpaceBlast.Instance.OnTargetFound();
                }

                if (!onBoxColliderHit)
                {
                    JointOverlayerSpaceBlast.Instance.OnTargetMiss();
                    if (currentRaycastCollider != null)
                        currentRaycastCollider = null;
                }
            }
        }
        return (currentRaycastCollider != null && currentRaycastCollider == lastRaycastCollider);
    }
}
using UnityEngine;
//using Windows.Kinect;

using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.IO;

/// <summary>
/// The hand event types.
/// </summary>
public enum HandEventType : int
{
	None = 0,
	Grip = 1,
	Release = 2
}
/// <summary>
/// Interaction manager is the component that deals with hand interactions.
/// </summary>
public class PizzaInteractionManager : MonoBehaviour 
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
	
	private bool isHandPress = false;
	private bool isSameColliderBox = false;

    private Vector3 cursorScreenPos = Vector3.zero;
	private bool dragInProgress = false;

    private PizzaObjectCollider lastRaycastCollider;
    private PizzaObjectCollider currentRaycastCollider;

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
	private float handInteractingSince = 0f;
	
	private float lastHandTime = 0f;
	private bool isHandClick = false;
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
	private static PizzaInteractionManager instance;

	
	/// <summary>
	/// Gets the single InteractionManager instance.
	/// </summary>
	/// <value>The InteractionManager instance.</value>
	public static PizzaInteractionManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = (PizzaInteractionManager)GameObject.FindObjectOfType(typeof(PizzaInteractionManager));
			}
			return instance;
		}
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
	}
	
	void OnDestroy()
	{
		if(interactionInited)
		{
			interactionInited = false;
			instance = null;
		}
	}
	
	void LateUpdate () 
	{
		useHandCursor = (PizzaGameController.Instance.CurrentState == ItemState.Cursor);

        if (handCursorSprite.enabled != useHandCursor)
            handCursorSprite.enabled = useHandCursor;

        if (useHandCursor)
        {

            // check for press
            isHandPress = ((IboxRightTopFront.z - 0.1f) >= handScreenPos.z);

            // check for hand click
            float fClickDist = (handScreenPos - lastHandScreenPos).magnitude;

            if (allowHandClicks && !dragInProgress && isHandInteracting &&
               (fClickDist < KinectInterop.Constants.ClickMaxDistance))
            {
                lastHandTime += Time.deltaTime;
                if (lastHandTime >= KinectInterop.Constants.ClickStayDuration)
                {
                    if (!isHandClick)
                    {
                        isHandClick = true;
                        handClickProgress = 1f;

                        if (controlMouseCursor)
                        {
                            MouseControl.MouseClick();

                            isHandClick = false;
                            handClickProgress = 0f;
                            UIInteractionController.Instance.FilledCircleAmount(handClickProgress);
                            UIInteractionController.Instance.ClearFeedbackText();
                            lastHandScreenPos = Vector3.zero;
                            lastHandTime = 0f;
                        }
                    }
                }
                else
                {
                    handClickProgress = lastHandTime / KinectInterop.Constants.ClickStayDuration;
                    raycastDelay += Time.deltaTime;

                    if (raycastDelay >= 0.1f)
                        raycastDelay = 0f;
                    
                    if (IsSameColliderBox())
                    {
                        identifyDelay += Time.deltaTime;

                        if (identifyDelay > 0.5f)
                        {
                            float localProgress = (handClickProgress - 0.5f) * 2.1f;
                            UIInteractionController.Instance.FilledCircleAmount(localProgress);
                        }
                    }
                }
            }
            else
            {
                isHandClick = false;
                handClickProgress = 0f;
                UIInteractionController.Instance.FilledCircleAmount(handClickProgress);
                UIInteractionController.Instance.ClearFeedbackText();
                lastHandScreenPos = handScreenPos;
                lastHandTime = 0f;
            }

            if (PlayerData.Instance.IsUserDetected)
            {
                HandEventType handEvent = HandEventType.None;

                // check if the hand is interacting

                if (!isHandInteracting)
                    isHandInteracting = true;

                //handScreenPos = SetCursorPositionBySensibility(handScreenPos);

                //if (!PlayerData.Instance.IsIboxValid)
                //{
                //    isHandInteracting = false;
                //    isHandPress = false;
                //}
                //else
                //{
                //    if (!isHandInteracting)
                //        isHandInteracting = true;

                //    handScreenPos = SetCursorPositionBySensibility(handScreenPos);
                //}

                // process hand
                handEvent = PizzaGameController.Instance.Handstate;

                if (controlMouseCursor && (handEvent != lastHandEvent))
                {
                    if (controlMouseDrag && !dragInProgress && (handEvent == HandEventType.Grip))
                    {
                        dragInProgress = true;
                        MouseControl.MouseDrag();
                    }
                    else if (dragInProgress && (handEvent == HandEventType.Release))
                    {
                        MouseControl.MouseRelease();
                        dragInProgress = false;
                    }
                }

                if (handEvent != HandEventType.None)
                {
                    lastHandEvent = handEvent;
                }

                // if the hand is primary, set the cursor position
                if (isHandInteracting)
                {
                    if (handClickProgress < 0.8f)
                    {
                        cursorScreenPos = handScreenPos;
                        if (cursorScreenPosInited)
                            CursorMovement();
                        else
                            cursorScreenPosInited = true;
                    }

                    if (controlMouseCursor && !useHandCursor)
                    {
                        MouseControl.MouseMove(cursorScreenPos, debugText);
                    }
                }
            }
            else
            {
                PizzaGameController.Instance.Handstate = HandEventType.None;
                handEvent = PizzaGameController.Instance.Handstate;
                isHandPress = false;
                if (lastHandEvent == HandEventType.Grip)
                    lastHandEvent = HandEventType.Release;
                else
                    lastHandEvent = HandEventType.None;

                if (controlMouseCursor && dragInProgress)
                {
                    MouseControl.MouseRelease();
                    dragInProgress = false;
                }
            }
        }
        else
        {
            identifyDelay = 0f;
            UIInteractionController.Instance.FilledCircleAmount(0f);
            if (currentRaycastCollider.Collider_ID == Collider_ID.Knive && currentRaycastCollider.BorderEnabled)
                currentRaycastCollider.SetBorderCondition(false);
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

    public bool IsSameColliderBox()
    {
        Vector3 screenNormalPos = Vector3.zero;
        Vector3 screenPixelPos = Vector3.zero;
        screenNormalPos = handScreenPos;

        // check if there is an underlying object to be selected
        if (screenNormalPos != Vector3.zero)
        {
            // convert the normalized screen pos to pixel pos
            screenPixelPos.x = (int)(screenNormalPos.x * Camera.main.pixelWidth);
            screenPixelPos.y = (int)(screenNormalPos.y * Camera.main.pixelHeight);
            Ray ray = Camera.main.ScreenPointToRay(screenPixelPos);

            // check if there is an underlying objects
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                bool onBoxColliderHit = false;
                if (hit.collider.gameObject.tag == "KniveCollider" ||
                    hit.collider.gameObject.tag == "IngredientType" ||
                    hit.collider.gameObject.tag == "IngredientChoice")
                    onBoxColliderHit = true;
                else if (hit.collider.gameObject.tag == "CuttingTableCollider")
                {
                    if (hit.collider.gameObject.GetComponent<PizzaCuttingTableCollider>().CurrentState != CuttingTableState.OnFood_01)
                        onBoxColliderHit = true;
                }
                else if (hit.collider.gameObject.tag == "RawPizzaCollider")
                {
                    if (hit.collider.gameObject.GetComponent<RawPizzaCollider>().CurrentState != RawPizzaState.Empty &&
                        PizzaGameController.Instance.OvenIsEmpty)
                        onBoxColliderHit = true;
                }
                else if (hit.collider.gameObject.tag == "OvenCollider")
                {
                    if (hit.collider.gameObject.GetComponent<OvenCollider>().CurrentState != OvenState.Empty)
                        onBoxColliderHit = true;
                }
               
                if (onBoxColliderHit)
                {
                    lastRaycastCollider = currentRaycastCollider;
                    currentRaycastCollider = hit.collider.gameObject.GetComponent<PizzaObjectCollider>();
                    currentRaycastCollider.CurrentInteractionSource = InteractionSource.FromRayCast;
                    if (!currentRaycastCollider.BorderEnabled)
                        currentRaycastCollider.SetBorderCondition(true);
                }
                else
                {
                    if (currentRaycastCollider != null && currentRaycastCollider.BorderEnabled)
                        currentRaycastCollider.SetBorderCondition(false);

                    currentRaycastCollider = null;
                }
            }
        }
        return (currentRaycastCollider != null && currentRaycastCollider == lastRaycastCollider);
    }
}
using UnityEngine;
using System.Collections;



/// <summary>
/// Interaction manager is the component that deals with hand interactions.
/// </summary>
public class BowAndArrowInteractionManager : MonoBehaviour
{
    /// <summary>
    /// The hand event types.
    /// </summary>
    public enum HandEventType : int
    {
        None = 0,
        Grip = 1,
        Release = 2
    }

    [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
    public int playerIndex = 0;

    [Tooltip("Smooth factor for cursor movement.")]
    public float smoothFactor = 3f;

    // Bool to specify whether to convert Unity screen coordinates to full screen mouse coordinates
    //public bool convertMouseToFullScreen = false;

    [Tooltip("GUI-Text to display the interaction-manager debug messages.")]
    public GUIText debugText;

    private long primaryUserID = 0;

    private bool isLeftHandPrimary = false;
    private bool isRightHandPrimary = false;

    private KinectInterop.HandState leftHandState = KinectInterop.HandState.Unknown;
    private KinectInterop.HandState rightHandState = KinectInterop.HandState.Unknown;

    private HandEventType leftHandEvent = HandEventType.None;
    private HandEventType lastLeftHandEvent = HandEventType.Release;

    private Vector3 leftHandPos = Vector3.zero;
    private Vector3 leftHandScreenPos = Vector3.zero;
    private Vector3 leftIboxLeftBotBack = Vector3.zero;
    private Vector3 leftIboxRightTopFront = Vector3.zero;
    private bool isleftIboxValid = false;
    private bool isLeftHandInteracting = false;
    private float leftHandInteractingSince = 0f;

    private Vector3 lastLeftHandPos = Vector3.zero;
    private float lastLeftHandTime = 0f;

    private HandEventType rightHandEvent = HandEventType.None;
    private HandEventType lastRightHandEvent = HandEventType.Release;

    private Vector3 rightHandPos = Vector3.zero;
    private Vector3 rightHandScreenPos = Vector3.zero;
    private Vector3 rightIboxLeftBotBack = Vector3.zero;
    private Vector3 rightIboxRightTopFront = Vector3.zero;
    private bool isRightIboxValid = false;
    private bool isRightHandInteracting = false;
    private float rightHandInteractingSince = 0f;

    private Vector3 lastRightHandPos = Vector3.zero;
    private float lastRightHandTime = 0f;

    // Bool to keep track whether Kinect and Interaction library have been initialized
    private bool interactionInited = false;

    // The single instance of FacetrackingManager
    private static BowAndArrowInteractionManager instance;


    /// <summary>
    /// Gets the single InteractionManager instance.
    /// </summary>
    /// <value>The InteractionManager instance.</value>
    public static BowAndArrowInteractionManager Instance
    {
        get
        {
            return instance;
        }
    }

    public bool IsLeftHandInteracting()
    {
        return isLeftHandInteracting;
    }

    public bool IsRightHandInteracting()
    {
        return isRightHandInteracting;
    }

    /// <summary>
    /// Determines whether the InteractionManager was successfully initialized.
    /// </summary>
    /// <returns><c>true</c> if InteractionManager was successfully initialized; otherwise, <c>false</c>.</returns>
    public bool IsInteractionInited()
    {
        return interactionInited;
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
    public HandEventType GetLeftHandEvent()
    {
        return leftHandEvent;
    }

    /// <summary>
    /// Gets the last detected left hand event (grip or release).
    /// </summary>
    /// <returns>The last left hand event.</returns>
    public HandEventType GetLastLeftHandEvent()
    {
        return lastLeftHandEvent;
    }

    /// <summary>
    /// Gets the current normalized viewport position of the left hand, in range [0, 1].
    /// </summary>
    /// <returns>The left hand viewport position.</returns>
    public Vector3 GetLeftHandScreenPos()
    {
        return leftHandScreenPos;
    }

    /// <summary>
    /// Determines whether the left hand is primary for the user.
    /// </summary>
    /// <returns><c>true</c> if the left hand is primary for the user; otherwise, <c>false</c>.</returns>
    public bool IsLeftHandPrimary()
    {
        return isLeftHandPrimary;
    }

    /// <summary>
    /// Gets the current right hand event (none, grip or release).
    /// </summary>
    /// <returns>The current right hand event.</returns>
    public HandEventType GetRightHandEvent()
    {
        return rightHandEvent;
    }

    /// <summary>
    /// Gets the last detected right hand event (grip or release).
    /// </summary>
    /// <returns>The last right hand event.</returns>
    public HandEventType GetLastRightHandEvent()
    {
        return lastRightHandEvent;
    }

    /// <summary>
    /// Gets the current normalized viewport position of the right hand, in range [0, 1].
    /// </summary>
    /// <returns>The right hand viewport position.</returns>
    public Vector3 GetRightHandScreenPos()
    {
        return rightHandScreenPos;
    }

    /// <summary>
    /// Determines whether the right hand is primary for the user.
    /// </summary>
    /// <returns><c>true</c> if the right hand is primary for the user; otherwise, <c>false</c>.</returns>
    public bool IsRightHandPrimary()
    {
        return isRightHandPrimary;
    }

    /// <summary>
    /// Determines whether a right hand click is detected, false otherwise.
    /// </summary>
    /// <returns><c>true</c> if a right hand click is detected; otherwise, <c>false</c>.</returns>

    //----------------------------------- end of public functions --------------------------------------//


    void Start()
    {
        instance = this;
        interactionInited = true;
    }

    void OnDestroy()
    {
        // uninitialize Kinect interaction
        if (interactionInited)
        {
            interactionInited = false;
            instance = null;
        }
    }

    void Update()
    {
        KinectManager kinectManager = KinectManager.Instance;

        // update Kinect interaction
        if (kinectManager && kinectManager.IsInitialized())
        {
            primaryUserID = kinectManager.GetUserIdByIndex(playerIndex);

            if (primaryUserID != 0)
            {
                HandEventType handEvent = HandEventType.None;

                // get the left hand state
                leftHandState = kinectManager.GetLeftHandState(primaryUserID);

                // check if the left hand is interacting
                isleftIboxValid = kinectManager.GetLeftHandInteractionBox(primaryUserID, ref leftIboxLeftBotBack, ref leftIboxRightTopFront, isleftIboxValid);
                //bool bLeftHandPrimaryNow = false;

                if (isleftIboxValid && //bLeftHandPrimaryNow &&
                   kinectManager.GetJointTrackingState(primaryUserID, (int)KinectInterop.JointType.HandLeft) != KinectInterop.TrackingState.NotTracked)
                {
                    leftHandPos = kinectManager.GetJointPosition(primaryUserID, (int)KinectInterop.JointType.HandLeft);

                    leftHandScreenPos.x = Mathf.Clamp01((leftHandPos.x - leftIboxLeftBotBack.x) / (leftIboxRightTopFront.x - leftIboxLeftBotBack.x));
                    leftHandScreenPos.y = Mathf.Clamp01((leftHandPos.y - leftIboxLeftBotBack.y) / (leftIboxRightTopFront.y - leftIboxLeftBotBack.y));
                    leftHandScreenPos.z = Mathf.Clamp01((leftIboxLeftBotBack.z - leftHandPos.z) / (leftIboxLeftBotBack.z - leftIboxRightTopFront.z));

                    //bool wasLeftHandInteracting = isLeftHandInteracting;

                    isLeftHandInteracting = (leftHandPos.x >= (leftIboxLeftBotBack.x - 1.0f)) && (leftHandPos.x <= (leftIboxRightTopFront.x + 0.5f)) &&
                        (leftHandPos.y >= (leftIboxLeftBotBack.y - 0.1f)) && (leftHandPos.y <= (leftIboxRightTopFront.y + 0.7f)) &&
                        (leftIboxLeftBotBack.z >= leftHandPos.z) && (leftIboxRightTopFront.z * 0.8f <= leftHandPos.z);
                    //bLeftHandPrimaryNow = isLeftHandInteracting;

                    /*
                    if (!wasLeftHandInteracting && isLeftHandInteracting)
                    {
                        leftHandInteractingSince = Time.realtimeSinceStartup;
                    }*/

                    
                    /*
                    if (allowHandClicks && !dragInProgress && isLeftHandInteracting &&
                       (fClickDist < KinectInterop.Constants.ClickMaxDistance))
                    {
                        if ((Time.realtimeSinceStartup - lastLeftHandTime) >= KinectInterop.Constants.ClickStayDuration)
                        {
                            if (!isLeftHandClick)
                            {
                                isLeftHandClick = true;
                                leftHandClickProgress = 1f;

                                if (controlMouseCursor)
                                {
                                    MouseControl.MouseClick();

                                    isLeftHandClick = false;
                                    leftHandClickProgress = 0f;
                                    lastLeftHandPos = Vector3.zero;
                                    lastLeftHandTime = Time.realtimeSinceStartup;
                                }
                            }
                        }
                        else
                        {
                            leftHandClickProgress = (Time.realtimeSinceStartup - lastLeftHandTime) / KinectInterop.Constants.ClickStayDuration;
                        }
                    }
                    else
                    {
                        isLeftHandClick = false;
                        leftHandClickProgress = 0f;
                        lastLeftHandPos = leftHandPos;
                        lastLeftHandTime = Time.realtimeSinceStartup;
                    }*/
                }
                else
                {
                    isLeftHandInteracting = false;
                }

                // get the right hand state
                rightHandState = kinectManager.GetRightHandState(primaryUserID);

                // check if the right hand is interacting
                isRightIboxValid = kinectManager.GetRightHandInteractionBox(primaryUserID, ref rightIboxLeftBotBack, ref rightIboxRightTopFront, isRightIboxValid);
                //bool bRightHandPrimaryNow = false;

                if (isRightIboxValid && //bRightHandPrimaryNow &&
                   kinectManager.GetJointTrackingState(primaryUserID, (int)KinectInterop.JointType.HandRight) != KinectInterop.TrackingState.NotTracked)
                {
                    rightHandPos = kinectManager.GetJointPosition(primaryUserID, (int)KinectInterop.JointType.HandRight);

                    rightHandScreenPos.x = Mathf.Clamp01((rightHandPos.x - rightIboxLeftBotBack.x) / (rightIboxRightTopFront.x - rightIboxLeftBotBack.x));
                    rightHandScreenPos.y = Mathf.Clamp01((rightHandPos.y - rightIboxLeftBotBack.y) / (rightIboxRightTopFront.y - rightIboxLeftBotBack.y));
                    rightHandScreenPos.z = Mathf.Clamp01((rightIboxLeftBotBack.z - rightHandPos.z) / (rightIboxLeftBotBack.z - rightIboxRightTopFront.z));

                    //bool wasRightHandInteracting = isRightHandInteracting;
                    isRightHandInteracting = (rightHandPos.x >= (rightIboxLeftBotBack.x - 0.5f)) && (rightHandPos.x <= (rightIboxRightTopFront.x + 1.0f)) &&
                        (rightHandPos.y >= (rightIboxLeftBotBack.y - 0.1f)) && (rightHandPos.y <= (rightIboxRightTopFront.y + 0.7f)) &&
                        (rightIboxLeftBotBack.z >= rightHandPos.z) && (rightIboxRightTopFront.z * 0.8f <= rightHandPos.z);
                    //bRightHandPrimaryNow = isRightHandInteracting;
                    /*
                    if (!wasRightHandInteracting && isRightHandInteracting)
                    {
                        rightHandInteractingSince = Time.realtimeSinceStartup;
                    }*/


                    
                    /*
                    if (isLeftHandInteracting && isRightHandInteracting)
                    {
                        if (rightHandInteractingSince <= leftHandInteractingSince)
                            isLeftHandInteracting = false;
                        else
                            isRightHandInteracting = false;
                    }
                    */
                   
                    // check for right hand click
                    // float fClickDist = (rightHandPos - lastRightHandPos).magnitude;
                }
                else
                {
                    isRightHandInteracting = false;
                }

                // process left hand
                handEvent = HandStateToEvent(leftHandState, lastLeftHandEvent);

                if ((isLeftHandInteracting != isLeftHandPrimary) || (isRightHandInteracting != isRightHandPrimary))
                {
                    lastLeftHandEvent = HandEventType.Release;
                    lastRightHandEvent = HandEventType.Release;
                }

                if ((isLeftHandInteracting != isLeftHandPrimary))
                {
                    lastLeftHandEvent = HandEventType.Release;
                    lastRightHandEvent = HandEventType.Release;
                }

                leftHandEvent = handEvent;
                if (handEvent != HandEventType.None)
                {
                    lastLeftHandEvent = handEvent;
                }


                isLeftHandPrimary = isLeftHandInteracting;
                
                // process right hand
                handEvent = HandStateToEvent(rightHandState, lastRightHandEvent);

                rightHandEvent = handEvent;
                if (handEvent != HandEventType.None)
                {
                    lastRightHandEvent = handEvent;
                }

                isRightHandPrimary = isRightHandInteracting;
               
            }
            else
            {
                leftHandState = KinectInterop.HandState.NotTracked;
                rightHandState = KinectInterop.HandState.NotTracked;

                isLeftHandPrimary = false;
                isRightHandPrimary = false;

                leftHandEvent = HandEventType.None;
                rightHandEvent = HandEventType.None;

                lastLeftHandEvent = HandEventType.Release;
                lastRightHandEvent = HandEventType.Release;
            }
        }

    }


    // converts hand state to hand event type
    private HandEventType HandStateToEvent(KinectInterop.HandState handState, HandEventType lastEventType)
    {
        switch (handState)
        {
            case KinectInterop.HandState.Open:
                return HandEventType.Release;

            case KinectInterop.HandState.Closed:
            case KinectInterop.HandState.Lasso:
                return HandEventType.Grip;

            case KinectInterop.HandState.Unknown:
                return lastEventType;
        }

        return HandEventType.None;
    }


    void OnGUI()
    {
        if (!interactionInited)
            return;

        // display debug information
        if (debugText)
        {
            string sGuiText = string.Empty;

            //if(isLeftHandPrimary)
            {
                sGuiText += "LCursor: " + leftHandScreenPos.ToString();

                if (lastLeftHandEvent == HandEventType.Grip)
                {
                    sGuiText += "  LeftGrip";
                }
                else if (lastLeftHandEvent == HandEventType.Release)
                {
                    sGuiText += "  LeftRelease";
                }
                else
                {
                    sGuiText += lastLeftHandEvent.ToString();
                }
            }

            //if(isRightHandPrimary)
            {
                sGuiText += "\nRCursor: " + rightHandScreenPos.ToString();

                if (lastRightHandEvent == HandEventType.Grip)
                {
                    sGuiText += "  RightGrip";
                }
                else if (lastRightHandEvent == HandEventType.Release)
                {
                    sGuiText += "  RightRelease";
                }
                else
                {
                    sGuiText += lastRightHandEvent.ToString();
                }
            }

            debugText.GetComponent<GUIText>().text = sGuiText;
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerData : MonoBehaviour
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;
	
	[Tooltip("Kinect joint that is going to be overlayed.")]
	public KinectInterop.JointType trackedJoint = KinectInterop.JointType.HandRight;

	public bool dynamicBox = true;
    public int levelIndex = 1;

    bool isIboxValid;
    bool usingRightHand;
	bool isUserDetected;
	bool handPositionInited;
    bool onSceneChange = false;
    Vector3 handPos = Vector3.zero;
	Vector3 handScreenPos = Vector3.zero;
	Vector3 spineMidPos = Vector3.zero;
	Vector3 handSpineMidDistance = Vector3.zero;
	Vector3 dynamicIboxLeftBotBack = Vector3.zero;
	Vector3 dynamicIboxRightTopFront = Vector3.zero;
	Vector3 IboxLeftBotBack = Vector3.zero;
	Vector3 IboxRightTopFront = Vector3.zero;

    public bool IsUserDetected {get {return this.isUserDetected;}}

	public bool IsIboxValid 
	{
		get {return this.isIboxValid;}
	}

	public bool DynamicBox 
	{
		get {return this.dynamicBox;}
		set {dynamicBox = value;}
	}
	public Vector3 HandScreenPos {get {return this.handScreenPos;}}

	private static PlayerData instance;
	
	public static PlayerData Instance
	{ 
		get
		{
			if (instance == null)
			{
				instance = (PlayerData)GameObject.FindObjectOfType(typeof(PlayerData));
			}
			return instance;
		}
	}

	void Update()
	{
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Application.loadedLevel > 2)
                Application.LoadLevelAsync(2);
            else if (Application.loadedLevel == 2)
                Application.Quit();
        }

        if (!onSceneChange)
        {
            KinectManager manager = KinectManager.Instance;
			
		    if (manager && manager.IsInitialized ()) 
		    {				
			    int iJointIndex = (int)trackedJoint;

			    isUserDetected = manager.IsUserDetected ();

                if (isUserDetected)
                {
                    long userId = manager.GetUserIdByIndex(playerIndex);

                    if (manager.IsJointTracked(userId, iJointIndex))
                    {

                        if (trackedJoint == KinectInterop.JointType.Head && Application.loadedLevel == 1)
                        {
                            Quaternion headOrientation = manager.GetJointOrientation(userId, (int)trackedJoint, true);
                            JointOverlayerCalibration.Instance.UpdateHeadAngle(headOrientation);
                        }
                        else
                        {
                            if (!handPositionInited)
                            {
                                if (manager.GetJointTrackingState(userId, iJointIndex) != KinectInterop.TrackingState.NotTracked &&
                                    manager.GetJointTrackingState(userId, (int)KinectInterop.JointType.SpineMid) != KinectInterop.TrackingState.NotTracked)
                                {
                                    handPositionInited = true;

                                    //Current scenes indexes are: 
                                    // 0 = Splash
                                    // 1 = Calibration
                                    // 2 = Menu
                                    // 3 = PizzaMaker
                                    // 4 = SoccerColorVsWords
                                    // 5 = SpaceBlast
                                    // 6 = BowAndArrow

                                    if (Application.loadedLevel != 0)
                                    {
                                        GameSaveLoad.Instance.LoadPlayerRange(ref IboxLeftBotBack, ref IboxRightTopFront, ref usingRightHand);

                                        if (usingRightHand)
                                            trackedJoint = KinectInterop.JointType.HandRight;
                                        else
                                            trackedJoint = KinectInterop.JointType.HandLeft;
                                    }

                                    if (Application.loadedLevel == 1)
                                    {
                                        JointOverlayerCalibration.Instance.HandPositionInited = handPositionInited;
                                    }
                                    else if (Application.loadedLevel == 2)
                                    {
                                        JointOverlayerMenu.Instance.HandPositionInited = handPositionInited;
                                    }
                                    else if (Application.loadedLevel == 3)
                                    {
                                        PizzaInteractionManager.Instance.IsInteractionInited = handPositionInited;
                                        PizzaInteractionManager.Instance.IsHandInteracting = handPositionInited;
                                    }
                                    else if (Application.loadedLevel == 4)
                                    {
                                        JointOverlayerColorVsWords.Instance.HandPositionInited = handPositionInited;
                                    }
                                    else if (Application.loadedLevel == 5)
                                    {
                                        JointOverlayerSpaceBlast.Instance.HandPositionInited = handPositionInited;
                                    }

                                    handPos = manager.GetJointPosition(userId, (int)trackedJoint);
                                    spineMidPos = manager.GetJointPosition(userId, (int)KinectInterop.JointType.SpineMid);

                                    handSpineMidDistance = handPos - spineMidPos;
                                    dynamicIboxLeftBotBack = Vector3.zero;
                                    dynamicIboxRightTopFront = Vector3.zero;
                                    handScreenPos = Vector3.zero;
                                }
                                return;
                            }

                            if (dynamicBox)
                            {
                                isIboxValid = manager.GetHandDynamicInteractionBox(userId, trackedJoint, ref IboxLeftBotBack, ref IboxRightTopFront, ref dynamicIboxLeftBotBack, ref dynamicIboxRightTopFront, handSpineMidDistance, isIboxValid, ref dynamicBox);
                                if (!dynamicBox)
                                {
                                    StartCoroutine(OnLoadScene());
                                    return;
                                }
                            }

                            bool tryOnCalibration = true;
                            if (Application.loadedLevel == 0)
                                tryOnCalibration = JointOverlayerCalibration.Instance.UseFixedIBox;

                            if ((isIboxValid || tryOnCalibration) && manager.GetJointTrackingState(userId, (int)trackedJoint) != KinectInterop.TrackingState.NotTracked)
                            {
                                handPos = manager.GetJointPosition(userId, (int)trackedJoint);

                                Vector3 handScreenPos = new Vector3();
                                Vector3 cursorScreenPos = new Vector3();

                                handScreenPos.x = Mathf.Clamp01((handPos.x - IboxLeftBotBack.x) / (IboxRightTopFront.x - IboxLeftBotBack.x));
                                handScreenPos.y = Mathf.Clamp01((handPos.y - IboxLeftBotBack.y) / (IboxRightTopFront.y - IboxLeftBotBack.y));
                                handScreenPos.z = Mathf.Clamp01((handPos.z - IboxLeftBotBack.z) / (IboxRightTopFront.z - IboxLeftBotBack.z));

                                cursorScreenPos.x = Mathf.Clamp01((handPos.x - IboxLeftBotBack.x) / (IboxRightTopFront.x - IboxLeftBotBack.x));
                                cursorScreenPos.y = Mathf.Clamp01((handPos.y - IboxLeftBotBack.y) / (IboxRightTopFront.y - IboxLeftBotBack.y));
                                cursorScreenPos.z = Mathf.Clamp01((IboxLeftBotBack.z - handPos.z) / (IboxLeftBotBack.z - IboxRightTopFront.z));

                                //Current scenes indexes are: 
                                // 0 = Splash
                                // 1 = Calibration
                                // 2 = Menu
                                // 3 = PizzaMaker
                                // 4 = SoccerColorVsWords
                                // 5 = SpaceBlast
                                // 6 = BowAndArrow

                                if (Application.loadedLevel == 1)
                                {
                                    JointOverlayerCalibration.Instance.UpdateScreenPos(handScreenPos, cursorScreenPos);
                                }
                                else if (Application.loadedLevel == 2)
                                {
                                    JointOverlayerMenu.Instance.UpdateScreenPos(handScreenPos, cursorScreenPos);
                                }
                                else if (Application.loadedLevel == 3)
                                {
                                    JointOverlayerPizzaMaker.Instance.UpdateScreenPos(handScreenPos, cursorScreenPos);
                                    PizzaInteractionManager.Instance.HandScreenPos = cursorScreenPos;
                                }
                                else if (Application.loadedLevel == 4)
                                {
                                    JointOverlayerColorVsWords.Instance.UpdateScreenPos(handScreenPos, cursorScreenPos);
                                }
                                else if (Application.loadedLevel == 5)
                                {
                                    JointOverlayerSpaceBlast.Instance.UpdateScreenPos(handScreenPos, cursorScreenPos);
                                }
                            }

                        }
                    }
                }
			}
		}
	}
    IEnumerator OnLoadScene()
    {
        bool usingRightHand = (trackedJoint == KinectInterop.JointType.HandRight);
        GameSaveLoad.Instance.SavePlayerRange(IboxLeftBotBack, IboxRightTopFront, usingRightHand);
        yield return new WaitForEndOfFrame();
        JointOverlayerCalibration.Instance.EnableCineticPopUp();
    }
}
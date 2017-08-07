using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MenuState
{
    Nope = 0,
    Loading,
    Login,
    MenuComon,
    MenuPointer,
    Instructions,
    ChousingGame
}

public class JointOverlayerMenu : MonoBehaviour
{
    [Tooltip("Text for debugging")]
    public UILabel debugText;

    [Tooltip("Filled circle sprite reference.")]
    public UISprite cursor;

    [Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
    public int playerIndex = 0;

    public InterfaceController interfaceController;

    GUIText decoyText;
    bool isIboxValid;
    bool userDetected;
    bool circleScreenPosInited;
    Vector3 handPos = Vector3.zero;
    Vector3 spineMidPos = Vector3.zero;
    Vector3 dynamicIboxLeftBotBack = Vector3.zero;
    Vector3 dynamicIboxRightTopFront = Vector3.zero;
    Vector3 handSpineMidDistance = Vector3.zero;
    Vector3 IboxLeftBotBack = Vector3.zero;
    Vector3 handScreenPos = Vector3.zero;
    Vector3 cursorScreenPos = Vector3.zero;
    Vector3 currentHandPos = new Vector3();
    Vector3 IboxRightTopFront = Vector3.zero;
    Vector3 referenceInitialPos = new Vector3();

    KinectInterop.JointType nextJoint;

    MenuState currentState;
    MenuState lastState;

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


    private static JointOverlayerMenu instance;
    /// <summary>
    /// Gets the single JointOverlayerCalibration instance.
    /// </summary>
    /// <value>The JointOverlayerCalibration instance.</value>
    public static JointOverlayerMenu Instance
	{ 
		get
		{
			if (instance == null)
			{
				instance = (JointOverlayerMenu)GameObject.FindObjectOfType(typeof(JointOverlayerMenu));
			}
			return instance;
		}
	}

    void Start()
    {
        currentState = MenuState.MenuComon;
    }

    void LateUpdate()
    {
        OnChangeStateTrigger();
        if (!userDetected)
        {
            if (PlayerData.Instance.IsUserDetected)
            {
                userDetected = PlayerData.Instance.IsUserDetected;
            }
        }
        if (userDetected)
        {
            CursorMovement();
        }
    }

    void CursorMovement()
    {
        //if (circleScreenPos.y >= 0.5666f)
        //    circleScreenPos.y = 0.5666f;
        //else if (circleScreenPos.y <= 0.3087f)
        //    circleScreenPos.y = 0.3087f;

        Debug.Log(cursorScreenPos);

        Vector3 objectFilledSpritePosition = new Vector3(cursorScreenPos.x * Screen.width - Screen.width / 2,
        cursorScreenPos.y * Screen.height - Screen.height / 2, 0f);

        if (circleScreenPosInited)
        {
            cursor.transform.localPosition = objectFilledSpritePosition;
            MouseControl.MouseMove(cursorScreenPos, decoyText);
            
        }
        else
            circleScreenPosInited = true;

    }

    void OnChangeStateTrigger()
    {
        if (currentState != lastState)
        {
            switch (currentState)
            {
                case MenuState.Nope:
                    break;
                 case MenuState.Loading:
                    interfaceController.SetInterfaceState(InterfaceState.Loading);
                    break;
                case MenuState.Login:
                    interfaceController.SetInterfaceState(InterfaceState.Login);
                    break;
                case MenuState.MenuComon:
                    interfaceController.SetInterfaceState(InterfaceState.MenuComon);
                    break;
                case MenuState.MenuPointer:
                    interfaceController.SetInterfaceState(InterfaceState.MenuPointer);
                    break;
                case MenuState.Instructions:
                    interfaceController.SetInterfaceState(InterfaceState.Instruction);
                    break;
                default:
                    break;
            }
            lastState = currentState;
        }
    }

    public void UpdateScreenPos(Vector3 _handScreenPos, Vector3 _cursorScreenPos)
    {
        handScreenPos = _handScreenPos;
        cursorScreenPos = _cursorScreenPos;
    }

    public void SetNextState(MenuState nextState)
    {
        currentState = nextState;
    }

    public void EnableCineticPopUp()
    {
        useFixedIBox = true;
        circleScreenPosInited = true;
        interfaceController.EnablePopUp(PopUps.Cinetic);
    }

    public void SetLextLevel(int nextLevel)
    {
        interfaceController.OnChooseLevelPress(nextLevel);
    }
}
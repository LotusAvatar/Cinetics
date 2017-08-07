using UnityEngine;
using System.Collections;

public enum InterfaceState
{
    Clear,
    Login,
    Loading,
    MenuComon,
    MenuPointer,
    Instruction
}

public enum PopUps
{
    Range,
    HeadMovement,
    HandMovement,
    Cinetic
}

public class InterfaceController : MonoBehaviour
{
    public GameObject background;
    public GameObject loading;
    public GameObject login;
    public GameObject mainMenu;
    public GameObject menuComon;
    public GameObject pointerMenu;
    public GameObject instructions;
    public GameObject popUp;
    public GameObject cursor;

    public GameObject rangeWarning;
    public GameObject headMovementWarning;
    public GameObject handMovementWarning;
    public GameObject cineticWarning;

    public UISprite popUpProgress;
    public UISprite loadingpProgress;

    bool loadGame = false;
    bool popUpEnabled = false;
    int nextLevel;
    float loadingProgress;
    float loadingDelay = 4f;
    float popUpCounter;
    float popUpDelay = 5f;
    string doctorsName;
    string pacientsName;
    PopUps currentPopUp;
    InterfaceState myState = InterfaceState.Clear;
    InterfaceState lastState = InterfaceState.Clear;
    private static InterfaceController instance;

    public InterfaceController Instance
    { get { return instance; } }

	void Update ()
    {
        StateChecker();

        if (popUpEnabled)
            PopUpCountdown();
    }

    void StateChecker()
    {
        if (myState != lastState)
        {
            switch (myState)
            {
                case InterfaceState.Clear: OnClear(); break;
                case InterfaceState.Login: LoginTrigger(); break;
                case InterfaceState.Loading: LoadingTrigger(); break;
                case InterfaceState.MenuComon: MenuTrigger(); break;
                case InterfaceState.MenuPointer: MenuPointerTrigger(); break;
                case InterfaceState.Instruction: InstructionTrigger(); break;
                default: break;
            }
        }
        lastState = myState;
        switch (myState)
        {
            case InterfaceState.Loading: LoadingProgress(); break;
            default: break;
        }
    }

    void OnClear()
    {
        background.SetActive(false);
        mainMenu.SetActive(false);
        loading.SetActive(false);
        login.SetActive(false);
        menuComon.SetActive(false);
        pointerMenu.SetActive(false);
        instructions.SetActive(false);
        cursor.SetActive(false);
       
    }

    void LoadingTrigger()
    {
        loadingProgress = 0f;
        mainMenu.SetActive(false);
        background.SetActive(true);
        loading.SetActive(true);
    }

    void LoginTrigger()
    {
        background.SetActive(true);
        login.SetActive(true);
    }

    void MenuTrigger()
    {
        background.SetActive(true);
        menuComon.SetActive(true);
        cursor.SetActive(true);
        if (lastState == InterfaceState.Clear)
        {
            mainMenu.SetActive(true);
            pointerMenu.SetActive(true);
        }
    }

    void MenuPointerTrigger()
    {
        instructions.SetActive(false);
        pointerMenu.SetActive(true);
    }

    void InstructionTrigger()
    {
        pointerMenu.SetActive(false);
        instructions.SetActive(true);
    }

    void OnLoginPress()
    {
        JointOverlayerCalibration.Instance.ContinueHandAnimation(CalibrationState.GetPlayerSkeleton);
    }

    void OnCalibratePress()
    {
        background.SetActive(false);
        menuComon.SetActive(false);
        pointerMenu.SetActive(false);
    }

    void OnInstructionsPress()
    {
        myState = InterfaceState.Instruction;
    }

    void LoadingProgress()
    {
        loadingProgress += Time.deltaTime / loadingDelay;
        if (loadingProgress < 1f)
            loadingpProgress.fillAmount = loadingProgress;
        else 
            LoaingComplete();
    }

    void LoaingComplete()
    {
        if (!loadGame)
        {
            loadGame = true;
            Application.LoadLevelAsync(nextLevel);
        }
    }

    void PopUpCountdown()
    {
        popUpCounter -= Time.deltaTime;
        if (popUpCounter >= 0f)
            popUpProgress.fillAmount = 1f - (popUpCounter / popUpDelay);
        else
            DisablePopUp();
    }

    void DisablePopUp()
    {
        popUpEnabled = false;
        rangeWarning.SetActive(false);
        headMovementWarning.SetActive(false);
        handMovementWarning.SetActive(false);
        cineticWarning.SetActive(false);
        popUp.SetActive(false);
        if (currentPopUp == PopUps.Cinetic)
            OnChooseLevelPress(2);
    }

    public void OnChooseLevelPress(int level)
    {
        nextLevel = level;
        SetInterfaceState(InterfaceState.Loading);
    }

    public void SetInterfaceState(InterfaceState nextState)
    {
        myState = nextState;
    }

    public void EnablePopUp(PopUps _popUp)
    {
        popUpCounter = popUpDelay;
        currentPopUp = _popUp;
        popUp.SetActive(true);
        popUpEnabled = true;

        switch (_popUp)
        {
            case PopUps.Range:
                rangeWarning.SetActive(true);
                break;
            case PopUps.HeadMovement:
                headMovementWarning.SetActive(true);
                break;
            case PopUps.HandMovement:
                handMovementWarning.SetActive(true);
                break;
            case PopUps.Cinetic:
                cineticWarning.SetActive(true);
                break;
            default:
                break;
        }
    }
}

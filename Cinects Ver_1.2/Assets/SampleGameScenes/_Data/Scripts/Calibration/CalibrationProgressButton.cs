using UnityEngine;
using System.Collections;

public class CalibrationProgressButton : UIButton
{
    public ButtonType bt;

    UISprite progressSprite;
    ButtonType myType;
    int level = 0;
    bool once = false;

    float progressDelay = 2f;
    float progressCounter;

    [System.NonSerialized]
    protected State lastState = State.Normal;
    [System.NonSerialized]
    UISprite mSprite;
    [System.NonSerialized]
    UI2DSprite mSprite2D;
    [System.NonSerialized]
    string mNormalSprite;
    [System.NonSerialized]
    UnityEngine.Sprite mNormalSprite2D;

    void Start()
    {
        progressSprite = this.GetComponent<ProgressHolder>().progress;
        myType = this.GetComponent<ProgressHolder>().buttonType;
        level = this.GetComponent<ProgressHolder>().level;
    }

    void Update()
    {
        OnStateChecker(mState);
        if (mState == State.Hover)
        {
            OnProgressChecker(); 
        }
    }

    void OnStateChecker(State state)
    {
        if (lastState != state)
        {
            if (state == State.Hover)
            {
                progressSprite = this.GetComponent<ProgressHolder>().progress;
            }
            if (lastState == State.Hover)
            {
                progressCounter = 0f;
                progressSprite.fillAmount = progressCounter;
            }
            lastState = state;
        }
    }

    void OnProgressChecker()
    {
        progressCounter += Time.deltaTime / progressDelay;
        if (progressCounter < 1f)
        {
            progressSprite.fillAmount = progressCounter;
        }
        else
        {
            SetState(State.Pressed, true);
            progressSprite.fillAmount = 0f;
            OnButtonPress();
        }
    }

    public void OnButtonPress()
    {
        if (!once)
        {
            once = true;
            if (myType == ButtonType.LoadLevel)
                JointOverlayerMenu.Instance.SetLextLevel(level);
            
            this.GetComponent<BoxCollider>().enabled = false;
        }
    }
}

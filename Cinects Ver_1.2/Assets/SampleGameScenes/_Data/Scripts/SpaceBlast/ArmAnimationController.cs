using UnityEngine;
using System.Collections;

public enum ArmState
{
    Idle = 0,
    Attack,
    Recharge,
    GetItem,
    Pull
}

public class ArmAnimationController : MonoBehaviour
{
    ArmObjectsManager armObjectsManager;
    Animator armAnimator;
    private ArmState armState;
    private ArmState lastArmState;

    public ArmState ArmState
    {
        get { return armState; }
        set { armState = value; }
    }

    // Use this for initialization
    void Start ()
    {
        armObjectsManager = this.gameObject.GetComponent<ArmObjectsManager>();
        armAnimator = this.gameObject.GetComponent<Animator>();
        armState = ArmState.Recharge;
        lastArmState = ArmState.Idle;
    }

    // Update is called once per frame
    void Update ()
    {
        StateChecker();
    }

    public void SetArmAnimatorState(ArmState nextState)
    {
        armState = nextState;
    }

    public void StopPulling()
    {
        armAnimator.SetBool("Pull", false);
        armAnimator.SetBool("GetPowerUp", false);
        armObjectsManager.StopMagneticPulse();
        Invoke("OnGetItem", 1.5f);
    }

    void StartMagneticPulse()
    {
        armObjectsManager.StartMagneticPulse();
        armState = ArmState.GetItem;
    }

    void StateChecker()
    {
        if (lastArmState != armState)
        {
            switch (armState)
            {
                case ArmState.Attack: OnAttack(); break;
                case ArmState.Pull: OnPull(); break;
                case ArmState.Recharge: OnRecharge(); break;
                default: break;
            }
        }
        lastArmState = armState;
    }

    void OnAttack()
    {
        armAnimator.SetTrigger("Attack");
        armState = ArmState.Attack;
    }

    void OnPull()
    {
        armAnimator.SetBool("Pull", true);
        SpaceBlastGameController.Instance.objectIndex = 3;
        armState = ArmState.Pull;
    }

    void OnGetItem()
    {
        armAnimator.SetBool("GetPowerUp", true);
        armState = ArmState.GetItem;
    }

    void OnRecharge()
    {
        armAnimator.SetTrigger("Recharge");
        armState = ArmState.Recharge;
    }

    void OnIdle()
    {
        if (!armAnimator.GetBool("Pull"))
            armState = ArmState.Idle;
    }

    void OnGetRechargeObject()
    {
        if (armState == ArmState.GetItem)
        {
            armObjectsManager.ClearPowerUp();
        }
        armState = ArmState.Idle;
        armObjectsManager.OnChangeObjectInArm();
    }

    void OnLaunchMissile()
    {
        armObjectsManager.LaunchMissile();
    }
}

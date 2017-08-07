using UnityEngine;
using System.Collections;


public enum InteractionSource
{
    FromCollider = 0,
    FromRayCast
}

public enum Collider_ID
{
    Knive,
    CuttingTable,
    RawPizza,
    Oven
}

public class PizzaObjectCollider : MonoBehaviour 
{
    protected Items itemType;
    protected int overlayerObject_ID;
    protected PizzaGameController gameController;
    protected float interactionProgress;
    protected bool updateProgress;
    protected bool borderEnabled;
    protected Collider_ID collider_ID;

    public Collider_ID Collider_ID
    {
        get { return collider_ID; }
        set { collider_ID = value; }
    }

    protected InteractionSource interactionSource = InteractionSource.FromCollider;

    public InteractionSource CurrentInteractionSource
    {
        get { return interactionSource; }
        set { interactionSource = value; }
    }

    public Items ItemType { get { return this.itemType; } }
    public bool BorderEnabled { get {return this.borderEnabled; } }

    protected delegate void OnCircleFilledAction();
    protected event OnCircleFilledAction OnCircleFilled;

    protected virtual void Start()
	{
		gameController = PizzaGameController.Instance;
	}

    protected virtual void Update()
	{
        SetFeedbackCircle();
    }

    public virtual void SetBorderCondition(bool condition)
    {
        borderEnabled = condition;
        JointOverlayerPizzaMaker.Instance.SetCurrentOverlayerBorderCondition(borderEnabled);
    }
    public virtual void OverlayerObject_ID(int _id)
    {
        overlayerObject_ID = _id;
        itemType = (Items)overlayerObject_ID;
    }

    public virtual void InteractWithCollider()
	{
		gameController.TryToInteract(collider_ID);
	}

    public virtual void GetItem()
	{
	}

    public virtual void ReleaseItem()
	{
		
	}

    protected virtual void SetFeedbackCircle()
    {
        if (updateProgress)
        {
            interactionProgress += Time.deltaTime;
            if (interactionProgress >= 0.5f)
            {
                float localProgress = (interactionProgress - 0.5f) * 0.9f;
                UIInteractionController.Instance.FilledCircleAmount(localProgress, true);

            }
            if (interactionProgress >= 1.75f)
            {
                updateProgress = false;
                interactionProgress = 0f;
                UIInteractionController.Instance.FilledCircleAmount(interactionProgress, true);
                OnCircleFilled();
            }
        }
    }

    protected virtual void  OnTriggerEnter(Collider hit)
	{
		
	}

    protected virtual void  OnTriggerExit(Collider hit)
	{
		
	}
}
using UnityEngine;
using System.Collections;

public class PizzaKniveObjectCollider : PizzaObjectCollider 
{
    void OnEnable()
    {
        collider_ID = Collider_ID.Knive;
        OnCircleFilled += ReleaseItem;
    }

    void OnDisable()
    {
        OnCircleFilled -= ReleaseItem;
    }

    public override void GetItem()
    {
        gameController.SetOverlayerObjectToGrab(overlayerObject_ID);
    }

    public override void ReleaseItem()
    {
        gameController.SetOverlayerObjectToRelease();
    }

    protected override void OnTriggerEnter(Collider hit)
    {
        if (hit.tag == "DraggableKnive" && JointOverlayerPizzaMaker.Instance.OverlayerObjectState == OverlayerObjectState.InHand)
        {
            updateProgress = true;
            interactionSource = InteractionSource.FromCollider;
            UIInteractionController.Instance.SetObjectLabel(itemType, ActionType.Identify);
        }
    }

    protected override void OnTriggerExit(Collider hit)
    {
        if (hit.tag == "DraggableKnive" && JointOverlayerPizzaMaker.Instance.OverlayerObjectState == OverlayerObjectState.InHand)
        {
            updateProgress = false;
            interactionProgress = 0f;
            UIInteractionController.Instance.FilledCircleAmount(interactionProgress, true);
            UIInteractionController.Instance.ClearFeedbackText();
        }
    }
}
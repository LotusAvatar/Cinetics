using UnityEngine;
using System.Collections;

public enum CuttingTableState
{
    OnFood_01 = 0,
    OnFood_02,
    OnFood_03
}

public class PizzaCuttingTableCollider : PizzaObjectCollider
{
    public GameObject myBorder;
    public GameObject cuttingKniveParticle;
    public Transform[] particlesPlaceHolder;
    GameObject newParticle;
    bool boxColliderEnabled;

    private CuttingTableState myState;
    
    public CuttingTableState CurrentState
    {
        get { return myState; }
    }

    public bool BoxColliderEnabled
    {
        get { return boxColliderEnabled; }
    }


    void OnEnable()
    {
        collider_ID = Collider_ID.CuttingTable;
        EnableBoxCollider(false);
        OnCircleFilled += InteractWithCollider;
        JointOverlayerPizzaMaker.OnInteraction += OnInteractionAction;
    }

    void OnDisable()
    {
        OnCircleFilled -= InteractWithCollider;
        JointOverlayerPizzaMaker.OnInteraction -= OnInteractionAction;
    }

    void OnInteractionAction()
    {
        if (interactionSource == InteractionSource.FromCollider)
        {
            SetBorderCondition(false);
            if (JointOverlayerPizzaMaker.Instance.CurrentCollider_ID == collider_ID)
            {
                Invoke("EnableParticles", 1f);
                Invoke("EnableCuttedIngredient", 4f);
            }
        }
    }

    void EnableCuttedIngredient()
    {
        if (myState != CuttingTableState.OnFood_03)
            gameController.CuttingIngredient();
    }

    public override void InteractWithCollider()
    {
         base.InteractWithCollider();
    }

    public override void SetBorderCondition(bool condition)
    {
        borderEnabled = condition;
        myBorder.SetActive(borderEnabled);
    }

    public void EnableBoxCollider(bool condition)
    {
        if (!condition)
            myState = CuttingTableState.OnFood_01;

        boxColliderEnabled = condition;
        gameObject.GetComponent<BoxCollider>().enabled = condition;
    }

    public void EnableParticles()
    {
        for (int i = 0; i < particlesPlaceHolder.Length; i++)
        {
            newParticle = (GameObject)Instantiate(cuttingKniveParticle,
                particlesPlaceHolder[i].position, particlesPlaceHolder[i].rotation) as GameObject;

            newParticle.transform.SetParent(particlesPlaceHolder[i]);
        }
    }

    public void IngredientInPlace(PizzaIngredients ingredient)
    {
        int ingredientToInt = (int)ingredient % 3;
        myState = (CuttingTableState)ingredientToInt;
        EnableBoxCollider(true);
    }

    protected override void OnTriggerEnter(Collider hit)
    {
        if ((hit.tag == "DraggableKnive" || hit.tag == "DraggableIngredient") &&
            JointOverlayerPizzaMaker.Instance.OverlayerObjectState == OverlayerObjectState.InHand)
        {
            updateProgress = true;
            interactionSource = InteractionSource.FromCollider;
            if (hit.tag == "DraggableKnive")
                UIInteractionController.Instance.SetIngredientsLabel(PizzaIngredients.Cheese, ActionType.CutIngredient);
            else 
                UIInteractionController.Instance.SetIngredientsLabel(PizzaIngredients.Cheese, ActionType.ReleseObject);
            SetBorderCondition(true);
        }
    }

    protected override void OnTriggerExit(Collider hit)
    {
        if ((hit.tag == "DraggableKnive" || hit.tag == "DraggableIngredient") &&
            JointOverlayerPizzaMaker.Instance.OverlayerObjectState == OverlayerObjectState.InHand)
        {
            updateProgress = false;
            interactionProgress = 0f;
            UIInteractionController.Instance.FilledCircleAmount(interactionProgress, true);
            UIInteractionController.Instance.ClearFeedbackText();
            SetBorderCondition(false);
        }
    }
}

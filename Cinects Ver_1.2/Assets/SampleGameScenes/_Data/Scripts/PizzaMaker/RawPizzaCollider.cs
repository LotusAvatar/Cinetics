using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum RawPizzaState
{
    Empty = 0,
    HasIngredient,
    OnMovement
}

public class RawPizzaCollider : PizzaObjectCollider
{
    public GameObject myBorder;
    public GameObject interactParticle;
    public Transform particlePlaceHolder;
    public Transform tableRef;
    public Transform outSceneRef;
    public Transform rawPizza;
    public Transform transformRef;
    List<PizzaIngredients> currentPizzaingredients = new List<PizzaIngredients>();
    Dictionary<IngredientType, int> ingredientTypeAmount = new Dictionary<IngredientType, int>();
    GameObject newParticle;
    bool boxColliderEnabled;

    private RawPizzaState myState;

    public RawPizzaState CurrentState
    {
        get { return myState; }
    }

    public bool BoxColliderEnabled
    {
        get { return boxColliderEnabled; }
    }

    void OnEnable()
    {
        collider_ID = Collider_ID.RawPizza;
        OnCircleFilled += IngredientInteractingWithPizza;
        InitializeWithCheese();
    }

    void OnDisable()
    {
        OnCircleFilled -= IngredientInteractingWithPizza;
    }

    public override void InteractWithCollider()
    {
        if (gameController.OvenIsEmpty)
        {
            ResetTransformReferences();
            gameController.OnChangePizzaInteraction();
            ClearRawPizza();
        }
    }

    public void ResetTransformReferences()
    {
        transformRef.localPosition = outSceneRef.localPosition;
        transformRef.localRotation = outSceneRef.localRotation;
        rawPizza.localPosition = outSceneRef.localPosition;
        rawPizza.localRotation = outSceneRef.localRotation;
    }

    public override void SetBorderCondition(bool condition)
    {
        borderEnabled = condition;
        myBorder.SetActive(borderEnabled);
    }

    public void EnableBoxCollider(bool condition)
    {
        boxColliderEnabled = condition;
        gameObject.GetComponent<BoxCollider>().enabled = condition;
    }

    public void AddIngredientInList(PizzaIngredientPerType ingredientReference)
    {
        currentPizzaingredients.Add(ingredientReference.ingredient);
        if (ingredientTypeAmount.ContainsKey(ingredientReference.ingredientType))
            ingredientTypeAmount[ingredientReference.ingredientType]++;
        else
            ingredientTypeAmount.Add(ingredientReference.ingredientType, 1);
    }

    public void ClearRawPizza()
    {
        myState = RawPizzaState.Empty;
        SetBorderCondition(false);
        gameController.ClearIngredientsInPizza(ingredientTypeAmount);
        currentPizzaingredients.Clear();
        currentPizzaingredients = new List<PizzaIngredients>();
        ingredientTypeAmount.Clear();
        InitializeWithCheese();
    }

    void InitializeWithCheese()
    {
        myState = RawPizzaState.HasIngredient;
        PizzaIngredientPerType _pizza = new PizzaIngredientPerType();
        _pizza.ingredient = PizzaIngredients.CheeseSmall;
        _pizza.ingredientType = IngredientType.Cheese;
        IngredientsController.Instance.EnableIngredientInPizza(_pizza.ingredient);
        ingredientTypeAmount.Add(_pizza.ingredientType, 1);
        EnableBoxCollider(true);
    }

    void IngredientInteractingWithPizza()
    {
        myState = RawPizzaState.HasIngredient;
        gameController.IngredientInteractingWithPizza();
        newParticle = (GameObject)Instantiate(interactParticle,
               particlePlaceHolder.position, particlePlaceHolder.rotation) as GameObject;

        newParticle.transform.SetParent(particlePlaceHolder);
        SetBorderCondition(false);
    }

    protected override void OnTriggerEnter(Collider hit)
    {
        if ((hit.tag == "DraggableIngredient") &&
            JointOverlayerPizzaMaker.Instance.OverlayerObjectState == OverlayerObjectState.InHand)
        {
            updateProgress = true;
            interactionSource = InteractionSource.FromCollider;
            UIInteractionController.Instance.SetIngredientsLabel(JointOverlayerPizzaMaker.Instance.CurrentPizzaIngredient.ingredient, ActionType.PutInPizza);
            SetBorderCondition(true);
        }
    }

    protected override void OnTriggerExit(Collider hit)
    {
        if ((hit.tag == "DraggableIngredient") &&
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

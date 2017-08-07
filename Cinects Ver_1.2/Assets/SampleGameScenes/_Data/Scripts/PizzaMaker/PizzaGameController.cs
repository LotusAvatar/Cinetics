using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ItemState
{
	Cursor = 0,
	Dragging,
	Interacting,
	Changing
}

public class PizzaGameController : MonoBehaviour 
{
    #region InretactionControllerMemebers

    ItemState currentState = ItemState.Cursor;
	HandEventType handstate;
    public PizzaCuttingTableCollider cuttingTable;
    public RawPizzaCollider rawPizza;
    public OvenCollider ovenCollider;

    public bool OvenIsEmpty
    {
        get { return (ovenCollider.CurrentState == OvenState.Empty); }
    }

    public bool CanGrabIngredient
    {
        get { return (cuttingTable.CurrentState != CuttingTableState.OnFood_01); }
    }

    public bool RawPizzaHasIngredient
    {
        get { return (rawPizza.CurrentState != RawPizzaState.Empty); }
    }

    public ItemState CurrentState 
	{
		get {return this.currentState;}
		set {currentState = value;}
	}

	public HandEventType Handstate 
	{
		get {return this.handstate;}
		set {handstate = value;}
	}

	#endregion

	#region PizzaRulesMembers
	
	#endregion

	private static PizzaGameController instance;
	
	
	/// <summary>
	/// Gets the single PizzaGameController instance.
	/// </summary>
	/// <value>The PizzaGameController instance.</value>
	public static PizzaGameController Instance
	{
		get
		{
			if (instance == null)
			{
				instance = (PizzaGameController)GameObject.FindObjectOfType(typeof(PizzaGameController));
			}
			return instance;
		}
	}

    #region InretactionController

    void Start()
    {
        InitializePizzasTansformsReference();
    }

    void InitializePizzasTansformsReference()
    {
        JointOverlayerPizzaMaker.Instance.InitializePizzasTransform(ovenCollider.bakedPizza,
            ovenCollider.transformReference, ovenCollider.ovenPosReference, ovenCollider.playerPosReference, 
            rawPizza.rawPizza, rawPizza.transformRef, rawPizza.tableRef);
    }

	public void SetOverlayerObjectToRelease()
	{
		JointOverlayerPizzaMaker.Instance.OverlayerNextState (ItemState.Cursor);
	}

	public void SetOverlayerObjectToGrab(int overlayerObject_ID)
	{
		JointOverlayerPizzaMaker.Instance.OverlayerNextState (ItemState.Dragging);
	}

    public void OnChangePizzaInteraction()
    {
        ovenCollider.AddIngredientsInPizza();
        ovenCollider.ResetTransformReferences();
        JointOverlayerPizzaMaker.Instance.OverlayerNextState(ItemState.Interacting, Collider_ID.RawPizza);
    }

    public void ClearOven()
    {
        ovenCollider.ClearOven();
    }

	public void TryToInteract(Collider_ID collider_ID)
	{
        JointOverlayerPizzaMaker.Instance.OverlayerNextState(ItemState.Interacting, collider_ID);
    }

    public void ChangePizzaIngredientTo(PizzaIngredients ingredient)
    {
        cuttingTable.IngredientInPlace(ingredient);
    }

    public void CuttingIngredient()
    {
        IngredientsController.Instance.IngredientHasBeenCutted();
    }

    public void AddIngredientInPizza(PizzaIngredientPerType ingredientReference)
    {
        IngredientsController.Instance.EnableIngredientInPizza(ingredientReference.ingredient);
        rawPizza.AddIngredientInList(ingredientReference);
        cuttingTable.EnableBoxCollider(false);
    }

    public void IngredientInteractingWithPizza()
    {
        JointOverlayerPizzaMaker.Instance.OverlayerNextState(ItemState.Cursor, Collider_ID.RawPizza);
    }

    public void ClearIngredientsInPizza(Dictionary<IngredientType, int> _ingredientTypeAmount)
    {
        IngredientsController.Instance.ClearRawPizza(_ingredientTypeAmount);
    }

	#endregion
	
	#region PizzaRules
	
	#endregion
	
}

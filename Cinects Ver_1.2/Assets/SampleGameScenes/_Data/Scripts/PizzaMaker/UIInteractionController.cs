using UnityEngine;
using System.Collections;

public enum ActionType
{
    Identify = 0,
    GrabObject,
    ReleseObject,
    CutIngredient,
    PutInPizza,
    BakeInOven
}
public class UIInteractionController : MonoBehaviour
{

    public UISprite handCursor;
    public UISprite handFilledCircle;
    public UISprite objectFilledCircle;
    public UILabel feedbackLabel;


    private static UIInteractionController instance;
    
    public static UIInteractionController Instance
    {
        get
        {
            if (instance ==  null)
                instance = (UIInteractionController)GameObject.FindObjectOfType(typeof( UIInteractionController));
            
            return instance;
        }
    }

    private ActionType currentLabelAction;

    public ActionType CurrentLabelAction
    {
        get { return currentLabelAction; }
        set { currentLabelAction = value; }
    }


    public void SetIngredientsLabel(PizzaIngredients ingredients, ActionType actionType)
    {
        string ingredinetName = "";
        string actionText = "";

        switch (actionType)
        {
            case ActionType.CutIngredient: actionText = "Cortar/n"; break;
            case ActionType.GrabObject: actionText = "Pegar/n"; break;
            case ActionType.PutInPizza: actionText = "Acrescentar/n"; break;
            default: break;
        }

        switch (ingredients)
        {
            case PizzaIngredients.Bacon: ingredinetName = "Bacon"; break;
            case PizzaIngredients.BaconMedium: ingredinetName = "Fatias de Bacon"; break;
            case PizzaIngredients.BaconSmall: ingredinetName = "Bacon em cubos"; break;
            case PizzaIngredients.Champignon: ingredinetName = "Champignon"; break;
            case PizzaIngredients.ChampignonMedium: ingredinetName = "Fatias de Champignon"; break;
            case PizzaIngredients.ChampignonSmall: ingredinetName = "Champignon em cubos"; break;
            case PizzaIngredients.Cheese: ingredinetName = "Muzzarela"; break;
            case PizzaIngredients.CheeseMedium: ingredinetName = "Fatias de Muzzarela"; break;
            case PizzaIngredients.CheeseSmall: ingredinetName = "Muzzarela em cubos"; break;
            case PizzaIngredients.Olives: ingredinetName = "Azeitona"; break;
            case PizzaIngredients.OlivesMedium: ingredinetName = "Azeitona Inteira"; break;
            case PizzaIngredients.OlivesSmall: ingredinetName = "Fatias de Azeitona"; break;
            case PizzaIngredients.Onion: ingredinetName = "Cebola"; break;
            case PizzaIngredients.OnionMedium: ingredinetName = "Aneis de Cebola"; break;
            case PizzaIngredients.OnionSmall: ingredinetName = "Cebola em cubos"; break;
            case PizzaIngredients.Pepperoni: ingredinetName = "Pepperoni"; break;
            case PizzaIngredients.PepperoniMedium: ingredinetName = "Fatias de Pepperoni"; break;
            case PizzaIngredients.PepperoniSmall: ingredinetName = "Pepperoni em cubos"; break;
            case PizzaIngredients.RedPepper: ingredinetName = "Pimentão"; break;
            case PizzaIngredients.RedPepperMedium: ingredinetName = "Pimentão"; break;
            case PizzaIngredients.RedPepperSmall: ingredinetName = "Pimentão em cubos"; break;
            case PizzaIngredients.Shrimp: ingredinetName = "Camarão"; break;
            case PizzaIngredients.ShrimpMedium: ingredinetName = "Camarão descascado"; break;
            case PizzaIngredients.ShrimpSmall: ingredinetName = "Filetes de Camarão"; break;
            case PizzaIngredients.Tomato: ingredinetName = "Tomate"; break;
            case PizzaIngredients.TomatoMedium: ingredinetName = "Fatias de Tomate"; break;
            case PizzaIngredients.TomatoSmall: ingredinetName = "Tomate em cubos"; break;
            default: break;
        }
    }

    public void SetObjectLabel(Items itemType, ActionType actionType)
    {
        string itemText = "";
        string actionText = "";
        
        switch (actionType)
        {
            case ActionType.GrabObject: actionText = "Pegar/n"; break;
            case ActionType.ReleseObject: actionText = "Guardar/n"; break;
            case ActionType.BakeInOven: actionText = "Assar/n"; break;
            default: break;
        }

        switch (itemType)
        {
            case Items.CuttingTable: itemText = "Tabua"; break;
            case Items.Knive: itemText = "Faca"; break;
            case Items.Pizza: itemText = "Pizza"; break;
            case Items.Oven: itemText = "Forno"; break;
            default: break;
        }

        string text = "";
        if (PizzaGameController.Instance.CurrentState == ItemState.Cursor)
        {
            text += "Pegar ";
        }
        else if (PizzaGameController.Instance.CurrentState == ItemState.Dragging)
        {
            text += "Guardar ";
        }
        else
            return;

        switch (itemType)
        {
            case Items.Knive: text += "Faca"; break;
            default:
                break;
        }

        feedbackLabel.text = text;
    }

    public void ClearFeedbackText()
    {
        feedbackLabel.text = "";
    }

    public void FilledCircleAmount(float amount, bool objectCircle = false)
    {
        if (objectCircle)
            objectFilledCircle.fillAmount = amount;
        else
            handFilledCircle.fillAmount = amount;
    }
}

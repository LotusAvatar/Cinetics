using UnityEngine;
using System.Collections;

public enum TypeChoiceState
{
    Nope = 0,
    ShowTypeIngredients,
    HideTypeIngredients,
    Showing
}

public class IngredientPerTypeChoiceCollider : PizzaObjectCollider
{
    public PizzaIngredientPerType[] ingredientsPerType;
    public GameObject backgroundCollider;
    public GameObject[] myBorder;
    
    protected Vector3 initialScale;
    protected Vector3 initialPos = Vector3.zero;
    protected float progress;
    protected float progressSpeed = 2f;
    protected TypeChoiceState lastState = TypeChoiceState.Nope;
    protected TypeChoiceState currentState = TypeChoiceState.Nope;

    protected override void Start()
    {
        initialScale = Vector3.one * 0.1f;
    }

    protected override void Update()
    {
        StateChecker(currentState);
    }

    public override void InteractWithCollider()
    {
        ShowIngredientsOfMyType();
    }

    public void ShowIngredientsOfMyType()
    {
        currentState = TypeChoiceState.ShowTypeIngredients;
    }

    public void EnableIngredientOnTable()
    {
        currentState = TypeChoiceState.Nope;
    }

    public override void SetBorderCondition(bool condition)
    {
        if (borderEnabled != condition)
        {
            borderEnabled = condition;
            for (int i = 0; i < myBorder.Length; i++)
                myBorder[i].SetActive(borderEnabled);
        }
    }

    public virtual void OnChooseIngredient()
    {
        currentState = TypeChoiceState.Nope;
    }

    protected void StateChecker(TypeChoiceState _currentState)
    {
        if (lastState != _currentState)
        {
            progress = 0f;
            switch (_currentState)
            {
                case TypeChoiceState.Nope:
                    OnNopeTrigger();
                    break;
                case TypeChoiceState.ShowTypeIngredients:
                    SetBorderCondition(false);
                    break;
                case TypeChoiceState.HideTypeIngredients:
                    EnableIngredientCollider(false);
                    DisableIngredientBorders();
                    backgroundCollider.SetActive(true);
                    break;
                case TypeChoiceState.Showing:
                    EnableIngredientCollider();
                    backgroundCollider.SetActive(true);
                    break;
                default:
                    break;
            }
        }

        switch (_currentState)
        {
             case TypeChoiceState.ShowTypeIngredients:
                OnShowMovement();
                break;
            case TypeChoiceState.HideTypeIngredients:
                OnShowMovement(false);
                break;
            case TypeChoiceState.Showing:
                Showing();
                break;
            default:
                break;
        }

        lastState = _currentState;

    }

    protected virtual void OnNopeTrigger()
    {
        if (lastState == TypeChoiceState.Showing)
        {
            DisableIngredientBorders();
            EnableIngredientCollider(false);
            for (int i = 0; i < ingredientsPerType.Length; i++)
            {
                ingredientsPerType[i].prefab.transform.localPosition = initialPos;
                ingredientsPerType[i].prefab.transform.localScale = initialScale;
            }
        }
        backgroundCollider.SetActive(false);
    }

    protected virtual void EnableIngredientCollider(bool enable = true)
    {
        for (int i = 0; i < ingredientsPerType.Length; i++)
            ingredientsPerType[i].prefab.GetComponent<IngredientChoiceCollider>().EnableSphereCollider(enable);
    }

    protected virtual void DisableIngredientBorders()
    {
        for (int i = 0; i < ingredientsPerType.Length; i++)
            ingredientsPerType[i].prefab.GetComponent<IngredientChoiceCollider>().SetBorderCondition(false);
    }

    protected virtual void Showing()
    {
        for (int i = 0; i < ingredientsPerType.Length; i++)
            ingredientsPerType[i].prefab.transform.Rotate(Vector3.up, 1f);
    }

    protected virtual void OnShowMovement(bool show = true)
    {
        progress += Time.deltaTime * progressSpeed;
        Vector3 onProgressPos = new Vector3();
        Vector3 onProgressScale = new Vector3();
        Showing();
        if (show)
        {
            if (progress < 1f)
            {
                for (int i = 0; i < ingredientsPerType.Length; i++)
                {
                    onProgressPos = Vector3.Lerp(initialPos, ingredientsPerType[i].cuttingTableReference.localPosition, progress);
                    onProgressScale = Vector3.Lerp(initialScale, Vector3.one, progress);
                    ingredientsPerType[i].prefab.transform.localPosition = onProgressPos;
                    ingredientsPerType[i].prefab.transform.localScale = onProgressScale;
                }
            }
            else
            {
                for (int i = 0; i < ingredientsPerType.Length; i++)
                {
                    onProgressPos = ingredientsPerType[i].cuttingTableReference.localPosition;
                    ingredientsPerType[i].prefab.transform.localPosition = onProgressPos;
                    ingredientsPerType[i].prefab.transform.localScale = Vector3.one;
                }
                currentState = TypeChoiceState.Showing;
            }
        }
        else
        {
            if (progress < 1f)
            {
                for (int i = 0; i < ingredientsPerType.Length; i++)
                {
                    onProgressPos = Vector3.Lerp(ingredientsPerType[i].cuttingTableReference.localPosition, initialPos, progress);
                    onProgressScale = Vector3.Lerp(Vector3.one, initialScale, progress);
                    ingredientsPerType[i].prefab.transform.localPosition = onProgressPos;
                    ingredientsPerType[i].prefab.transform.localScale = onProgressScale;
                }
            }
            else
            {
                for (int i = 0; i < ingredientsPerType.Length; i++)
                {
                    ingredientsPerType[i].prefab.transform.localPosition = initialPos;
                    ingredientsPerType[i].prefab.transform.localScale = initialScale;
                }

                currentState = TypeChoiceState.Nope;
            }
        }
    }
}

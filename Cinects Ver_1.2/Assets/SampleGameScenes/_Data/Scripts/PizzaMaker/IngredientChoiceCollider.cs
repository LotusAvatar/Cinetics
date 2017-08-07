using UnityEngine;
using System.Collections;

public class IngredientChoiceCollider : PizzaObjectCollider
{
    public IngredientPerTypeChoiceCollider parent;
    public GameObject[] myBorders;
    public PizzaIngredients ingredient;

    protected override void Start()
    {
        base.Start();
        EnableSphereCollider(false);
        SetBorderCondition(false);
    }

    public override void SetBorderCondition(bool condition)
    {
        borderEnabled = condition;
        for (int i = 0; i < myBorders.Length; i++)
            myBorders[i].SetActive(borderEnabled);
    }

    public override void InteractWithCollider()
    {
        EnableIngredientOnTable();
    }

    public void EnableSphereCollider(bool enable)
    {
        gameObject.GetComponent<SphereCollider>().enabled = enabled;
    }

    void EnableIngredientOnTable()
    {
        parent.EnableIngredientOnTable();
        IngredientsController.Instance.EnableIngredientOnTable(ingredient);
    }
}

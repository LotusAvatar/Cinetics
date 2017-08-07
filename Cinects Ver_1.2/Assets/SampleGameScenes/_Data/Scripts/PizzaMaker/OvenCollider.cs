using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum OvenState
{
    Empty = 0,
    Baking,
    OnMovement
}

[System.Serializable]
public struct TypeObjectIngredient
{
    public PizzaIngredients ingredient;
    public GameObject prefab;
}

public class OvenCollider : PizzaObjectCollider
{
    public GameObject myBorder;
    public ParticleSystem[] steamParticles;
    public Transform tablePosReference;
    public Transform ovenPosReference;
    public Transform transformReference;
    public Transform playerPosReference;
    public Transform bakedPizza;
    public Material bakedPizzaMat;
    public ClockBehavior clockController;
    public TypeObjectIngredient[] ingredients;
    List<PizzaIngredients> currentPizzaingredients = new List<PizzaIngredients>();
    Dictionary<PizzaIngredients, GameObject> ingredientsDictionary = new Dictionary<PizzaIngredients, GameObject>();
    GameObject newParticle;
    bool boxColliderEnabled;
    Material rawMaterial;

    private OvenState myState;

    public OvenState CurrentState
    {
        get { return myState; }
    }

    public bool BoxColliderEnabled
    {
        get { return boxColliderEnabled; }
    }

    protected override void Start()
    {
        base.Start();
        EnableBoxCollider(false);
        collider_ID = Collider_ID.Oven;
        for (int i = 0; i < ingredients.Length; i++)
            ingredientsDictionary.Add(ingredients[i].ingredient, ingredients[i].prefab);
    }

    public override void InteractWithCollider()
    {
        for (int i = 0; i < steamParticles.Length; i++)
            steamParticles[i].Stop();
        ArangeToShowPizzaForPlayer();
        base.InteractWithCollider();
    }

    public void ResetTransformReferences()
    {
        transformReference.localPosition = tablePosReference.localPosition;
        transformReference.localRotation = tablePosReference.localRotation;
        bakedPizza.localPosition = tablePosReference.localPosition;
        bakedPizza.localRotation = tablePosReference.localRotation;
    }

	public GameObject KitchenRef;
	public GameObject Victory1; // objeto que guarda script de vitória.
	public GameObject Victory2;
	public GameObject Victory3;

	public int levelAtual= 0; // Variavel que guarda o numero do level.
	public void level1()// Metodos para definir qual o level atual.
	{
		levelAtual=1;
	}
	public void level2()// Metodos para definir qual o level atual.
	{
		levelAtual=2;
	}
	public void level3()// Metodos para definir qual o level atual.
	{
		levelAtual=3;
	}

    public void ArangeToShowPizzaForPlayer()
    {
        transformReference.localPosition = ovenPosReference.localPosition;
        transformReference.localRotation = ovenPosReference.localRotation;
		if (levelAtual == 1) {
			Victory1.GetComponent<level1victory> ().Victory ();
		} else if (levelAtual == 2) {
			Victory2.GetComponent<level2victory> ().Victory ();
		}else if (levelAtual == 3) {
			Victory3.GetComponent<level3victory> ().Victory ();
		}
		//SALVAR TEMPO***********************************************************************************************************
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

    public void AddIngredientsInPizza()
    {
        myState = OvenState.Baking;
        clockController.StateTrigger(ClockState.Ticking);
        bakedPizza.gameObject.SetActive(true);
        for (int i = 0; i < IngredientsController.Instance.EnabledIngredientsInRawPizza.Count; i++)
        {
            currentPizzaingredients.Add(IngredientsController.Instance.EnabledIngredientsInRawPizza[i]);
            ingredientsDictionary[IngredientsController.Instance.EnabledIngredientsInRawPizza[i]].SetActive(true);
        }
        Invoke("PizzaIsBaked", 5f);
    }

    public void ClearOven()
    {
        myState = OvenState.Empty;

        SetBorderCondition(false);
        EnableBoxCollider(false);

        for (int i = 0; i < currentPizzaingredients.Count; i++)
            ingredientsDictionary[currentPizzaingredients[i]].SetActive(false);

        bakedPizza.GetComponent<MeshRenderer>().material = rawMaterial;
        bakedPizza.gameObject.SetActive(false);
    }

    public void DisableCheese()
    {
        for (int i = 0; i < currentPizzaingredients.Count; i++)
            if (currentPizzaingredients[i] == PizzaIngredients.CheeseMedium || currentPizzaingredients[i] == PizzaIngredients.CheeseSmall)
                ingredientsDictionary[currentPizzaingredients[i]].SetActive(false);
    }

    void PizzaIsBaked()
    {
        Debug.Log("*****PizzaIsBaked!*****");
        clockController.StateTrigger(ClockState.Ringing);
        for (int i = 0; i < steamParticles.Length; i++)
            steamParticles[i].Play();
        rawMaterial = bakedPizza.GetComponent<MeshRenderer>().material;
        bakedPizza.GetComponent<MeshRenderer>().material = bakedPizzaMat;
        DisableCheese();
        EnableBoxCollider(true);
    }

}

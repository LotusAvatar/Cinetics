using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PizzaIngredients
{
	Bacon = 0,
	BaconMedium,
	BaconSmall,
	Champignon,
	ChampignonMedium,
	ChampignonSmall,
	Cheese,
	CheeseMedium,
	CheeseSmall,
	Olives,
	OlivesMedium,
	OlivesSmall,
	Onion,
	OnionMedium,
	OnionSmall,
	Pepperoni,
	PepperoniMedium,
	PepperoniSmall,
	RedPepper,
	RedPepperMedium,
	RedPepperSmall,
	Shrimp,
	ShrimpMedium,
	ShrimpSmall,
	Tomato,
	TomatoMedium,
	TomatoSmall
}

public enum IngredientType
{
	Meat,
	Cheese,
	Seafood,
	Vegetable,
	Mushroom,
	Canned,
	Fruit,
	Sweet
}

[System.Serializable]
public struct PizzaIngredientPerType
{
	public GameObject prefab;
	public PizzaIngredients ingredient;
	public IngredientType ingredientType;
    public Transform cuttingTableReference;
}

public class IngredientsController : MonoBehaviour 
{
	public PizzaIngredientPerType[] tableIngredients;
	public PizzaIngredientPerType[] pizzaIngredients;
	Dictionary<PizzaIngredients, PizzaIngredientPerType> tableIngredientsDictionary = new Dictionary<PizzaIngredients, PizzaIngredientPerType>();
	Dictionary<PizzaIngredients, PizzaIngredientPerType> pizzaIngredientsDictionary = new Dictionary<PizzaIngredients, PizzaIngredientPerType>();
    Dictionary<IngredientType, int> ingredientTypeAmount = new Dictionary<IngredientType, int>();
    bool lookForInteraction;

    private List<PizzaIngredients> enabledIngredientsInRawPizza = new List<PizzaIngredients>();
    private List<PizzaIngredients> enabledIngredientsInBakedPizza = new List<PizzaIngredients>();

    public List<PizzaIngredients> EnabledIngredientsInRawPizza
    {
        get { return enabledIngredientsInRawPizza;}
    }
	//***************************************************
	//Contador de ingredientes na pizza
	public int Bacon=0;
	public int Champignon=0;
	public int Cheese=0;
	public int Olive=0;
	public int Onion=0;
	public int Pepperoni=0;
	public int RedPepper=0;
	public int Shrimp=0;
	public int Tomato=0;
	public string ingredienteadd=" ";

	//public GameObject Texto;
	//***************************************************


    private PizzaIngredientPerType onTheTable;

    public PizzaIngredientPerType OnTheTable
    {
        get { return onTheTable; }
    }

    public static IngredientsController _instance;
	
	public static IngredientsController Instance
	{
		get
		{
			if (_instance == null)
				_instance = (IngredientsController)GameObject.FindObjectOfType(typeof(IngredientsController));

            return _instance;
		}
	}

	public bool LookForInteraction 
	{
		get {return this.lookForInteraction;}
		set {lookForInteraction = value;}
	}

    // Use this for initialization

    void Awake () 
	{
		FillDictionaries();
	}
	
	void FillDictionaries()
	{
		for (int i = 0; i < tableIngredients.Length; i++) 
		{
			tableIngredientsDictionary.Add(tableIngredients[i].ingredient, tableIngredients[i]);
		}
		for (int i = 0; i < pizzaIngredients.Length; i++) 
		{
			pizzaIngredientsDictionary.Add(pizzaIngredients[i].ingredient, pizzaIngredients[i]);
		}
	}
	
    public void ClearCuttingTable()
    {
        if (onTheTable.prefab != null)
            onTheTable.prefab.SetActive(false);
    }

    public void ClearRawPizza(Dictionary<IngredientType, int> _ingredientTypeAmount)
    {
        ingredientTypeAmount.Clear();
        foreach (KeyValuePair<IngredientType, int> value in _ingredientTypeAmount)
            ingredientTypeAmount.Add(value.Key, value.Value);

        enabledIngredientsInBakedPizza.Clear();

        if (enabledIngredientsInRawPizza.Count > 0)
        {
            for (int i = 0; i < enabledIngredientsInRawPizza.Count; i++)
            {
                enabledIngredientsInBakedPizza.Add(enabledIngredientsInRawPizza[i]);
                pizzaIngredientsDictionary[enabledIngredientsInRawPizza[i]].prefab.SetActive(false);
            }

            enabledIngredientsInRawPizza.Clear();
            enabledIngredientsInRawPizza = new List<PizzaIngredients>();
        }
    }

	public GameObject Score; 
	public GameObject ObjOven;

	public void AddIngredientCounter(string ingrediente) {

		if (ingrediente == "BaconMedium" || ingrediente == "BaconSmall") {
			Bacon += 1;
			Debug.Log (Bacon);
			if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 1) {
				Score.GetComponent<Score> ().AddModerate();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 2) {
				Score.GetComponent<Score> ().Remove();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 3) {
				Score.GetComponent<Score> ().Remove();
			}
		
		
		}else if (ingrediente == "ChampignonMedium" || ingrediente == "ChampignonSmall") {
			Champignon += 1;
			Debug.Log (Champignon);
			if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 1) {
				Score.GetComponent<Score> ().AddExtra();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 2) {
				Score.GetComponent<Score> ().AddExtra();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 3) {
				Score.GetComponent<Score> ().AddExtra();
			}
		
		
		}else if (ingrediente == "CheeseMedium" || ingrediente == "CheeseSmall") {
			Cheese += 1;
			Debug.Log (Cheese);
			if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 1) {
				Score.GetComponent<Score> ().AddEasy();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 2) {
				Score.GetComponent<Score> ().AddModerate();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 3) {
				Score.GetComponent<Score> ().AddModerate();
			}
		
		
		}else if (ingrediente == "OliveMedium" || ingrediente == "OliveSmall") {
			Olive += 1;
			Debug.Log (Olive);
			if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 1) {
				Score.GetComponent<Score> ().AddExtra();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 2) {
				Score.GetComponent<Score> ().AddExtra();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 3) {
				Score.GetComponent<Score> ().AddModerate();
			}
		
		
		}else if (ingrediente == "OnionMedium" || ingrediente == "OnionSmall") {
			Onion += 1;
			Debug.Log (Onion);
			if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 1) {
				Score.GetComponent<Score> ().AddExtra();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 2) {
				Score.GetComponent<Score> ().AddExtra();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 3) {
				Score.GetComponent<Score> ().Remove();
			}
		
		
		}else if (ingrediente == "PepperoniMedium" || ingrediente == "PepperoniSmall") {
			Pepperoni += 1;
			Debug.Log (Pepperoni);
			if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 1) {
				Score.GetComponent<Score> ().AddExtra();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 2) {
				Score.GetComponent<Score> ().Remove();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 3) {
				Score.GetComponent<Score> ().AddEasy();
			}
		
		
		}else if (ingrediente == "RedPepperMedium" || ingrediente == "RedPepperSmall") {
			RedPepper += 1;
			Debug.Log (RedPepper);
			if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 1) {
				Score.GetComponent<Score> ().AddModerate();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 2) {
				Score.GetComponent<Score> ().AddExtra();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 3) {
				Score.GetComponent<Score> ().AddExtra();
			}
		
		
		}else if (ingrediente == "ShrimpMedium" || ingrediente == "ShrimpSmall") {
			Shrimp += 1;
			Debug.Log (Shrimp);
			if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 1) {
				Score.GetComponent<Score> ().AddExtra();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 2) {
				Score.GetComponent<Score> ().AddHard();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 3) {
				Score.GetComponent<Score> ().AddHard();
			}
		
		
		}else if (ingrediente == "TomatoMedium" || ingrediente == "TomatoSmall") {
			Tomato += 1;
			Debug.Log (Tomato);
		
			if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 1) {
				Score.GetComponent<Score> ().AddExtra();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 2) {
				Score.GetComponent<Score> ().AddExtra();
			}else if (ObjOven.GetComponent<OvenCollider> ().levelAtual == 3) {
				Score.GetComponent<Score> ().AddModerate();
			}
		}
	}
    public void EnableIngredientInPizza(PizzaIngredients ingredient)
	{
        ClearCuttingTable();
        pizzaIngredientsDictionary[ingredient].prefab.SetActive(true);
        enabledIngredientsInRawPizza.Add(ingredient);
		Debug.Log (ingredient);
  		Debug.Log("*********************");
  		ingredienteadd = ingredient.ToString();
  		AddIngredientCounter(ingredienteadd);
    
    }

	public void EnableIngredientOnTable(PizzaIngredients ingredient)
	{
        ClearCuttingTable();
        onTheTable = tableIngredientsDictionary[ingredient];
        onTheTable.prefab.SetActive(true);
        PizzaGameController.Instance.ChangePizzaIngredientTo(ingredient);
    }

    public void IngredientHasBeenCutted()
    {
        int ingredientToInt = (int)onTheTable.ingredient;
        ingredientToInt++;
        EnableIngredientOnTable((PizzaIngredients)ingredientToInt);
    }

	public void zerar(){
		Bacon = 0;Champignon = 0;Cheese = 1;Olive = 0;Onion = 0;Pepperoni = 0;RedPepper = 0;Shrimp = 0;Tomato = 0;	
	}
}
